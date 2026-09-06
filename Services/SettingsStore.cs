using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SimpleDayCounter.Models;

namespace SimpleDayCounter.Services
{
    /// <summary>
    /// Small app-wide preferences, separate from the widget list itself.
    /// Persisted to settings.json alongside widgets.json.
    /// </summary>
    public class AppSettings
    {
        public bool AlwaysOnTop { get; set; } = true;
    }

    /// <summary>
    /// Owns reading/writing the widget list and app settings to disk. The
    /// </summary>
    public static class SettingsStore
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SimpleDayCounter");

        private static readonly string FilePath = Path.Combine(AppFolder, "widgets.json");
        private static readonly string AppSettingsPath = Path.Combine(AppFolder, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static List<WidgetConfig> Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return CreateDefaultWidgets();
                }

                var json = File.ReadAllText(FilePath);
                var widgets = JsonSerializer.Deserialize<List<WidgetConfig>>(json, JsonOptions);
                return widgets ?? new List<WidgetConfig>();
            }
            catch
            {
                // Corrupt or unreadable file - don't crash the app, start fresh.
                return new List<WidgetConfig>();
            }
        }

        public static void Save(List<WidgetConfig> widgets)
        {
            Directory.CreateDirectory(AppFolder);
            var json = JsonSerializer.Serialize(widgets, JsonOptions);
            WriteAtomically(FilePath, json);
        }

        public static AppSettings LoadAppSettings()
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(AppSettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void SaveAppSettings(AppSettings settings)
        {
            Directory.CreateDirectory(AppFolder);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            WriteAtomically(AppSettingsPath, json);
        }

        private static void WriteAtomically(string path, string content)
        {
            // Write to a temp file then move, so a crash mid-write never
            // corrupts the real file.
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, content);
            File.Copy(tempPath, path, overwrite: true);
            File.Delete(tempPath);
        }

        private static List<WidgetConfig> CreateDefaultWidgets()
        {
            var defaults = new List<WidgetConfig>
            {
                new WidgetConfig
                {
                    Label = "New Year",
                    TargetDate = new DateTime(DateTime.Today.Year + 1, 1, 1),
                    Color = "#7F77DD",
                    X = 80,
                    Y = 80
                }
            };
            Save(defaults);
            return defaults;
        }
    }
}
