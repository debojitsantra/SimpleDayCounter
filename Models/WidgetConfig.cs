using System;

namespace SimpleDayCounter.Models
{
    /// <summary>
    /// A single day-counter widget: what it counts down to, how it looks,
    /// and where it sits on screen. The whole list of these is persisted
    /// to widgets.json by SettingsStore.
    /// </summary>
    public class WidgetConfig
    {
        /// <summary>Stable identifier, generated once when a widget is created.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Label { get; set; } = "New event";

        public DateTime TargetDate { get; set; } = DateTime.Today.AddDays(30);

        /// <summary>Hex color string, e.g. "#7F77DD", used for the day-count text.</summary>
        public string Color { get; set; } = "#7F77DD";

        /// <summary>Saved screen position (desktop coordinates), so widgets stay put across restarts.</summary>
        public double X { get; set; } = 100;
        public double Y { get; set; } = 100;
    }
}
