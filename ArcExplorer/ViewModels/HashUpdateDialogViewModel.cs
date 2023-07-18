using ReactiveUI;

namespace ArcExplorer.ViewModels
{
    public class HashUpdateDialogViewModel : ViewModelBase
    {
        public string Description { get; } = "";

        public string Author { get; } = "";

        public string Date { get; } = "";

        public HashUpdateDialogViewModel(string description, string author, string date)
        {
            Description = description;
            Author = author;
            Date = date;
        }

        public bool WasCancelled
        {
            get => wasCancelled;
            set => this.RaiseAndSetIfChanged(ref wasCancelled, value);
        }
        private bool wasCancelled = true;
    }
}
