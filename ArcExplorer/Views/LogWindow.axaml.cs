using ArcExplorer.Logging;
using ArcExplorer.ViewModels;
using ReactiveUI.Avalonia;

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
