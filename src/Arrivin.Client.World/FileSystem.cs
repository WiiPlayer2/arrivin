using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;

namespace Arrivin.Client.World;

internal class FileSystem<RT> : IFileSystem<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Option<FileEntry>> GetFileEntry(FilePath path) =>
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
        from _10 in Eff(fun(() => File.Delete(path.Value)))
        from _20 in Eff(fun(() => Directory.Delete(path.Value, true)))
        select unit;

    public Aff<RT, Unit> SetFileEntry(FilePath path, FileEntry entry) =>
        from directory in Eff(() => Path.GetDirectoryName(path.Value))
        from _10 in Eff(() => Directory.CreateDirectory(directory))
        from _15 in RemoveFileEntry(path)
        from _20 in entry.Match(symlink => Eff(() => Directory.CreateSymbolicLink(path.Value, symlink.Path.Value)))
        select unit;
}
