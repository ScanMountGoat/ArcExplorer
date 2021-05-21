using ArcExplorer.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;
using System;
using System.Collections.Generic;
using ArcExplorer.Models;
using ArcExplorer.UserControls;

namespace ArcExplorer.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            this.FindControl<FileTreeView>("fileTreeView").DoubleTapped += MainWindow_DoubleTapped;
        }

        private void MainWindow_DoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.EnterFolder();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            ApplicationSettings.Instance.SaveToFile();
            Log.CloseAndFlush();
        }

        public void OpenPreferencesWindow()
        {
            var window = PreferencesWindow.Instance.Value;
            window.Show();
        }

        public async void OpenArc()
        {
            // The dialog requires the window reference, so this can't be in the viewmodel.
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter> { new FileDialogFilter { Extensions = new List<string> { "arc" }, Name = "ARC" } }
            };
            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                (DataContext as MainWindowViewModel)?.OpenArcFile(result[0]);
            }
        }

        public async void OpenArcNetworked()
        {
            var dialog = new OpenArcConnectionWindow();
            dialog.Closed += (s, e) =>
            {
                if (!dialog.WasCancelled)
                {
                    (DataContext as MainWindowViewModel)?.OpenArcNetworked(dialog.IpAddress);
                }
            };
            await dialog.ShowDialog(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
