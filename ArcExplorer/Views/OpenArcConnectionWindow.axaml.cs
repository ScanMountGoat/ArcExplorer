using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArcExplorer.Views
{
    public class OpenArcConnectionWindow : Window
    {
        public OpenArcConnectionWindow()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        public bool WasCancelled { get; private set; } = true;

        public string IpAddress { get; set; } = "000.000.000.000";

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ConnectClick()
        {
            WasCancelled = false;
            Close();
        }

        public void CancelClick()
        {
            WasCancelled = true;
            Close();
        }
    }
}
