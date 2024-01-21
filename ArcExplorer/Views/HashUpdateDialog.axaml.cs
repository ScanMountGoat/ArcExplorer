using ArcExplorer.ViewModels;
using Avalonia.ReactiveUI;

namespace ArcExplorer.Views
{
    public partial class HashUpdateDialog : ReactiveWindow<HashUpdateDialogViewModel>
    {
        public HashUpdateDialog()
        {
            InitializeComponent();
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
