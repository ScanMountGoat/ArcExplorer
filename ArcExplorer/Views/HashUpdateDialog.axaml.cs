using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArcExplorer.Views
{
    public class HashUpdateDialog : Window
    {
        public string Description { get; } = "";

        public string Author { get; } = "";

        public string Date { get; } = "";
        
        public bool WasCancelled { get; private set; }

        public HashUpdateDialog()
        {
            InitializeComponent();
        }

        public HashUpdateDialog(string description, string author, string date) : this()
        {
            Description = description;
            Author = author;
            Date = date;

            // Set the data context after updating the values to ensure the correct values are displayed. 
            DataContext = this;
        }

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
