using ArcExplorer.Models;
using ArcExplorer.UserControls;
using ArcExplorer.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;
using System;
using System.Collections.Generic;

namespace ArcExplorer.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;

            // Use the event on the parent window to avoid keyboard focus issues.
            KeyDown += MainWindow_KeyDown;

            var fileTreeView = this.FindControl<FileTreeView>("fileTreeView");
            fileTreeView.DoubleTapped += MainWindow_DoubleTapped;
        }

        private void MainWindow_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            // Register key bindings to improve file navigation.
            // These are inspired by common shortcuts for Windows Explorer, Finder, etc.
            switch (e.Key)
            {
                case Avalonia.Input.Key.Right:
                case Avalonia.Input.Key.Enter:
                    (DataContext as MainWindowViewModel)?.EnterSelectedFolder();
                    break;
                case Avalonia.Input.Key.Left:
                    (DataContext as MainWindowViewModel)?.ExitFolder();
                    break;
                case Avalonia.Input.Key.Up:
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Alt)
                        (DataContext as MainWindowViewModel)?.ExitFolder();
                    else
                        (DataContext as MainWindowViewModel)?.SelectPreviousFile();
                    break;
                case Avalonia.Input.Key.Down:
                    (DataContext as MainWindowViewModel)?.SelectNextFile();
                    break;
                default:
                    break;
            }
        }

        private void MainWindow_DoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.EnterSelectedFolder();
            (sender as FileTreeView)?.Focus();
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
