using ArcExplorer.Models;
using ReactiveUI;
using System;

namespace ArcExplorer.ViewModels
{
    public class PreferencesWindowViewModel : ViewModelBase
    {
        public ApplicationSettings.VisualTheme CurrentTheme
        {
            get => currentTheme;
            set => this.RaiseAndSetIfChanged(ref currentTheme, value);
        }
        private ApplicationSettings.VisualTheme currentTheme = ApplicationSettings.Instance.Theme;

        public Array Themes { get; } = Enum.GetValues(typeof(ApplicationSettings.VisualTheme));

        public Array IntegerDisplayFormats { get; } = Enum.GetValues(typeof(ApplicationSettings.IntegerDisplayFormat));

        public ApplicationSettings.IntegerDisplayFormat IntegerDisplayFormat 
        { 
            get => integerDisplayFormat;
            set => this.RaiseAndSetIfChanged(ref integerDisplayFormat, value);
        }
        private ApplicationSettings.IntegerDisplayFormat integerDisplayFormat = ApplicationSettings.Instance.DisplayFormat;

        public bool StartMaximized
        {
            get => startMaximized;
            set => this.RaiseAndSetIfChanged(ref startMaximized, value);
        }
        private bool startMaximized = ApplicationSettings.Instance.StartMaximized;

        public string ExtractLocation
        {
            get => extractLocation;
            set => this.RaiseAndSetIfChanged(ref extractLocation, value);
        }
        private string extractLocation = ApplicationSettings.Instance.ExtractLocation;

        public PreferencesWindowViewModel()
        {
            PropertyChanged += PreferencesWindowViewModel_PropertyChanged;
        }

        private void PreferencesWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CurrentTheme):
                    ApplicationSettings.Instance.Theme = CurrentTheme;
                    ApplicationStyles.SetThemeFromSettings();
                    break;
                case nameof(IntegerDisplayFormat):
                    ApplicationSettings.Instance.DisplayFormat = IntegerDisplayFormat;
                    break;
                case nameof(ExtractLocation):
                    ApplicationSettings.Instance.ExtractLocation = ExtractLocation;
                    break;
                case nameof(StartMaximized):
                    ApplicationSettings.Instance.StartMaximized = StartMaximized;
                    break;
                default:
                    break;
            }
        }
    }
}
