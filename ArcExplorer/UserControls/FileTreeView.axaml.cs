using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ArcExplorer.ViewModels;
using System.Collections;

namespace ArcExplorer.UserControls
{
    public class FileTreeView : UserControl
    {
        public static readonly DirectProperty<FileTreeView, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<FileTreeView, IEnumerable>(
            nameof(Items),
            o => o.Items,
            (o, v) => o.Items = v);

        public IEnumerable Items
        {
            get => items;
            set => SetAndRaise(ItemsProperty, ref items, value);
        }
        private IEnumerable items = new AvaloniaList<FileGridItem>();

        public static readonly DirectProperty<FileTreeView, int> SelectedIndexProperty =
            AvaloniaProperty.RegisterDirect<FileTreeView, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v, 
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetAndRaise(SelectedIndexProperty, ref selectedIndex, value);
        }
        private int selectedIndex;

        public FileTreeView()
        {
            InitializeComponent();
        }

        public void ExtractFile()
        {
            // TODO: Fix file extraction.
            //SelectedItem?.OnFileExtracting();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
