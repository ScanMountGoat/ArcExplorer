using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CrossArcAvaloniaConcept.ViewModels;

namespace CrossArcAvaloniaConcept.UserControls
{
    public class FileDetailsView : UserControl
    {
        public static readonly DirectProperty<FileDetailsView, FileNode?> FileProperty =
            AvaloniaProperty.RegisterDirect<FileDetailsView, FileNode?>(
            nameof(File),
            o => o.File,
            (o, v) => o.File = v, 
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public FileNode? File
        {
            get => file;
            set => SetAndRaise(FileProperty, ref file, value);
        }
        private FileNode? file;

        public FileDetailsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
