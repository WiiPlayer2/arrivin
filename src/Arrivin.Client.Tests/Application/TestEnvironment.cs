using Arrivin.Client.Application;
using Arrivin.Client.Domain;
using Arrivin.Domain;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Moq;

namespace Arrivin.Client.Tests.Application;

internal class TestEnvironment<RT> where RT : struct, HasCancel<RT>
{
    public TestEnvironment()
    {
        ApiClient
            .Setup(x => x.GetDeployment(It.IsAny<ApiUrl>(), It.IsAny<DeploymentName>()))
            .Returns(Prelude.SuccessAff(Option<DeploymentInfo>.None));
        ApiClient
            .Setup(x => x.SetDeployment(It.IsAny<ApiUrl>(), It.IsAny<DeploymentName>(), It.IsAny<DeploymentInfo>()))
            .Returns(Prelude.unitAff);

        Nix
            .Setup(x => x.CopyFrom(It.IsAny<StoreUrl>(), It.IsAny<StorePath>()))
            .Returns(Prelude.unitAff);

        FileSystem
            .Setup(x => x.GetFileEntry(It.IsAny<FilePath>()))
            .Returns((FilePath path) => Prelude.SuccessAff(FileSystemEntries.TryGetValue(path)));
        FileSystem
            .Setup(x => x.SetFileEntry(It.IsAny<FilePath>(), It.IsAny<FileEntry>()))
            .Returns((FilePath path, FileEntry entry) =>
            {
                FileSystemEntries[path] = entry;
                return Prelude.unitAff;
            });
        FileSystem
            .Setup(x => x.RemoveFileEntry(It.IsAny<FilePath>()))
            .Returns((FilePath path) =>
            {
                FileSystemEntries.Remove(path);
                return Prelude.unitAff;
            });

        Cli
            .Setup(x => x.Call(It.IsAny<StorePath>()))
            .Returns(Prelude.unitAff);
    }

    public Mock<IApiClient<RT>> ApiClient { get; } = new();

    public Mock<ICli<RT>> Cli { get; } = new();

    public Mock<IFileSystem<RT>> FileSystem { get; } = new();

    public IDictionary<FilePath, FileEntry> FileSystemEntries { get; } = new Dictionary<FilePath, FileEntry>();

    public Mock<INix<RT>> Nix { get; } = new();
}
