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
        private IEnumerable items = new AvaloniaList<FileNodeBase>();

        public static readonly DirectProperty<FileTreeView, FileNodeBase?> SelectedItemProperty =
            AvaloniaProperty.RegisterDirect<FileTreeView, FileNodeBase?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v, 
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public FileNodeBase? SelectedItem
        {
            get => selectedItem;
            set => SetAndRaise(SelectedItemProperty, ref selectedItem, value);
        }
        private FileNodeBase? selectedItem;

        public FileTreeView()
        {
            InitializeComponent();
            this.FindControl<ListBox>("fileListBox").DoubleTapped += FileTreeView_DoubleTapped;
        }

        private void FileTreeView_DoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (SelectedItem is FolderNode folder)
            {
                // TODO: This is a confusing name.
                folder.OnLoadingChildren();
            }
        }


        public void ExtractFile()
        {
            SelectedItem?.OnFileExtracting();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
