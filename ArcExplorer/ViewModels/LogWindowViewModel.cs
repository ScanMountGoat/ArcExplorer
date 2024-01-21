using Avalonia.Collections;
using ReactiveUI;
using System.Collections;

namespace ArcExplorer.ViewModels
{
    public class LogWindowViewModel : ViewModelBase
    {
        public IEnumerable Items
        {
            get => items;
            set => this.RaiseAndSetIfChanged(ref items, value);
        }
        private IEnumerable items = new AvaloniaList<string>();
    }
}
