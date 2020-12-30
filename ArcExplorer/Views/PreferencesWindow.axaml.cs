using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ArcExplorer.Models;
using ArcExplorer.ViewModels;
using Newtonsoft.Json;
using System;

namespace ArcExplorer.Views
{
    public class PreferencesWindow : Window
    {
        public static Lazy<PreferencesWindow> Instance { get; } = new Lazy<PreferencesWindow>();

        public PreferencesWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new PreferencesWindowViewModel();

            Closing += PreferencesWindow_Closing;
        }

        private void PreferencesWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // HACK: Weird workaround for only allowing a single window.
            // Windows can't be shown after closing.
            e.Cancel = true;
            Hide();

            // TODO: This is probably not the best place for this.
            var json = JsonConvert.SerializeObject(ApplicationSettings.Instance);
            System.IO.File.WriteAllText("ApplicationPreferences.json", json);
        }

        public async void OpenFolderClick()
        {
            // The dialog requires the window reference, so this can't be in the viewmodel.
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                var vm = (DataContext as PreferencesWindowViewModel);
                if (vm != null)
                    vm.ExtractLocation = result;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
