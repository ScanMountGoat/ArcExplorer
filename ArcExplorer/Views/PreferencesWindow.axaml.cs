using ArcExplorer.Models;
using ArcExplorer.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(result) && ViewModel != null)
            {
                ViewModel.ExtractLocation = result;
            }
        }

        public async void OpenFileClick()
        {
            // The dialog requires the window reference, so this can't be in the viewmodel.
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter> { new FileDialogFilter { Extensions = new List<string> { "arc" }, Name = "ARC" } }
            };

            var result = await dialog.ShowAsync(this);
            if (result.Length > 0 && !string.IsNullOrEmpty(result[0]) && ViewModel != null)
            {
                ViewModel.ArcStartupLocation = result[0];
            }
        }
    }
}
