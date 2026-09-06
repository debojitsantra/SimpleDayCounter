# SimpleDayCounter

A lightweight Windows tray app that shows draggable day-countdown widgets. Built with .NET 8 + WPF.

## Functions

- Add/edit/delete widgets from a **Settings** window, reachable only via
  **right-click the tray icon → Manage widgets…**
- Double-click a widget in Settings to edit it directly
- Widget positions are remembered between restarts.
- **Always on top** toggle in the tray menu.
- Optional **Start with Windows** toggle.

## Requirements

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build
  (end users only need the .NET 8 **Desktop Runtime** if you use the
  framework-dependent build, or nothing extra with the self-contained one — see below)

## Build & run

Open a terminal in this folder:

```powershell
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

The app has no visible window on launch — look for its icon in the system
tray (you may need to click the little "^" to show hidden tray icons the
first time).

## standalone .exe

Framework-dependent (smaller, requires .NET 8 Desktop Runtime on the target machine):

```powershell
dotnet publish -c Release -p:SelfContained=false
```

Fully self-contained (larger, but runs on a machine with no .NET installed):

```powershell
dotnet publish -c Release -p:SelfContained=true
```

Either produces a single file at:

```
bin\Release\net8.0-windows\win-x64\publish\SimpleDayCounter.exe
```

## CI

`.github/workflows/build.yml` builds both variants (framework-dependent and
self-contained) on every push/PR to `main` via GitHub Actions.

Data is stored in `%AppData%\SimpleDayCounter\`:

- `widgets.json` — your list of widgets
- `settings.json` — app-wide preferences (currently just always-on-top)

You generally never need to touch these files directly, but they're there
if you want to back them up or move widgets between machines.

## Project layout

```
SimpleDayCounter.csproj             Project file (WPF, .NET 8)
App.xaml / App.xaml.cs               Tray icon + app lifecycle.
Models/WidgetConfig.cs               Data model for one widget
Services/SettingsStore.cs            Load/save widgets.json and settings.json
Services/StartupService.cs           Registry Run-key toggle
Views/WidgetWindow.*                 The draggable on-screen countdown card
Views/SettingsWindow.*               List of widgets + add/edit/delete/double-click
Views/WidgetEditorWindow.*           Add/edit form for a single widget
Views/HexColorToBrushConverter.cs    Binding converter for color swatches in the list
Resources/app.ico                    Tray + app icon
.github/workflows/build.yml          CI
```
