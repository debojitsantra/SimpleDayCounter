using System;
using System.IO;
using Microsoft.Win32;

namespace SimpleDayCounter.Services
{
    /// <summary>
    /// Toggles "run at Windows startup" via the per-user registry Run key.
    /// </summary>
    public static class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "SimpleDayCounter";

        public static bool IsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return key?.GetValue(ValueName) != null;
        }

        /// <returns>True if the change was applied successfully.</returns>
        public static bool SetEnabled(bool enabled)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);

            if (enabled)
            {
                var exePath = GetReliableExePath();
                if (exePath == null)
                {
                    return false;
                }

                key.SetValue(ValueName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return true;
        }

        private static string? GetReliableExePath()
        {
            var path = Environment.ProcessPath;

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!File.Exists(path))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            if (!string.Equals(fileName, "dotnet", StringComparison.OrdinalIgnoreCase)
                && Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return null;
        }
    }
}
