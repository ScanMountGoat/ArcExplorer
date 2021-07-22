using ArcExplorer.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ArcExplorer.Views
{
    public class OpenArcConnectionWindow : ReactiveWindow<OpenArcConnectionWindowViewModel>
    {
        public OpenArcConnectionWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
