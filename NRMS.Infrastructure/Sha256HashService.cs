using System.Security.Cryptography;
using NRMS.Application.Abstractions;

namespace NRMS.Infrastructure;

public sealed class Sha256HashService : IHashService
{
    public async Task<string> ComputeSha256Async(Stream content, CancellationToken ct = default)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));

        if (content.CanSeek) content.Position = 0;

        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(content, ct).ConfigureAwait(false);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
