using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SimpleDayCounter.Models;

namespace SimpleDayCounter.Views
{
    /// <summary>
    /// A single floating, draggable, always-on-top day-counter card.
    /// Refreshes its text once a minute via DispatcherTimer
    /// </summary>
    public partial class WidgetWindow : Window
    {
        public WidgetConfig Config { get; private set; }

        /// <summary>Raised whenever the user finishes dragging this widget, so the
        /// owner (App) can persist the new position.</summary>
        public event Action<WidgetWindow>? PositionChanged;

        private readonly DispatcherTimer _refreshTimer;

        public WidgetWindow(WidgetConfig config, bool alwaysOnTop)
        {
            InitializeComponent();
            Config = config;

            Left = config.X;
            Top = config.Y;
            Topmost = alwaysOnTop;

            ApplyConfig();

            // Update once a minute. A day-counter never needs a tighter
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _refreshTimer.Tick += (_, _) => UpdateCountdownText();
            _refreshTimer.Start();

            Closed += (_, _) => _refreshTimer.Stop();
        }

        /// <summary>Applies a new always-on-top state to this already-open window.</summary>
        public void SetAlwaysOnTop(bool alwaysOnTop)
        {
            Topmost = alwaysOnTop;
        }

        /// <summary>Re-applies label/date/color after a settings edit, without recreating the window.</summary>
        public void ApplyConfig()
        {
            TitleText.Text = Config.Label;
            DateText.Text = Config.TargetDate.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);

            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Config.Color);
                DaysText.Foreground = new SolidColorBrush(color);
            }
            catch
            {
                // Bad/missing color string - fall back to the default already set in XAML.
            }

            UpdateCountdownText();
        }

        private void UpdateCountdownText()
        {
            var today = DateTime.Today;
            var target = Config.TargetDate.Date;
            int diff = (target - today).Days;

            if (diff > 0)
            {
                DaysText.Text = diff.ToString(CultureInfo.InvariantCulture);
                DaysLabelText.Text = diff == 1 ? "DAY LEFT" : "DAYS LEFT";
            }
            else if (diff == 0)
            {
                DaysText.Text = "Today";
                DaysLabelText.Text = "";
            }
            else
            {
                DaysText.Text = Math.Abs(diff).ToString(CultureInfo.InvariantCulture);
                DaysLabelText.Text = Math.Abs(diff) == 1 ? "DAY AGO" : "DAYS AGO";
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();

                // DragMove blocks until the mouse button is released, so by
                // the time we get here the drag is finished.
                Config.X = Left;
                Config.Y = Top;
                PositionChanged?.Invoke(this);
            }
        }
    }
}
