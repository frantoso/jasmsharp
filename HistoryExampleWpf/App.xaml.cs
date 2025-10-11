using System.Configuration;
using System.Data;
using System.Windows;

namespace HistoryExampleWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    ///     Gets the view model instance.
    /// </summary>
    public static ViewModel.ViewModel ViewModel { get; } = new ViewModel.ViewModel();
}