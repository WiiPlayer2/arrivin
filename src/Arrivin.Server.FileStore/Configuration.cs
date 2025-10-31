using System.ComponentModel.DataAnnotations;

namespace Arrivin.Server.FileStore;

public record Configuration
{
    public const string KEY = "FileStore";
    
    [Required] public required string Path  { get; init; }
}
