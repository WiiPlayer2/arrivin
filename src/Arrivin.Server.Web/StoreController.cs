using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CliWrap;
using CliWrap.Buffered;
using EasyCompressor;
using LanguageExt;
using LanguageExt.UnitsOfMeasure;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;
using IOFile = System.IO.File;
using static LanguageExt.Prelude;

namespace Arrivin.Server.Web;

[Route("store")]
[Controller]
public class StoreController(IConfiguration configuration, ILogger<StoreController> logger) : ControllerBase
{
    private static readonly DirectoryInfo TMP_DOWNLOAD_PATH = Directory.CreateTempSubdirectory("arrivind");

    private readonly string cachePath = configuration.GetValue<string>("NarCachePath")!;

    private static readonly Encoding UTF8 = new UTF8Encoding(false);

    [Route("nar/{hash}.nar.zst")]
    [HttpHead]
    public async Task<IResult> GetCompressedNar(string hash)
    {
        var narLinkPath = Path.Join(cachePath, "links", hash);
        if (!IOFile.Exists(narLinkPath))
            return Results.NotFound();

        var linkTarget = new FileInfo(narLinkPath).ResolveLinkTarget(false);
        if (linkTarget?.Exists ?? false) return Results.Ok();

        IOFile.Delete(narLinkPath);
        return Results.NotFound();
    }

    [Route("nar/{hash}.nar")]
    [HttpGet]
    public async Task<IResult> GetNar(string hash, CancellationToken cancellationToken)
    {
        var cliResult = await Cli.Wrap("nix")
            .WithArguments(builder => builder
                .Add(["--experimental-features", "nix-command"])
                .Add(["store", "path-from-hash-part", hash])
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(cancellationToken);

        if (!cliResult.IsSuccess) return Results.NotFound();
        var storePath = cliResult.StandardOutput.Trim();

        var outputStream = new MemoryStream();
        await Cli.Wrap("nix")
            .WithArguments(builder => builder
                .Add(["--experimental-features", "nix-command"])
                .Add(["nar", "dump-path", storePath])
            )
            .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
            .ExecuteAsync(cancellationToken);
        outputStream.Position = 0;

        return Results.Stream(outputStream);
    }

    [Route("{hash}.narinfo")]
    [HttpGet, HttpHead]
    public async Task<IResult> GetNarInfo([FromRoute] string hash, CancellationToken cancellationToken)
    {
        var cliResult = await Cli.Wrap("nix")
            .WithArguments(builder => builder
                .Add(["--experimental-features", "nix-command"])
                .Add(["store", "path-from-hash-part", hash])
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(cancellationToken);

        if (!string.IsNullOrEmpty(cliResult.StandardError))
            logger.LogError("nix path-from-hash-part: {error}", cliResult.StandardError);
        logger.LogTrace("nix path-from-hash-part: {output}", cliResult.StandardOutput);

        if (!cliResult.IsSuccess)
            return Results.NotFound();

        var storePath = cliResult.StandardOutput.Trim();
        var pathInfo = await ResolvePathInfo(storePath, cancellationToken) ?? throw new InvalidOperationException();

        var narInfoBuilder = new StringBuilder();
        narInfoBuilder.AppendLine($"""
                                   StorePath: {storePath}
                                   URL: nar/{hash}.nar
                                   Compression: none
                                   NarHash: {pathInfo.NarHash}
                                   NarSize: {pathInfo.NarSize}
                                   """);

        if (pathInfo.References.Length > 0)
            narInfoBuilder.AppendLine($"References: {string.Join(" ", pathInfo.References.Select(StripPath))}");

        if (pathInfo.Deriver is not null)
            narInfoBuilder.AppendLine($"Deriver: {StripPath(pathInfo.Deriver)}");

        if (pathInfo.Signatures is not null)
            foreach (var signature in pathInfo.Signatures)
                narInfoBuilder.AppendLine($"Sig: {signature}");

        var narInfo = narInfoBuilder.ToString();
        return Results.Text(narInfo);
    }

    [Route("nix-cache-info")]
    public IResult GetNixCacheInfo() => Results.Ok("""
                                                   StoreDir: /nix/store
                                                   WantMassQuery: 1
                                                   Priority: 40
                                                   """);

    [Route("nar/{hash}.nar{compressionExtension}")]
    [HttpPut]
    [DisableRequestSizeLimit]
    public async Task<IResult> PutCompressedNar(string hash, string compressionExtension, CancellationToken cancellationToken)
    {
        var downloadPath = Path.Join(TMP_DOWNLOAD_PATH.FullName, $"{hash}.nar{compressionExtension}");
        await using var fileStream = IOFile.Create(downloadPath);
        await HttpContext.Request.Body.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
        fileStream.Close();

        return Results.Ok();
    }

    [Route("{hash}.narinfo")]
    [HttpPut]
    public async Task<IResult> PutNarInfo(string hash, CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await HttpContext.Request.Body.CopyToAsync(buffer, cancellationToken);
        await buffer.FlushAsync(cancellationToken);

        var text = Encoding.UTF8.GetString(buffer.ToArray());
        logger.LogTrace("nar info:\n{narInfo}", text);

        string storePath = default!;
        string fileHash = default!;
        var references = Array<string>();
        string? deriver = default;
        var compression = "none";
        foreach (var line in text.Split('\n'))
        {
            var splitIndex = line.IndexOf(':');
            if (splitIndex == -1)
                continue;

            var propertyName = line[..splitIndex];
            var propertyValue = line[(splitIndex + 2)..];
            switch (propertyName)
            {
                case "StorePath":
                    storePath = propertyValue;
                    break;

                case "FileHash":
                    fileHash = propertyValue;
                    break;

                case "References":
                    references = propertyValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    break;

                case "Deriver":
                    deriver = propertyValue;
                    break;

                case "Compression":
                    compression = propertyValue.ToLowerInvariant();
                    break;
            }
        }

        var narHash = fileHash[7..];
        var (compressionExtension, decompress) = ResolveCompression(compression);
        var narPath = Path.Join(TMP_DOWNLOAD_PATH.FullName, $"{narHash}.nar{compressionExtension}");
        var pathInfo = await ResolvePathInfo(storePath, cancellationToken);
        if (pathInfo is null)
        {
            logger.LogDebug("Importing NAR into {storePath}", storePath);

            using var importStream = new MemoryStream();
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            await WriteStoreImportPackage(importStream).RunUnit();
            await importStream.FlushAsync(cancellationToken);
            importStream.Position = 0;
            
            try
            {
                await CliStoreImport(importStream);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to import. Repairing path and trying again...");
                await Cli.Wrap("nix-store")
                    .WithArguments(builder => builder
                        .Add(["--repair-path", storePath]))
                    .ExecuteBufferedAsync(cancellationToken);
                await CliStoreImport(importStream);
            }
        }

        IOFile.Delete(narPath);
        try
        {
            Directory.CreateDirectory(Path.Join(cachePath, "links"));
            var symlink = new FileInfo(Path.Join(cachePath, "links", narHash));
            if(symlink.Exists)
                symlink.Delete();
            symlink.CreateAsSymbolicLink(storePath);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to create symlink for {narHash}.", narHash);
        }

        return Results.Ok();

        async Task CliStoreImport(MemoryStream importStream) =>
            await Cli.Wrap("nix-store")
                .WithArguments(builder => builder
                    .Add(["--import"]))
                .WithStandardInputPipe(PipeSource.FromStream(importStream))
                .ExecuteBufferedAsync(cancellationToken);

        Aff<Unit> WriteStoreImportPackage(Stream stream) =>
            from _10 in WriteNarLong2(stream, 1L, cancellationToken)
            from _20 in use(
                retry(
                    Schedule.repeat(5) | Schedule.exponential(1.Seconds()),
                    Eff(() => IOFile.Open(narPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                ),
                compressedNar => Aff(() => decompress(compressedNar, stream, cancellationToken).ToUnit().ToValue())
            )
            from _30 in Aff(() => stream.WriteAsync("NIXE\0\0\0\0"u8.ToArray(), cancellationToken).ToUnit())
            from _40 in WriteNixString2(stream, storePath, cancellationToken)
            from _50 in WriteNarLong2(stream, references.Length, cancellationToken)
            from _60 in references
                .Select(reference => WriteNixString2(stream, PrintPath(reference), cancellationToken))
                .TraverseSerial(identity)
            from _70 in WriteNixString2(stream, deriver is null ? string.Empty : PrintPath(deriver), cancellationToken)
            from _80 in WriteNarLong2(stream, 0L, cancellationToken)
            from _90 in WriteNarLong2(stream, 0L, cancellationToken)
            select unit;
    }

    private string PrintPath(string path) => $"/nix/store/{path}";

    private (string Extension, Func<Stream, Stream, CancellationToken, Task> Decompress) ResolveCompression(string compression)
    {
        return compression switch
        {
            "none" => (string.Empty, None),
            "zstd" => (".zst", Zstd),
            "xz" => (".xz", Xz),
        };

        static Task None(Stream input, Stream output, CancellationToken cancellationToken) => input.CopyToAsync(output, cancellationToken);

        static async Task Zstd(Stream input, Stream output, CancellationToken cancellationToken)
        {
            var compressor = new ZstdSharpCompressor();
            await compressor.DecompressAsync(input, output, cancellationToken);
        }

        static async Task Xz(Stream input, Stream output, CancellationToken cancellationToken)
        {
            await Cli.Wrap("xz")
                .WithArguments(["-d", "-c"])
                .WithStandardInputPipe(PipeSource.FromStream(input))
                .WithStandardOutputPipe(PipeTarget.ToStream(output))
                .ExecuteAsync(cancellationToken);
        }
    }

    private async Task<PathInfo?> ResolvePathInfo(string storePath, CancellationToken cancellationToken)
    {
        var cliResult2 = await Cli.Wrap("nix")
            .WithArguments(builder => builder
                .Add(["--experimental-features", "nix-command"])
                .Add(["path-info", "--json", storePath])
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(cancellationToken);

        if (!string.IsNullOrEmpty(cliResult2.StandardError))
            logger.LogTrace("nix path-info: {error}", cliResult2.StandardError);
        logger.LogTrace("nix path-info: {output}", cliResult2.StandardOutput);

        if (!cliResult2.IsSuccess) return null;

        var pathInfo = ReadPathInfo(cliResult2.StandardOutput);
        return pathInfo;

        PathInfo ReadPathInfo(string json)
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Array => jsonElement.Deserialize<PathInfo[]>()!.First(),
                JsonValueKind.Object => jsonElement.Deserialize<Dictionary<string, PathInfo>>()!.Values.First(),
            };
        }
    }

    private string StripPath(string path) => path["/nix/store/".Length..];

    private async Task WriteNarLong(Stream stream, long value, CancellationToken cancellationToken)
    {
        var valueBytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
            System.Array.Reverse(valueBytes);

        await stream.WriteAsync(valueBytes, cancellationToken);
    }

    private Aff<Unit> WriteNarLong2(Stream stream, long value, CancellationToken cancellationToken) =>
        from valueBytes in SuccessEff((Arr<byte>)BitConverter.GetBytes(value))
        let endianizedValueBytes =
            !BitConverter.IsLittleEndian
                ? valueBytes.Reverse()
                : valueBytes
        from _ in Aff(() => stream.WriteAsync(endianizedValueBytes.ToArray(), cancellationToken).ToUnit())
        select unit;

    private async Task WriteNixString(Stream stream, string value, CancellationToken cancellationToken)
    {
        await WriteNarLong(stream, value.Length, cancellationToken);
        await stream.WriteAsync(Encoding.UTF8.GetBytes(value), cancellationToken);

        var padding = (8 - value.Length % 8) % 8;
        for (var i = 0; i < padding; i++)
            stream.WriteByte(0);
    }

    private Aff<Unit> WriteNixString2(Stream stream, string value, CancellationToken cancellationToken) =>
        from _10 in WriteNarLong2(stream, value.Length, cancellationToken)
        from _20 in Aff(() => stream.WriteAsync(UTF8.GetBytes(value), cancellationToken).ToUnit())
        let padding = (8 - value.Length % 8) % 8
        let padBytes = Enumerable.Repeat<byte>(0, padding).ToArray()
        from _30 in Aff(() => stream.WriteAsync(padBytes, cancellationToken).ToUnit())
        select unit;
    

    private record PathInfo(
        [property: JsonPropertyName("narSize")]
        long NarSize,
        [property: JsonPropertyName("narHash")]
        string NarHash,
        [property: JsonPropertyName("deriver")]
        string? Deriver,
        [property: JsonPropertyName("references")]
        string[] References,
        [property: JsonPropertyName("signatures")]
        string[]? Signatures
    );
}
