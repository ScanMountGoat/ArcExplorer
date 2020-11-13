using CrossArcAvaloniaConcept.Models;
using ReactiveUI;
using System;

namespace CrossArcAvaloniaConcept.ViewModels
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
                default:
                    break;
            }
        }
    }
}
