# TeeHee - Text Expander

A free lightweight text expander for Windows built with .NET 8 and WPF.

## Features

- **Instant Expansion**: Type your trigger anywhere on Windows and watch it instantly expand to your defined text. No delays, no hassle.
- **Dynamic Placeholders**: Use built-in placeholders like {{date}}, {{time}}, {{clipboard}}, and more to insert dynamic content automatically.
- **System-Wide**: Works everywhere—emails, documents, browsers, code editors, chat apps. If you can type there, TeeHee works there.
- **Categories**: Organize your triggers into categories for easy management. Filter and find what you need in seconds.
- **Pause & Resume**: Quickly toggle text expansion on or off from the system tray. Visual indicator shows current status.
- **Import & Export**: Backup your triggers or share them across devices. Your data is stored locally in a simple JSON file.
- **Lightweight**: Runs quietly in your system tray using minimal resources. Single instance only—no duplicates.
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


## How It Works

1. The app installs a low-level keyboard hook using Windows API
2. As you type, characters are added to a buffer
3. When a trigger is detected (buffer ends with a trigger string), the app:
   - Sends backspace keystrokes to delete the trigger text
   - Types the replacement text using SendInput

## Tips

- Keep triggers short and unique (e.g., `:em` for email)
- Use prefixes like `:` or `//` to avoid accidental triggers
- Multi-line text is supported in expansions

## Troubleshooting

### App doesn't start
- Make sure you have .NET 8 Desktop runtime installed (for framework-dependent builds) from https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime
- Run as Administrator if you have permission issues

### Text expansion not working
- Some applications with elevated privileges may block keyboard hooks
- Try running TeeHee as Administrator

### Triggers not saving
- Check write permissions to `%APPDATA%\TeeHee\`

## License

MIT License (code to be uploaded soon)

## Stats

<img title="repo views" src="https://mg.lu/gh/teehee/">

