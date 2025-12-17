using NRMS.Desktop.Services;

namespace NRMS.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public AppServices Services { get; } = new AppServices();
}
