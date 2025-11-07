using FunicularSwitch.Generators;
using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface IFileSystem<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Option<FileEntry>> GetFileEntry(FilePath path);

    Aff<RT, Unit> RemoveFileEntry(FilePath path);

    Aff<RT, Unit> SetFileEntry(FilePath path, FileEntry entry);
}

[UnionType]
public abstract partial record FileEntry
{
    // public record Directory(IDirectory<RT> Value) : FileEntry<RT>;

    // public record File(IDirectory<RT> Value) : FileEntry<RT>;

    public record Symlink_(FilePath Path) : FileEntry
    {
        public override string ToString() => $"-> {Path}";
    }
}

// public interface IDirectory<RT> { }
