using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;

namespace ArcExplorer.Views
{
    public class LogWindow : Window
    {
        public static readonly DirectProperty<LogWindow, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<LogWindow, IEnumerable>(
            nameof(Items),
            o => o.Items,
            (o, v) => o.Items = v);

        public IEnumerable Items
        {
            get => items;
            set => SetAndRaise(ItemsProperty, ref items, value);
        }
        private IEnumerable items = new AvaloniaList<string>();

        public LogWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
