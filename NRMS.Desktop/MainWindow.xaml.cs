using Microsoft.Win32;
using NRMS.Application.Contracts;
using NRMS.Application.Contracts.Numbering;
using NRMS.Domain.Enums;
using NRMS.Domain.Numbering;
using System.IO;
using System.Windows;

namespace NRMS.Desktop;

public partial class MainWindow : Window
{
    private Guid _currentCaseId = Guid.Empty;
    private string? _selectedFilePath;

    public MainWindow()
    {
        InitializeComponent();

        // Populate CaseType combo box
        CaseTypeComboBox.ItemsSource = Enum.GetValues(typeof(CaseType)).Cast<CaseType>();
        CaseTypeComboBox.SelectedItem = CaseType.Allocation;

        // Populate NumberType combo box
        NumberTypeComboBox.ItemsSource = Enum.GetValues(typeof(NumberType)).Cast<NumberType>();
        NumberTypeComboBox.SelectedItem = NumberType.Msisdn;

        SetStatus("Ready.");
        RefreshEvidenceGrid(Array.Empty<EvidenceRow>());
        RefreshAuditGrid(Array.Empty<AuditRow>());
        RefreshBlocksGrid(Array.Empty<BlockRow>());
        RefreshAllocationsGrid(Array.Empty<AllocationRow>());

        UpdateButtons();
    }

    private async void CreateCaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var createdBy = (CreatedByTextBox.Text ?? string.Empty).Trim();
            var caseType = (CaseType)(CaseTypeComboBox.SelectedItem ?? CaseType.Allocation);

            var svc = ((App)System.Windows.Application.Current).Services.CaseService;

            var result = await svc.CreateCaseAsync(new CreateCaseCommand(caseType, createdBy));

            _currentCaseId = result.CaseId;
            CaseIdTextBox.Text = result.CaseId.ToString();
            CaseReferenceTextBox.Text = result.CaseReference;

            _selectedFilePath = null;
            SelectedFileTextBox.Text = string.Empty;

            await ReloadEvidenceAsync(); // also reloads audit
            UpdateButtons();

            SetStatus($"Case created: {result.CaseReference}");
        }
        catch (Exception ex)
        {
            SetStatus($"Create case failed: {ex.Message}");
        }
    }

    private async void OpenCaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var caseRef = (OpenCaseReferenceTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(caseRef))
            {
                SetStatus("Enter a Case Reference to open.");
                return;
            }

            var services = ((App)System.Windows.Application.Current).Services;
            var c = await services.CaseRepository.GetByReferenceAsync(caseRef);

            if (c is null)
            {
                SetStatus($"Case not found: {caseRef}");
                return;
            }

            _currentCaseId = c.CaseId;

            CaseIdTextBox.Text = c.CaseId.ToString();
            CaseReferenceTextBox.Text = c.CaseReference;

            _selectedFilePath = null;
            SelectedFileTextBox.Text = string.Empty;

            await ReloadEvidenceAsync(); // also reloads audit
            UpdateButtons();

            SetStatus($"Opened case: {c.CaseReference}");
        }
        catch (Exception ex)
        {
            SetStatus($"Open case failed: {ex.Message}");
        }
    }

    private void PickFileButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select evidence file",
                Filter = "All files (*.*)|*.*"
            };

            var ok = dlg.ShowDialog(this);
            if (ok == true)
            {
                _selectedFilePath = dlg.FileName;
                SelectedFileTextBox.Text = _selectedFilePath;
                SetStatus("File selected.");
            }

            UpdateButtons();
        }
        catch (Exception ex)
        {
            SetStatus($"Pick file failed: {ex.Message}");
        }
    }

    private async void AttachEvidenceButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_currentCaseId == Guid.Empty)
            {
                SetStatus("Create a case first or open an existing case first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                SetStatus("Pick a valid file first.");
                return;
            }

            var importedBy = (CreatedByTextBox.Text ?? string.Empty).Trim();
            var source = (SourceDescriptionTextBox.Text ?? string.Empty).Trim();
            var fileName = Path.GetFileName(_selectedFilePath);

            var svc = ((App)System.Windows.Application.Current).Services.CaseService;

            await using var fs = new FileStream(_selectedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var result = await svc.AttachEvidenceAsync(new AttachEvidenceCommand(
                CaseId: _currentCaseId,
                ImportedBy: importedBy,
                FileName: fileName,
                SourceDescription: source,
                Content: fs
            ));

            _selectedFilePath = null;
            SelectedFileTextBox.Text = string.Empty;

            await ReloadEvidenceAsync(); // also reloads audit
            UpdateButtons();

            SetStatus($"Evidence attached: {result.StoredPath}");
        }
        catch (Exception ex)
        {
            SetStatus($"Attach evidence failed: {ex.Message}");
        }
    }

    private async void ReloadAuditButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ReloadAuditAsync();
            SetStatus("Audit trail reloaded.");
        }
        catch (Exception ex)
        {
            SetStatus($"Reload audit failed: {ex.Message}");
        }
    }

    private async void CreateOperatorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var name = (OperatorNameTextBox.Text ?? string.Empty).Trim();
            var lic = (OperatorLicenseTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                SetStatus("Operator name is required.");
                return;
            }

            var svc = ((App)System.Windows.Application.Current).Services.NumberingService;

            var res = await svc.CreateOperatorAsync(new CreateOperatorCommand(name, string.IsNullOrWhiteSpace(lic) ? null : lic));

            AllocateOperatorIdTextBox.Text = res.OperatorId.ToString();

            SetStatus($"Operator created. OperatorId={res.OperatorId}");
        }
        catch (Exception ex)
        {
            SetStatus($"Create operator failed: {ex.Message}");
        }
    }

    private async void AddBlockButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var numberType = (NumberType)(NumberTypeComboBox.SelectedItem ?? NumberType.Msisdn);

            if (!long.TryParse((BlockStartTextBox.Text ?? string.Empty).Trim(), out var start))
            {
                SetStatus("Block Start must be a valid number.");
                return;
            }

            if (!long.TryParse((BlockEndTextBox.Text ?? string.Empty).Trim(), out var end))
            {
                SetStatus("Block End must be a valid number.");
                return;
            }

            var svc = ((App)System.Windows.Application.Current).Services.NumberingService;

            var res = await svc.AddNumberingBlockAsync(new AddNumberingBlockCommand(
                NumberType: numberType,
                Start: start,
                End: end,
                Notes: "Added from desktop demo"
            ));

            AllocateBlockIdTextBox.Text = res.BlockId.ToString();

            await ReloadNumberingGridsAsync();

            SetStatus($"Block added. BlockId={res.BlockId}");
        }
        catch (Exception ex)
        {
            SetStatus($"Add block failed: {ex.Message}");
        }
    }

    private async void AllocateBlockButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!Guid.TryParse((AllocateOperatorIdTextBox.Text ?? string.Empty).Trim(), out var operatorId))
            {
                SetStatus("Operator Id must be a valid GUID.");
                return;
            }

            if (!Guid.TryParse((AllocateBlockIdTextBox.Text ?? string.Empty).Trim(), out var blockId))
            {
                SetStatus("Block Id must be a valid GUID.");
                return;
            }

            var svc = ((App)System.Windows.Application.Current).Services.NumberingService;

            var res = await svc.AllocateNumberingBlockAsync(new AllocateNumberingBlockCommand(
                BlockId: blockId,
                OperatorId: operatorId,
                ExpiresAtUtc: DateTime.UtcNow.AddDays(30),
                Notes: "Allocated from desktop demo"
            ));

            await ReloadNumberingGridsAsync();

            SetStatus($"Allocated. AllocationId={res.AllocationId}");
        }
        catch (Exception ex)
        {
            SetStatus($"Allocate failed: {ex.Message}");
        }
    }

    private async Task ReloadEvidenceAsync()
    {
        if (_currentCaseId == Guid.Empty)
        {
            RefreshEvidenceGrid(Array.Empty<EvidenceRow>());
            RefreshAuditGrid(Array.Empty<AuditRow>());
            return;
        }

        var services = ((App)System.Windows.Application.Current).Services;
        var c = await services.CaseRepository.GetAsync(_currentCaseId);

        if (c is null)
        {
            RefreshEvidenceGrid(Array.Empty<EvidenceRow>());
            RefreshAuditGrid(Array.Empty<AuditRow>());
            return;
        }

        var rows = c.Evidence.Select(e => new EvidenceRow(
            ImportedAtUtc: e.ImportedAtUtc.ToString("O"),
            FileName: e.FileName,
            Sha256: e.Sha256.Value,
            StoredPath: e.StoredPath
        )).ToList();

        RefreshEvidenceGrid(rows);

        await ReloadAuditAsync();
    }

    private async Task ReloadAuditAsync()
    {
        if (_currentCaseId == Guid.Empty)
        {
            RefreshAuditGrid(Array.Empty<AuditRow>());
            return;
        }

        var services = ((App)System.Windows.Application.Current).Services;
        var events = await services.AuditRepository.GetByCaseAsync(_currentCaseId);

        var rows = events.Select(e => new AuditRow(
            OccurredAtUtc: e.OccurredAtUtc.ToString("O"),
            EventType: e.EventType.ToString(),
            Actor: e.Actor,
            ObjectSummary: $"{e.ObjectType}:{e.ObjectId}"
        )).ToList();

        RefreshAuditGrid(rows);
    }

    private async Task ReloadNumberingGridsAsync()
    {
        var services = ((App)System.Windows.Application.Current).Services;

        // Blocks: show MSISDN blocks for demo (you can change later to selected type)
        var blocks = await services.NumberingBlockRepository.ListByTypeAsync(NumberType.Msisdn);

        var blockRows = blocks.Select(b => new BlockRow(
            BlockId: b.BlockId.ToString(),
            NumberType: b.NumberType.ToString(),
            Range: b.Range.ToString(),
            Status: b.Status.ToString(),
            Notes: b.Notes ?? string.Empty
        )).ToList();

        RefreshBlocksGrid(blockRows);

        // Allocations: aggregate all allocations by scanning known blocks
        var allocRows = new List<AllocationRow>();
        foreach (var b in blocks)
        {
            var allocs = await services.AllocationRepository.ListByBlockAsync(b.BlockId);
            allocRows.AddRange(allocs.Select(a => new AllocationRow(
                AllocationId: a.AllocationId.ToString(),
                BlockId: a.BlockId.ToString(),
                OperatorId: a.OperatorId.ToString(),
                AllocatedAtUtc: a.AllocatedAtUtc.ToString("O"),
                ExpiresAtUtc: a.ExpiresAtUtc?.ToString("O") ?? string.Empty,
                Notes: a.Notes ?? string.Empty
            )));
        }

        RefreshAllocationsGrid(allocRows);
    }

    private void RefreshEvidenceGrid(IEnumerable<EvidenceRow> rows)
    {
        EvidenceDataGrid.ItemsSource = rows.ToList();
    }

    private void RefreshAuditGrid(IEnumerable<AuditRow> rows)
    {
        AuditDataGrid.ItemsSource = rows.ToList();
    }

    private void RefreshBlocksGrid(IEnumerable<BlockRow> rows)
    {
        BlocksDataGrid.ItemsSource = rows.ToList();
    }

    private void RefreshAllocationsGrid(IEnumerable<AllocationRow> rows)
    {
        AllocationsDataGrid.ItemsSource = rows.ToList();
    }

    private void UpdateButtons()
    {
        CreateCaseButton.IsEnabled = true;
        OpenCaseButton.IsEnabled = true;

        PickFileButton.IsEnabled = _currentCaseId != Guid.Empty;

        AttachEvidenceButton.IsEnabled =
            _currentCaseId != Guid.Empty &&
            !string.IsNullOrWhiteSpace(_selectedFilePath) &&
            File.Exists(_selectedFilePath);
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private sealed record EvidenceRow(string ImportedAtUtc, string FileName, string Sha256, string StoredPath);
    private sealed record AuditRow(string OccurredAtUtc, string EventType, string Actor, string ObjectSummary);

    private sealed record BlockRow(string BlockId, string NumberType, string Range, string Status, string Notes);

    private sealed record AllocationRow(string AllocationId, string BlockId, string OperatorId, string AllocatedAtUtc, string ExpiresAtUtc, string Notes);
}
