using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;

namespace Arrivin.Client.World;

internal class FileSystem<RT>(ILogger<FileSystem<RT>> logger) : IFileSystem<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Option<FileEntry>> GetFileEntry(FilePath path) =>
        from _10 in Eff(fun(() => logger.LogTrace("Getting file entry \"{path}\"...", path)))
        from pathExists in SuccessEff(Path.Exists(path.Value))
        from fileEntry in pathExists
            ? (
                from fileInfo in SuccessEff(new FileInfo(path.Value))
                from fileEntry in fileInfo.LinkTarget is null
                    ? FailEff<FileEntry>("Only symlinks are supported currently")
                    : SuccessEff(FileEntry.Symlink(FilePath.From(fileInfo.LinkTarget)))
                select fileEntry
            ).Map(Some)
            : SuccessEff(Option<FileEntry>.None)
        select fileEntry;

    public Aff<RT, Unit> RemoveFileEntry(FilePath path) =>
        from _05 in Eff(fun(() => logger.LogTrace("Removing file entry \"{path}\"...", path)))
        from _10 in File.Exists(path.Value) ? Eff(fun(() => File.Delete(path.Value))) : unitEff
        from _20 in Directory.Exists(path.Value) ? Eff(fun(() => Directory.Delete(path.Value, true))) : unitEff
        select unit;

    public Aff<RT, Unit> SetFileEntry(FilePath path, FileEntry entry) =>
        from _05 in Eff(fun(() => logger.LogTrace("Setting file entry \"{path}\" to \"{entry}\"...", path, entry)))
        from directory in Eff(() => Path.GetDirectoryName(path.Value))
        from _10 in Eff(() => Directory.CreateDirectory(directory))
        from _15 in RemoveFileEntry(path)
        from _20 in entry.Match(symlink => Eff(() => Directory.CreateSymbolicLink(path.Value, symlink.Path.Value)))
        select unit;
}
