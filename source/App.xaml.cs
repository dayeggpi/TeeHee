using System.Windows;
using System.Threading;

namespace TeeHee;

public partial class App : System.Windows.Application
{
    private static Mutex? _mutex;
    private TrayIconManager? _trayIconManager;
    private KeyboardHook? _keyboardHook;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "TeeHee_SingleInstance_Mutex";
        _mutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            System.Windows.MessageBox.Show(
                "TeeHee is already running.\n\nCheck your system tray for the TeeHee icon.",
                "TeeHee Already Running",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            Shutdown();
            return;
        }

        base.OnStartup(e);

        System.Diagnostics.Debug.WriteLine("App starting...");

        // Detect settings location before loading
        var locationResult = TriggerDatabase.Instance.DetectSettingsLocation();
        
		if (locationResult == SettingsLocationResult.NotFound)
		{
			// Show dialog to let user choose location (hide cancel on first launch)
			var dialog = new SettingsLocationDialog();
			dialog.ShowCancelButton = false;
            if (dialog.ShowDialog() == true)
            {
                TriggerDatabase.Instance.SetSettingsLocation(dialog.SelectedMode, dialog.CustomPath);
            }
            else
            {
                // User cancelled - default to AppData
                TriggerDatabase.Instance.SetSettingsLocation(SettingsLocationMode.AppData);
            }
        }

        TriggerDatabase.Instance.Load();
        System.Diagnostics.Debug.WriteLine($"Loaded {TriggerDatabase.Instance.Triggers.Count} triggers");
        System.Diagnostics.Debug.WriteLine($"Settings location: {TriggerDatabase.SettingsPath}");

        _keyboardHook = new KeyboardHook();
        _keyboardHook.IsEnabled = TriggerDatabase.Instance.Settings.IsEnabled;
        _keyboardHook.Start();
        System.Diagnostics.Debug.WriteLine("Keyboard hook started");

        _trayIconManager = new TrayIconManager();
        _trayIconManager.OnOpenRequested += ShowMainWindow;
        _trayIconManager.OnExitRequested += ExitApplication;
        _trayIconManager.OnToggleRequested += ToggleEnabled;
        _trayIconManager.Initialize();
        System.Diagnostics.Debug.WriteLine("Tray icon initialized");

        ShowMainWindow();
    }

    public void RefreshKeyboardHook()
    {
        _keyboardHook?.RefreshTriggerCache();
    }

    public void SetKeyboardHookEnabled(bool enabled)
    {
        if (_keyboardHook != null)
        {
            _keyboardHook.IsEnabled = enabled;
        }
        TriggerDatabase.Instance.Settings.IsEnabled = enabled;
        TriggerDatabase.Instance.Save();
    }

    public bool GetKeyboardHookEnabled()
    {
        return _keyboardHook?.IsEnabled ?? true;
    }

    private void ToggleEnabled(bool enabled)
    {
        SetKeyboardHookEnabled(enabled);
    }

    public void UpdateTrayIcon(bool enabled)
    {
        _trayIconManager?.UpdateToggleState(enabled);
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (s, e) => _mainWindow = null;
        }
        
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
        _mainWindow.Focus();
    }

    private void ExitApplication()
    {
        _keyboardHook?.Stop();
        _trayIconManager?.Dispose();
        TriggerDatabase.Instance.Save();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardHook?.Stop();
        _trayIconManager?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}