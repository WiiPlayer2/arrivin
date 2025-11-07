using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class DeployDeployment<RT>(
    PullDeployment<RT> pullDeployment,
    GetDeployment<RT> getDeployment,
    INix<RT> nix,
    ICli<RT> cli,
    IFileSystem<RT> fileSystem
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> With(ServerUrl serverUrl, FilePath dataDirectory, DeploymentName name, NixArgs extraBuildArgs, Option<StoreUrl> useStoreOption = default) =>
        from deploymentInfo in getDeployment.For(serverUrl, name)
            .Bind(x => x.ToEff())
        from links in ReadLinks(dataDirectory, name)
        let needsPulling = NeedsPulling(deploymentInfo, links)
        from _10 in needsPulling
            ? from _05 in PullDeployment(dataDirectory, serverUrl, name, useStoreOption)
              from outPath in ProduceOutput(dataDirectory, name, deploymentInfo, extraBuildArgs)
              from _10 in Activate(dataDirectory, name, deploymentInfo.Derivation, outPath)
              select unit
            : unitAff
        select unit;

    private Aff<RT, Unit> Activate(FilePath dataDirectory, DeploymentName name, StorePath derivation, StorePath activatablePath) =>
        from lastActivatedPathOption in GetRoot(dataDirectory, FileName.From($"{name}.out"))
        let mustActivate = lastActivatedPathOption
            .Map(lastActivatedPath => lastActivatedPath != activatablePath)
            .IfNone(true)
        from _05 in mustActivate
            ? from activationPath in SuccessEff(StorePath.From(Path.Join(activatablePath.Value, "arrivin-activate")))
              from _10 in cli.Call(activatablePath)
              from _20 in AddRoot(dataDirectory, FileName.From($"{name}.out"), activatablePath)
              from _30 in AddRoot(dataDirectory, FileName.From($"{name}.drv"), derivation)
              from _40 in RemoveRoot(dataDirectory, FileName.From($"{name}-current.out"))
              from _50 in RemoveRoot(dataDirectory, FileName.From($"{name}-current.drv"))
              select unit
            : unitAff
        select unit;

    private Aff<RT, Unit> AddRoot(FilePath dataDir, FileName linkName, StorePath storePath) =>
        fileSystem.SetFileEntry(GetLinkPath(dataDir, linkName), FileEntry.Symlink(FilePath.From(storePath.Value)));

    private static FilePath GetLinkPath(FilePath dataDir, FileName linkName) => dataDir
        .Join(FileName.From("gcroots"))
        .Join(FileName.From("deployments"))
        .Join(linkName);

    private Aff<RT, Option<StorePath>> GetRoot(FilePath dataDir, FileName linkName) =>
        from fileEntryOption in fileSystem.GetFileEntry(GetLinkPath(dataDir, linkName))
        from linkTargetOption in fileEntryOption.Match(
            fileEntry => fileEntry.Match(symlink => SuccessEff(Some(StorePath.From(symlink.Path.Value)))
            ),
            () => SuccessEff(Option<StorePath>.None)
        )
        select linkTargetOption;

    private static bool NeedsPulling(DeploymentInfo deploymentInfo, Links links) =>
        deploymentInfo.OutPath.Match(
            currentOutPath => links.OutPath.Match(
                oldOutPath => currentOutPath != oldOutPath,
                () => true
            ),
            () => links.Derivation.Match(
                oldDerivation => deploymentInfo.Derivation != oldDerivation,
                () => true
            )
        );

    private Aff<RT, StorePath> ProduceOutput(FilePath dataDirectory, DeploymentName name, DeploymentInfo deploymentInfo, NixArgs extraBuildArgs) =>
        from outPath in deploymentInfo.OutPath.Match(
            outPath => SuccessAff(outPath),
            nix.Build(deploymentInfo.Derivation, extraBuildArgs))
        from _10 in AddRoot(dataDirectory, FileName.From($"{name}-current.out"), outPath)
        select outPath;

    private Aff<RT, DeploymentInfo> PullDeployment(FilePath dataDirectory, ServerUrl serverUrl, DeploymentName name, Option<StoreUrl> useStoreOption) =>
        from deploymentInfo in pullDeployment.With(serverUrl, name, useStoreOption)
        from _10 in deploymentInfo.OutPath
            .Map<Aff<RT, Unit>>(_ => unitAff)
            .IfNone(() => AddRoot(dataDirectory, FileName.From($"{name}-current.drv"), deploymentInfo.Derivation))
        select deploymentInfo;

    private Aff<RT, Links> ReadLinks(FilePath dataDirectory, DeploymentName name) =>
        from derivationOption in GetRoot(dataDirectory, FileName.From($"{name}.drv"))
        from outPathOption in GetRoot(dataDirectory, FileName.From($"{name}.out"))
        select new Links(derivationOption, outPathOption);

    private Aff<RT, Unit> RemoveRoot(FilePath dataDir, FileName linkName) =>
        fileSystem.RemoveFileEntry(GetLinkPath(dataDir, linkName));

    private record Links(Option<StorePath> Derivation, Option<StorePath> OutPath);
}
