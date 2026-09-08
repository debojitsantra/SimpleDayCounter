using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SimpleDayCounter.Models;

namespace SimpleDayCounter.Views
{
    public partial class WidgetWindow : Window
    {
        public WidgetConfig Config { get; private set; }


        public event Action<WidgetWindow>? PositionChanged;

        private readonly DispatcherTimer _refreshTimer;

        public WidgetWindow(WidgetConfig config, bool alwaysOnTop)
        {
            InitializeComponent();
            Config = config;

            var (clampedX, clampedY) = ClampToVisibleScreen(config.X, config.Y);
            Left = clampedX;
            Top = clampedY;
            Topmost = alwaysOnTop;

            ApplyConfig();

            // Update once a minute. 
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _refreshTimer.Tick += (_, _) => UpdateCountdownText();
            _refreshTimer.Start();

            Closed += (_, _) => _refreshTimer.Stop();
        }

        private static (double X, double Y) ClampToVisibleScreen(double x, double y)
        {
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;

            // Leave a little margin so the card isn't flush against the very edge.
            const double margin = 20;
            double minX = virtualLeft + margin;
            double minY = virtualTop + margin;
            double maxX = virtualLeft + virtualWidth - margin;
            double maxY = virtualTop + virtualHeight - margin;

            // If the whole virtual desktop is somehow degenerate, just fall
            // back to the saved position rather than risk dividing/clamping
            // into something nonsensical.
            if (virtualWidth <= 0 || virtualHeight <= 0)
            {
                return (x, y);
            }

            bool onScreen = x >= virtualLeft && x <= virtualLeft + virtualWidth
                             && y >= virtualTop && y <= virtualTop + virtualHeight;

            if (onScreen)
            {
                return (x, y);
            }

            double clampedX = Math.Min(Math.Max(x, minX), maxX);
            double clampedY = Math.Min(Math.Max(y, minY), maxY);
            return (clampedX, clampedY);
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
                double startLeft = Left;
                double startTop = Top;

                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {

                    return;
                }

                // DragMove blocks until the mouse button is released, so by

                if (Left != startLeft || Top != startTop)
                {
                    Config.X = Left;
                    Config.Y = Top;
                    PositionChanged?.Invoke(this);
                }
            }
        }
    }
}