using ArcExplorer.ViewModels;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using System.Collections;

namespace ArcExplorer.UserControls
{
    public partial class FileTreeView : UserControl
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

        public static readonly DirectProperty<FileTreeView, FileGridItem?> SelectedItemProperty =
            AvaloniaProperty.RegisterDirect<FileTreeView, FileGridItem?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly DirectProperty<FileTreeView, int> SelectedIndexProperty =
            AvaloniaProperty.RegisterDirect<FileTreeView, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public FileGridItem? SelectedItem
        {
            get => selectedItem;
            set => SetAndRaise(SelectedItemProperty, ref selectedItem, value);
        }
        private FileGridItem? selectedItem;

        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetAndRaise(SelectedIndexProperty, ref selectedIndex, value);
        }
        private int selectedIndex;

        public DataGrid FileGrid { get; }

        public FileTreeView()
        {
            InitializeComponent();
            FileGrid = fileGrid;
        }

        public void ExtractFile()
        {
            SelectedItem?.Node?.OnFileExtracting();
        }

        public void OpenParentFolder()
        {
            SelectedItem?.Node?.OnOpeningParentFolder();
        }
    }
}
