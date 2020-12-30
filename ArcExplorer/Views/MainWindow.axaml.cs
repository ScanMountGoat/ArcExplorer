using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ArcExplorer.ViewModels;
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
#if DEBUG
            this.AttachDevTools();
#endif
            this.Find<MenuItem>("preferencesMenuItem").Click += Preferences_Click;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }

        private void Preferences_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = PreferencesWindow.Instance.Value;
            window.Show();
        }

        public async void OpenArc()
        {
            // The dialog requires the window reference, so this can't be in the viewmodel.
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false
            };
            dialog.Filters.Add(new FileDialogFilter { Extensions = new List<string> { "arc" }, Name = "ARC" });
            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                (DataContext as MainWindowViewModel)?.OpenArc(result[0]);
            }
        }

        private async void OpenArcNetworked()
        {
            var window = new OpenArcConnectionWindow();
            window.Show();
            // TODO: Pass the IP Address to the mainwindow viewmodel to open the ARC.
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
