using ArcExplorer.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ArcExplorer.Views
{
    public class HashUpdateDialog : ReactiveWindow<HashUpdateDialogViewModel>
    {
        public HashUpdateDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ConnectClick()
        {
            if (ViewModel != null)
                ViewModel.WasCancelled = false;

            Close();
        }

        public void CancelClick()
        {
            if (ViewModel != null)
                ViewModel.WasCancelled = true;

            Close();
        }
    }
}
