using ArcExplorer.Models;
using ArcExplorer.UserControls;
using ArcExplorer.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;

namespace ArcExplorer.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;

            // Use the event on the parent window to avoid keyboard focus issues.
            KeyDown += FolderNavigation_KeyDown;

            fileTreeView.DoubleTapped += MainWindow_DoubleTapped;

            // Navigation keys won't normally trigger the key down event, so add an additional handler.
            // https://github.com/AvaloniaUI/Avalonia/issues/5244 
            fileTreeView.FileGrid?.AddHandler(KeyDownEvent, FolderNavigation_KeyDown, RoutingStrategies.Tunnel);
        }

        private void FolderNavigation_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            // Register key bindings to improve file navigation.
            // These are inspired by common shortcuts for Windows Explorer, Finder, etc.
            switch (e.Key)
            {
                case Avalonia.Input.Key.Right:
                case Avalonia.Input.Key.Enter:
                    ViewModel?.EnterSelectedFolder();
                    e.Handled = true;
                    break;
                case Avalonia.Input.Key.Left:
                    ViewModel?.ExitFolder();
                    e.Handled = true;
                    break;
                case Avalonia.Input.Key.Up:
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Alt)
                    {
                        ViewModel?.ExitFolder();
                        e.Handled = true;
                    }
                    else
                    {
                        ViewModel?.SelectPreviousFile();
                        e.Handled = true;
                    }
                    break;
                case Avalonia.Input.Key.Down:
                    ViewModel?.SelectNextFile();
                    e.Handled = true;
                    break;
                default:
                    break;
            }

        }

        private void MainWindow_DoubleTapped(object? sender, RoutedEventArgs e)
        {
            ViewModel?.EnterSelectedFolder();
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
                ViewModel?.OpenArcFile(result[0]);
            }
        }

        public async void OpenArcNetworked()
        {
            var vm = new OpenArcConnectionWindowViewModel
            {
                IpAddress = ApplicationSettings.Instance.ArcIpAddress ?? "000.000.000.000"
            };

            var dialog = new OpenArcConnectionWindow 
            { 
                ViewModel = vm 
            };

            dialog.Closed += (s, e) =>
            {
                if (!vm.WasCancelled)
                {
                    // Only save settings that were actually submitted.
                    if (vm.RememberIpAddress)
                    {
                        ApplicationSettings.Instance.ArcIpAddress = vm.IpAddress;
                    }

                    ViewModel?.OpenArcNetworked(vm.IpAddress);
                }
            };

            await dialog.ShowDialog(this);
        }
    }
}
