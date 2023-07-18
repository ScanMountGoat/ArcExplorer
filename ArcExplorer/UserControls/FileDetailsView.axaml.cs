using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ArcExplorer.ViewModels;

namespace ArcExplorer.UserControls
{
    public partial class FileDetailsView : UserControl
    {
        public static readonly DirectProperty<FileDetailsView, FileNodeBase?> FileProperty =
            AvaloniaProperty.RegisterDirect<FileDetailsView, FileNodeBase?>(
            nameof(File),
            o => o.File,
            (o, v) => o.File = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public FileNodeBase? File
        {
            get => file;
            set => SetAndRaise(FileProperty, ref file, value);
        }
        private FileNodeBase? file;

        public FileDetailsView()
        {
            InitializeComponent();
        }
    }
}
