using System.Text.RegularExpressions;
using NRMS.Application.Abstractions;

namespace NRMS.Infrastructure.Storage;

public sealed class FileSystemEvidenceStorage : IEvidenceStorage
{
    private readonly string _root;

    public FileSystemEvidenceStorage(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Root directory is required.", nameof(rootDirectory));

        _root = rootDirectory;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Guid evidenceId, string originalFileName, Stream content, CancellationToken ct = default)
    {
        if (evidenceId == Guid.Empty) throw new ArgumentException("EvidenceId is required.", nameof(evidenceId));
        if (string.IsNullOrWhiteSpace(originalFileName)) throw new ArgumentException("File name is required.", nameof(originalFileName));
        if (content is null) throw new ArgumentNullException(nameof(content));

        var safeName = SanitizeFileName(originalFileName.Trim());
        var fileName = $"EVID-{evidenceId:N}_{safeName}";
        var fullPath = Path.Combine(_root, fileName);

        if (content.CanSeek) content.Position = 0;

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct).ConfigureAwait(false);

        return fileName;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = new string(Path.GetInvalidFileNameChars());
        var invalidRegex = new Regex($"[{Regex.Escape(invalid)}]", RegexOptions.Compiled);
        var cleaned = invalidRegex.Replace(name, "_");
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "evidence.bin" : cleaned;
    }
}
