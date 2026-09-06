using System;
using System.Windows;
using System.Windows.Media;
using SimpleDayCounter.Models;

namespace SimpleDayCounter.Views
{
    /// <summary>
    /// Modal add/edit form for a single widget. Mutates the WidgetConfig
    /// passed in directly and sets DialogResult = true on success, so the
    /// caller (SettingsWindow) knows whether to keep the change.
    /// </summary>
    public partial class WidgetEditorWindow : Window
    {
        private readonly WidgetConfig _target;

        private static readonly (string Name, string Hex)[] PresetColors =
        {
            ("Purple", "#7F77DD"),
            ("Teal",   "#1D9E75"),
            ("Coral",  "#D85A30"),
            ("Pink",   "#D4537E"),
            ("Blue",   "#378ADD"),
            ("Green",  "#639922"),
            ("Amber",  "#BA7517"),
            ("Red",    "#E24B4A"),
        };

        public WidgetEditorWindow(WidgetConfig target)
        {
            InitializeComponent();
            _target = target;

            foreach (var (name, hex) in PresetColors)
            {
                ColorBox.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = name, Tag = hex });
            }

            for (int day = 1; day <= 31; day++)
            {
                DayBox.Items.Add(day);
            }

            var months = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
            for (int m = 0; m < 12; m++)
            {
                MonthBox.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = months[m], Tag = m + 1 });
            }

            int currentYear = DateTime.Today.Year;
            for (int year = currentYear - 1; year <= currentYear + 15; year++)
            {
                YearBox.Items.Add(year);
            }

            LabelBox.Text = target.Label;

            DayBox.SelectedItem = target.TargetDate.Day;
            MonthBox.SelectedIndex = target.TargetDate.Month - 1;
            YearBox.SelectedItem = target.TargetDate.Year;

            // Preselect the matching preset if the current color matches one,
            // otherwise leave the preset list blank and show it in the custom box.
            var matchIndex = Array.FindIndex(PresetColors, p => string.Equals(p.Hex, target.Color, StringComparison.OrdinalIgnoreCase));
            if (matchIndex >= 0)
            {
                ColorBox.SelectedIndex = matchIndex;
            }
            else
            {
                CustomColorBox.Text = target.Color;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var label = LabelBox.Text.Trim();
            if (string.IsNullOrEmpty(label))
            {
                ShowError("Enter a label for this widget.");
                return;
            }

            if (DayBox.SelectedItem is not int day ||
                MonthBox.SelectedItem is not System.Windows.Controls.ComboBoxItem monthItem ||
                monthItem.Tag is not int month ||
                YearBox.SelectedItem is not int year)
            {
                ShowError("Pick a day, month, and year.");
                return;
            }

            DateTime targetDate;
            try
            {
                targetDate = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                ShowError($"{monthItem.Content} doesn't have a day {day}. Pick a valid date.");
                return;
            }

            string colorHex = _target.Color;

            var customText = CustomColorBox.Text.Trim();
            if (!string.IsNullOrEmpty(customText))
            {
                if (!IsValidColor(customText))
                {
                    ShowError("That custom color isn't valid. Use a hex code like #378ADD.");
                    return;
                }
                colorHex = customText;
            }
            else if (ColorBox.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag is string hex)
            {
                colorHex = hex;
            }

            _target.Label = label;
            _target.TargetDate = targetDate;
            _target.Color = colorHex;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private static bool IsValidColor(string text)
        {
            try
            {
                System.Windows.Media.ColorConverter.ConvertFromString(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
