using ArcExplorer.Logging;
using ArcExplorer.ViewModels;
using Avalonia.ReactiveUI;

namespace ArcExplorer.Views
{
    public partial class LogWindow : ReactiveWindow<LogWindowViewModel>
    {
        public LogWindow()
        {
            InitializeComponent();
            ViewModel = new LogWindowViewModel { Items = ApplicationSink.Instance.Value.LogMessages };
        }
    }
}
