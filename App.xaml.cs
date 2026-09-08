using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using SimpleDayCounter.Models;
using SimpleDayCounter.Services;
using SimpleDayCounter.Views;
using Forms = System.Windows.Forms;

namespace SimpleDayCounter
{
    /// <summary>
    /// App entry point. 
    /// and a set of floating widget windows. Settings are reachable only via
    /// right-click on the tray icon, per design.
    /// </summary>
    public partial class App : System.Windows.Application
    {

        private const string SingleInstanceMutexName = "SimpleDayCounter-9F1B2C3D-4E5F-4A6B-8C7D-1E2F3A4B5C6D";

        private Mutex? _singleInstanceMutex;
        private Forms.NotifyIcon? _trayIcon;
        private readonly Dictionary<string, WidgetWindow> _openWidgets = new();
        private List<WidgetConfig> _widgets = new();
        private SettingsWindow? _settingsWindow;
        private AppSettings _appSettings = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out bool createdNew);
            if (!createdNew)
            {
                System.Windows.MessageBox.Show(
                    "SimpleDayCounter is already running - check your system tray.",
                    "SimpleDayCounter",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                _singleInstanceMutex = null; 
                Shutdown();
                return;
            }


            DispatcherUnhandledException += (_, args) =>
            {
                System.Windows.MessageBox.Show(
                    $"An unexpected error occurred:\n\n{args.Exception}",
                    "SimpleDayCounter - Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                args.Handled = true;
            };

            _appSettings = SettingsStore.LoadAppSettings();
            _widgets = SettingsStore.Load();

            foreach (var widget in _widgets)
            {
                OpenWidgetWindow(widget);
            }

            SetupTrayIcon();
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new Forms.NotifyIcon
            {
                Icon = LoadTrayIcon(),
                Visible = true,
                Text = "SimpleDayCounter"
            };


            var menu = new Forms.ContextMenuStrip();

            var manageItem = new Forms.ToolStripMenuItem("Manage widgets…");
            manageItem.Click += (_, _) => OpenSettings();
            menu.Items.Add(manageItem);

            menu.Items.Add(new Forms.ToolStripSeparator());

            var startupItem = new Forms.ToolStripMenuItem("Start with Windows")
            {
                CheckOnClick = true,
                Checked = StartupService.IsEnabled()
            };
            startupItem.Click += (_, _) =>
            {
                bool succeeded = StartupService.SetEnabled(startupItem.Checked);
                if (!succeeded)
                {
                    // Revert the checkbox - the change was not actually applied.
                    startupItem.Checked = !startupItem.Checked;
                    System.Windows.MessageBox.Show(
                        "Couldn't set \"Start with Windows\" because the app's own .exe path " +
                        "could not be determined. This usually happens when running via " +
                        "\"dotnet run\" instead of the published .exe - try running the " +
                        "published SimpleDayCounter.exe directly instead.",
                        "SimpleDayCounter",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                }
            };
            menu.Items.Add(startupItem);

            var alwaysOnTopItem = new Forms.ToolStripMenuItem("Always on top")
            {
                CheckOnClick = true,
                Checked = _appSettings.AlwaysOnTop
            };
            alwaysOnTopItem.Click += (_, _) => SetAlwaysOnTop(alwaysOnTopItem.Checked);
            menu.Items.Add(alwaysOnTopItem);

            menu.Items.Add(new Forms.ToolStripSeparator());

            var exitItem = new Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) => ExitApplication();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
        }

        private static System.Drawing.Icon LoadTrayIcon()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Resources/app.ico");
                var streamInfo = GetResourceStream(uri);
                if (streamInfo != null)
                {
                    return new System.Drawing.Icon(streamInfo.Stream);
                }
            }
            catch
            {
                // Fall through to system default below.
            }

            return System.Drawing.SystemIcons.Application;
        }

        private void OpenSettings()
        {
            //  bring the existing
            // one to front instead of stacking duplicates.
            if (_settingsWindow != null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow(_widgets)
            {
                WidgetAdded = OnWidgetAdded,
                WidgetEdited = OnWidgetEdited,
                WidgetDeleted = OnWidgetDeleted
            };
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        private void OnWidgetAdded(WidgetConfig widget)
        {
            // Cascade new widgets diagonally so they don't spawn stacked
            // directly on top of each other before the user drags them apart.
            int index = _widgets.Count;
            widget.X = 80 + (index % 6) * 40;
            widget.Y = 80 + (index % 6) * 40;

            _widgets.Add(widget);
            OpenWidgetWindow(widget);
            SettingsStore.Save(_widgets);
        }

        private void OnWidgetEdited(WidgetConfig widget)
        {
            if (_openWidgets.TryGetValue(widget.Id, out var window))
            {
                window.ApplyConfig();
            }
            SettingsStore.Save(_widgets);
        }

        private void OnWidgetDeleted(WidgetConfig widget)
        {
            if (_openWidgets.TryGetValue(widget.Id, out var window))
            {
                window.Close();
                _openWidgets.Remove(widget.Id);
            }

            _widgets.RemoveAll(w => w.Id == widget.Id);
            SettingsStore.Save(_widgets);
        }

        private void SetAlwaysOnTop(bool alwaysOnTop)
        {
            _appSettings.AlwaysOnTop = alwaysOnTop;

            foreach (var window in _openWidgets.Values)
            {
                window.SetAlwaysOnTop(alwaysOnTop);
            }

            SettingsStore.SaveAppSettings(_appSettings);
        }

        private void OpenWidgetWindow(WidgetConfig widget)
        {
            var window = new WidgetWindow(widget, _appSettings.AlwaysOnTop);
            window.PositionChanged += _ => SettingsStore.Save(_widgets);
            window.Show();
            _openWidgets[widget.Id] = window;
        }

        private void ExitApplication()
        {
            SettingsStore.Save(_widgets);

            foreach (var window in _openWidgets.Values.ToList())
            {
                window.Close();
            }

            _settingsWindow?.Close();

            _trayIcon!.Visible = false;
            _trayIcon.Dispose();

            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();

            Shutdown();
        }
    }
}