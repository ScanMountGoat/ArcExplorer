using ArcExplorer.Models;
using ArcExplorer.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArcExplorer.Views
{
    public class OpenArcConnectionWindow : Window
    {
        public OpenArcConnectionWindow()
        {
            InitializeComponent();
            DataContext = this;
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
