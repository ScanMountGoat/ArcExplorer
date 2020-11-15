using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArcExplorer.Views
{
    public class OpenArcConnectionWindow : Window
    {
        public OpenArcConnectionWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ConnectClick()
        {
            // TODO: Try and establish a connection using the given IP Address.
        }

        private void CancelClick()
        {
            Close();
        }
    }
}
