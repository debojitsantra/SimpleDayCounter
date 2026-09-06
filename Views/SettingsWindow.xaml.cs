using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SimpleDayCounter.Models;

namespace SimpleDayCounter.Views
{
    /// <summary>
    /// The only place widgets are added, edited, or removed. Opened
    /// exclusively from the tray icon's right-click menu.
    /// Changes are applied live to the running widgets via the callbacks
    /// passed in from App - nothing here writes to disk directly, that
    /// stays the App/SettingsStore's job.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ObservableCollection<WidgetConfig> _widgets;

        /// <summary>Fired when a widget is added, edited, or deleted so App can sync live windows + save.</summary>
        public System.Action<WidgetConfig>? WidgetAdded { get; set; }
        public System.Action<WidgetConfig>? WidgetEdited { get; set; }
        public System.Action<WidgetConfig>? WidgetDeleted { get; set; }

        public SettingsWindow(System.Collections.Generic.List<WidgetConfig> widgets)
        {
            InitializeComponent();
            _widgets = new ObservableCollection<WidgetConfig>(widgets);
            WidgetsList.ItemsSource = _widgets;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newWidget = new WidgetConfig();
            var editor = new WidgetEditorWindow(newWidget) { Owner = this };

            if (editor.ShowDialog() == true)
            {
                WidgetAdded?.Invoke(newWidget);
                _widgets.Add(newWidget);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (WidgetsList.SelectedItem is not WidgetConfig selected)
            {
                System.Windows.MessageBox.Show(this, "Select a widget to edit first.", "No widget selected",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            EditWidget(selected);
        }

        private void WidgetsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WidgetsList.SelectedItem is WidgetConfig selected)
            {
                EditWidget(selected);
            }
        }

        private void EditWidget(WidgetConfig selected)
        {
            var editor = new WidgetEditorWindow(selected) { Owner = this };
            if (editor.ShowDialog() == true)
            {
                WidgetEdited?.Invoke(selected);

                // Refresh the list display (label/date/color may have changed).
                var index = _widgets.IndexOf(selected);
                _widgets.RemoveAt(index);
                _widgets.Insert(index, selected);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (WidgetsList.SelectedItem is not WidgetConfig selected)
            {
                System.Windows.MessageBox.Show(this, "Select a widget to delete first.", "No widget selected",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var confirm = System.Windows.MessageBox.Show(this,
                $"Remove the widget \"{selected.Label}\"?",
                "Confirm delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm == System.Windows.MessageBoxResult.Yes)
            {
                _widgets.Remove(selected);
                WidgetDeleted?.Invoke(selected);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
