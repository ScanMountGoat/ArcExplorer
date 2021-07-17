using ArcExplorer.Models;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;

namespace ArcExplorer.ViewModels
{
    public class OpenArcConnectionWindowViewModel : ViewModelBase
    {
        // TODO: Does this need property changed?
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
