using ArcExplorer.Models;
using ArcExplorer.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcExplorer.Views
{
    public partial class PreferencesWindow : ReactiveWindow<PreferencesWindowViewModel>
    {
        public static Lazy<PreferencesWindow> Instance { get; } = new Lazy<PreferencesWindow>();

        public PreferencesWindow()
        {
            InitializeComponent();
            ViewModel = new PreferencesWindowViewModel();
            Closing += PreferencesWindow_Closing;
        }

        private void PreferencesWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // HACK: Weird workaround for only allowing a single window.
            // Windows can't be shown after closing.
            e.Cancel = true;
            Hide();

            ApplicationSettings.Instance.SaveToFile();
        }

        public async void OpenFolderClick()
        {
            // The dialog requires the window reference, so this can't be in the viewmodel.
            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false });
            var path = result.FirstOrDefault()?.Path.LocalPath;
            if (!string.IsNullOrEmpty(path) && ViewModel != null)
            {
                ViewModel.ExtractLocation = path;
            }
        }

        public async void OpenFileClick()
        {
            // The dialog requires the window reference, so this can't be in the viewmodel.
            var options = new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = new List<FilePickerFileType> { new FilePickerFileType("ARC") { Patterns = new List<string> { "*.arc" } } } };
            var result = await StorageProvider.OpenFilePickerAsync(options);
            var path = result.FirstOrDefault()?.Path.LocalPath;
            if (!string.IsNullOrEmpty(path) && ViewModel != null)
            {
                ViewModel.ArcStartupLocation = path;
            }
        }
    }
}
