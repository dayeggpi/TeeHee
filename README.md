# TeeHee - Text Expander

A free, open source, lightweight text expander for Windows.

## Features

- **Instant Expansion**: Type your trigger anywhere on Windows and watch it instantly expand to your defined text. No delays, no hassle.
- **Dynamic Placeholders**: Use built-in placeholders like {{date}}, {{time}}, {{clipboard}}, and more to insert dynamic content automatically.
- **System-Wide**: Works everywhere, emails, documents, browsers, code editors, chat apps. If you can type there, TeeHee works there.
- **Categories**: Organize your triggers into categories for easy management. Filter and find what you need in seconds.
- **Pause & Resume**: Quickly toggle text expansion on or off from the system tray. Visual indicator shows current status.
- **Import & Export**: Backup your triggers or share them across devices. Your data is stored locally in a simple JSON file.
- **Lightweight**: Runs quietly in your system tray using minimal resources. Single instance only, no duplicates.
- **Modern Interface**: Beautiful dark-themed UI that's easy on the eyes. Managing your triggers has never looked this good.
- **Privacy First**: 100% offline. No data collection, no analytics, no accounts. Your triggers stay on your computer.

## Default Trigger

The app comes with one default trigger:
- **Trigger**: `:hi`
- **Expansion**: `hello world`

## Data Storage

Triggers are stored in JSON format at:

```
%APPDATA%\TeeHee\triggers.json
```

Contains: Your text expansion triggers (input/output pairs)


Settings are stored in JSON format at:

```
%APPDATA%\TeeHee\settings.json
```

Contains: Application preferences (trigger speed, custom database path)

## Tips

- Keep triggers short and unique (e.g., `:em` for email)
- Use prefixes like `:` or `//` to avoid accidental triggers
- Multi-line text is supported in expansions

## Troubleshooting

### App doesn't start

- Make sure you have .NET 8 runtime installed (for framework-dependent builds)
- Run as Administrator if you have permission issues

### Text expansion not working

- Some applications with elevated privileges may block keyboard hooks
- Try running TeeHee as Administrator

### Triggers not saving

- Check write permissions to `%APPDATA%\TeeHee\`



## Requirements

- Windows 10/11 (64-bit)
- .NET 8 SDK (for building)
- Emoji.Wpf package (`dotnet add package Emoji.Wpf`)

## Building the Application

### Prerequisites

1. Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
2. Install Visual Studio Code (optional, but recommended)
   - Install the C# extension

### Build Steps

1. **Open a terminal/command prompt** in the project folder

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project** (Debug mode):
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

### Creating a Standalone Executable

To create a single `.exe` file that can run without .NET installed:

```bash
dotnet publish -c Release
```

The executable will be created at:
```
bin\Release\net8.0-windows\win-x64\publish\TeeHee.exe
```

## Project Structure

```
TeeHee/
├── App.xaml              # Application resources and styling
├── App.xaml.cs           # Application startup logic
├── ConfirmDialog.xaml    # Confirm Dialog window
├── ConfirmDialog.xaml.cs # Confirm Dialog code-behind
├── InfoDialog.xaml       # Info Dialog window
├── InfoDialog.xaml.cs    # Info Dialog code-behind
├── TriggerDialog.xaml    # Trigger Dialog window
├── TriggerDialog.xaml.cs # Trigger Dialog code-behind
├── MainWindow.xaml       # Main UI window
├── MainWindow.xaml.cs    # Main window code-behind
├── TeeHee.csproj         # Project configuration
├── app.ico               # Application icon
├── Models/
│   ├── Trigger.cs        # Trigger data model
│   └── AppSettings.cs    # Settings model
└── Services/
    ├── TriggerDatabase.cs  	# JSON persistence
    ├── KeyboardHook.cs     	# Global keyboard hook
    ├── TrayIconManager.cs   	# System tray management
    └── StartupManager.cs    	# Windows startup registration
    └── PlaceholderService.cs   # PlaceholderService management
    └── TruncateConverter.cs    # Truncate service
```

## How It Works

1. The app installs a low-level keyboard hook using Windows API
2. As you type, characters are added to a buffer
3. When a trigger is detected (buffer ends with a trigger string), the app:
   - Sends backspace keystrokes to delete the trigger text
   - Types the replacement text using SendInput



## License

MIT License

## Stats

<img title="repo views" src="https://mg.lu/gh/teehee/">