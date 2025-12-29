using ArcExplorer.ViewModels;
using ReactiveUI.Avalonia;

namespace ArcExplorer.Views
{
    public partial class OpenArcConnectionWindow : ReactiveWindow<OpenArcConnectionWindowViewModel>
    {
        public OpenArcConnectionWindow()
        {
            InitializeComponent();
        }

        public void ConnectClick()
        {
            if (DataContext is OpenArcConnectionWindowViewModel vm)
                vm.WasCancelled = false;

            Close();
        }

        public void CancelClick()
        {
            if (DataContext is OpenArcConnectionWindowViewModel vm)
                vm.WasCancelled = true;

            Close();
        }
    }
}
