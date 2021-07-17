using ArcExplorer.Models;
using Avalonia.Controls.ApplicationLifetimes;
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

        public bool MergeTrailingSlash
        {
            get => mergeTrailingSlash;
            set => this.RaiseAndSetIfChanged(ref mergeTrailingSlash, value);
        }
        private bool mergeTrailingSlash = ApplicationSettings.Instance.MergeTrailingSlash;

        public string ExtractLocation
        {
            get => extractLocation;
            set => this.RaiseAndSetIfChanged(ref extractLocation, value);
        }
        private string extractLocation = ApplicationSettings.Instance.ExtractLocation;

        public string? ArcStartupLocation
        {
            get => arcStartupLocation;
            set => this.RaiseAndSetIfChanged(ref arcStartupLocation, value);
        }
        private string? arcStartupLocation = ApplicationSettings.Instance.ArcStartupLocation;

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

                    // Refresh the file icons.
                    if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        if (desktop.MainWindow.DataContext is MainWindowViewModel vm)
                        { 
                            vm.ReloadCurrentDirectory();
                            vm.RefreshIcons();
                        }
                    }
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
                case nameof(ArcStartupLocation):
                    ApplicationSettings.Instance.ArcStartupLocation = ArcStartupLocation;
                    break;
                case nameof(MergeTrailingSlash):
                    ApplicationSettings.Instance.MergeTrailingSlash = MergeTrailingSlash;
                    break;
                default:
                    break;
            }
        }
    }
}
