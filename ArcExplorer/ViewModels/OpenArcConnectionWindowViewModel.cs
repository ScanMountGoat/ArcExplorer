using ReactiveUI;

namespace ArcExplorer.ViewModels
{
    public class OpenArcConnectionWindowViewModel : ViewModelBase
    {
        public bool RememberIpAddress
        {
            get => rememberIpAddress;
            set => this.RaiseAndSetIfChanged(ref rememberIpAddress, value);
        }
        private bool rememberIpAddress;

        public bool WasCancelled { get; set; } = true;

        public string IpAddress
        {
            get => ipAddress;
            set => this.RaiseAndSetIfChanged(ref ipAddress, value);
        }
        private string ipAddress = "000.000.000.000";
    }
}
