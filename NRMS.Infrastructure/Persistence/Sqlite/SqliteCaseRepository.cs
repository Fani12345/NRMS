using Microsoft.Data.Sqlite;
using System.Reflection;
using NRMS.Application.Abstractions;
using NRMS.Domain.Entities;
using NRMS.Domain.Enums;
using NRMS.Domain.Exceptions;
using NRMS.Domain.ValueObjects;

namespace NRMS.Infrastructure.Persistence.Sqlite;

public sealed class SqliteCaseRepository : ICaseRepository
{
    private readonly SqliteDb _db;

    public SqliteCaseRepository(SqliteDb db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _db.EnsureCreated();
    }

    public async Task AddAsync(NrmsCase nrmsCase, CancellationToken ct = default)
    {
        if (nrmsCase is null) throw new ArgumentNullException(nameof(nrmsCase));

        await using var conn = _db.OpenConnection();
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = @"
INSERT INTO Cases (CaseId, CaseReference, CaseType, Status, CreatedBy, CreatedAtUtc)
VALUES (@id, @ref, @type, @status, @by, @at);
";
            cmd.Parameters.AddWithValue("@id", nrmsCase.CaseId.ToString("D"));
            cmd.Parameters.AddWithValue("@ref", nrmsCase.CaseReference);
            cmd.Parameters.AddWithValue("@type", (int)nrmsCase.CaseType);
            cmd.Parameters.AddWithValue("@status", (int)nrmsCase.Status);
            cmd.Parameters.AddWithValue("@by", nrmsCase.CreatedBy);
            cmd.Parameters.AddWithValue("@at", nrmsCase.CreatedAtUtc.ToString("O"));

            await cmd.ExecuteNonQueryAsync(ct);
        }

        foreach (var e in nrmsCase.Evidence)
            await InsertEvidenceAsync(conn, tx, nrmsCase.CaseId, e, ct);

        await tx.CommitAsync(ct);
    }

    public async Task<NrmsCase?> GetAsync(Guid caseId, CancellationToken ct = default)
    {
        if (caseId == Guid.Empty) throw new DomainException("CaseId is required.");

        await using var conn = _db.OpenConnection();

        string? caseReference;
        CaseType caseType;
        CaseStatus status;
        string? createdBy;
        DateTime createdAtUtc;

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT CaseReference, CaseType, Status, CreatedBy, CreatedAtUtc
FROM Cases
WHERE CaseId = @id;
";
            cmd.Parameters.AddWithValue("@id", caseId.ToString("D"));

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            caseReference = reader.GetString(0);
            caseType = (CaseType)reader.GetInt32(1);
            status = (CaseStatus)reader.GetInt32(2);
            createdBy = reader.GetString(3);
            createdAtUtc = DateTime.SpecifyKind(DateTime.Parse(reader.GetString(4)), DateTimeKind.Utc);
        }

        var nrmsCase = new NrmsCase(caseId, caseReference!, caseType, createdBy!, createdAtUtc);
        RestoreStatus(nrmsCase, status);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT EvidenceId, FileName, SourceDescription, ImportedAtUtc, ImportedBy, Sha256, StoredPath
FROM EvidenceItems
WHERE CaseId = @caseId
ORDER BY ImportedAtUtc ASC;
";
            cmd.Parameters.AddWithValue("@caseId", caseId.ToString("D"));

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var evidenceId = Guid.Parse(reader.GetString(0));
                var fileName = reader.GetString(1);
                var sourceDescription = reader.GetString(2);
                var importedAtUtc = DateTime.SpecifyKind(DateTime.Parse(reader.GetString(3)), DateTimeKind.Utc);
                var importedBy = reader.GetString(4);
                var sha256 = new Sha256(reader.GetString(5));
                var storedPath = reader.GetString(6);

                nrmsCase.AddEvidence(new EvidenceItem(
                    evidenceId,
                    fileName,
                    sourceDescription,
                    importedAtUtc,
                    importedBy,
                    sha256,
                    storedPath
                ));
            }
        }

        return nrmsCase;
    }

    public async Task UpdateAsync(NrmsCase nrmsCase, CancellationToken ct = default)
    {
        if (nrmsCase is null) throw new ArgumentNullException(nameof(nrmsCase));

        await using var conn = _db.OpenConnection();
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);

        await using (var existsCmd = conn.CreateCommand())
        {
            existsCmd.Transaction = tx;
            existsCmd.CommandText = "SELECT COUNT(1) FROM Cases WHERE CaseId = @id;";
            existsCmd.Parameters.AddWithValue("@id", nrmsCase.CaseId.ToString("D"));
            var count = Convert.ToInt32(await existsCmd.ExecuteScalarAsync(ct));
            if (count == 0) throw new DomainException("Case not found.");
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = @"
UPDATE Cases
SET CaseReference = @ref,
    CaseType = @type,
    Status = @status,
    CreatedBy = @by,
    CreatedAtUtc = @at
WHERE CaseId = @id;
";
            cmd.Parameters.AddWithValue("@id", nrmsCase.CaseId.ToString("D"));
            cmd.Parameters.AddWithValue("@ref", nrmsCase.CaseReference);
            cmd.Parameters.AddWithValue("@type", (int)nrmsCase.CaseType);
            cmd.Parameters.AddWithValue("@status", (int)nrmsCase.Status);
            cmd.Parameters.AddWithValue("@by", nrmsCase.CreatedBy);
            cmd.Parameters.AddWithValue("@at", nrmsCase.CreatedAtUtc.ToString("O"));

            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using (var del = conn.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM EvidenceItems WHERE CaseId = @caseId;";
            del.Parameters.AddWithValue("@caseId", nrmsCase.CaseId.ToString("D"));
            await del.ExecuteNonQueryAsync(ct);
        }

        foreach (var e in nrmsCase.Evidence)
            await InsertEvidenceAsync(conn, tx, nrmsCase.CaseId, e, ct);

        await tx.CommitAsync(ct);
    }

    private static async Task InsertEvidenceAsync(SqliteConnection conn, SqliteTransaction tx, Guid caseId, EvidenceItem e, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
INSERT INTO EvidenceItems (EvidenceId, CaseId, FileName, SourceDescription, ImportedAtUtc, ImportedBy, Sha256, StoredPath)
VALUES (@eid, @cid, @fn, @sd, @at, @by, @sha, @path);
";
        cmd.Parameters.AddWithValue("@eid", e.EvidenceId.ToString("D"));
        cmd.Parameters.AddWithValue("@cid", caseId.ToString("D"));
        cmd.Parameters.AddWithValue("@fn", e.FileName);
        cmd.Parameters.AddWithValue("@sd", e.SourceDescription);
        cmd.Parameters.AddWithValue("@at", e.ImportedAtUtc.ToString("O"));
        cmd.Parameters.AddWithValue("@by", e.ImportedBy);
        cmd.Parameters.AddWithValue("@sha", e.Sha256.Value);
        cmd.Parameters.AddWithValue("@path", e.StoredPath);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void RestoreStatus(NrmsCase nrmsCase, CaseStatus status)
    {
        var prop = typeof(NrmsCase).GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop is null) return;

        var setMethod = prop.GetSetMethod(nonPublic: true);
        if (setMethod is null) return;

        setMethod.Invoke(nrmsCase, new object[] { status });
    }
}
