using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;
using System.Collections.ObjectModel;

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
        private IEnumerable items = new ObservableCollection<string>();

        public LogWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
