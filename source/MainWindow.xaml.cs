using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace TeeHee;

public partial class MainWindow : Window
{
    private string _currentTheme = "Light";
    
    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
        LoadCategories();
        RefreshTriggerList();
        LoadPlaceholders();
        UpdateToggleUI();
        ApplyTheme(_currentTheme);
    }

    #region Theme Management

    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        _currentTheme = _currentTheme switch
        {
            "Light" => "Dark",
            "Dark" => "Auto",
            _ => "Light"
        };
        
        ApplyTheme(_currentTheme);
        SaveThemeSetting();
    }

	private void ApplyTheme(string theme)
	{
		var actualTheme = theme;
		if (theme == "Auto")
		{
			actualTheme = IsSystemDarkMode() ? "Dark" : "Light";
		}

		var resources = Application.Current.Resources;

		if (actualTheme == "Dark")
		{
			resources["PrimaryColor"] = ColorFromHex("#6366F1");
			resources["PrimaryHoverColor"] = ColorFromHex("#818CF8");
			resources["BackgroundColor"] = ColorFromHex("#0F0F0F");
			resources["SurfaceColor"] = ColorFromHex("#1A1A1A");
			resources["SurfaceHoverColor"] = ColorFromHex("#262626");
			resources["BorderColor"] = ColorFromHex("#2E2E2E");
			resources["TextColor"] = ColorFromHex("#FAFAFA");
			resources["TextMutedColor"] = ColorFromHex("#737373");
			resources["DangerColor"] = ColorFromHex("#EF4444");
			resources["SuccessColor"] = ColorFromHex("#22C55E");
			
			// Update brushes
			resources["PrimaryBrush"] = new SolidColorBrush(ColorFromHex("#6366F1"));
			resources["PrimaryHoverBrush"] = new SolidColorBrush(ColorFromHex("#818CF8"));
			resources["BackgroundBrush"] = new SolidColorBrush(ColorFromHex("#0F0F0F"));
			resources["SurfaceBrush"] = new SolidColorBrush(ColorFromHex("#1A1A1A"));
			resources["SurfaceHoverBrush"] = new SolidColorBrush(ColorFromHex("#262626"));
			resources["BorderBrush"] = new SolidColorBrush(ColorFromHex("#2E2E2E"));
			resources["TextBrush"] = new SolidColorBrush(ColorFromHex("#FAFAFA"));
			resources["TextMutedBrush"] = new SolidColorBrush(ColorFromHex("#737373"));
			resources["DangerBrush"] = new SolidColorBrush(ColorFromHex("#EF4444"));
			resources["SuccessBrush"] = new SolidColorBrush(ColorFromHex("#22C55E"));
			
			ThemeButton.Content = theme == "Auto" ? "\u25D0" : "\u263E";
		}
		else
		{
			resources["PrimaryColor"] = ColorFromHex("#2563EB");
			resources["PrimaryHoverColor"] = ColorFromHex("#1D4ED8");
			resources["BackgroundColor"] = ColorFromHex("#FAFAFA");
			resources["SurfaceColor"] = ColorFromHex("#FFFFFF");
			resources["SurfaceHoverColor"] = ColorFromHex("#F5F5F5");
			resources["BorderColor"] = ColorFromHex("#E5E5E5");
			resources["TextColor"] = ColorFromHex("#171717");
			resources["TextMutedColor"] = ColorFromHex("#737373");
			resources["DangerColor"] = ColorFromHex("#DC2626");
			resources["SuccessColor"] = ColorFromHex("#16A34A");
			
			// Update brushes
			resources["PrimaryBrush"] = new SolidColorBrush(ColorFromHex("#2563EB"));
			resources["PrimaryHoverBrush"] = new SolidColorBrush(ColorFromHex("#1D4ED8"));
			resources["BackgroundBrush"] = new SolidColorBrush(ColorFromHex("#FAFAFA"));
			resources["SurfaceBrush"] = new SolidColorBrush(ColorFromHex("#FFFFFF"));
			resources["SurfaceHoverBrush"] = new SolidColorBrush(ColorFromHex("#F5F5F5"));
			resources["BorderBrush"] = new SolidColorBrush(ColorFromHex("#E5E5E5"));
			resources["TextBrush"] = new SolidColorBrush(ColorFromHex("#171717"));
			resources["TextMutedBrush"] = new SolidColorBrush(ColorFromHex("#737373"));
			resources["DangerBrush"] = new SolidColorBrush(ColorFromHex("#DC2626"));
			resources["SuccessBrush"] = new SolidColorBrush(ColorFromHex("#16A34A"));
			
			ThemeButton.Content = theme == "Auto" ? "\u25D0" : "\u2600";
		}

		ThemeButton.ToolTip = $"Theme: {theme}";
	}

	private static System.Windows.Media.Color ColorFromHex(string hex)
	{
		return (System.Windows.Media.Color)ColorConverter.ConvertFromString(hex);
	}

    private static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false;
        }
    }

    private void SaveThemeSetting()
    {
        TriggerDatabase.Instance.Settings.Theme = _currentTheme;
        TriggerDatabase.Instance.Save();
    }

    #endregion

    #region Toggle Management

    private void MainToggle_Click(object sender, RoutedEventArgs e)
    {
        var newState = MainToggle.IsChecked == true;
        ((App)Application.Current).SetKeyboardHookEnabled(newState);
        ((App)Application.Current).UpdateTrayIcon(newState);
        UpdateToggleUI();
    }

    private void UpdateToggleUI()
    {
        var isEnabled = TriggerDatabase.Instance.Settings.IsEnabled;
        MainToggle.IsChecked = isEnabled;
        
        if (isEnabled)
        {
            StatusLabel.Text = "ON";
            StatusLabel.Foreground = (Brush)FindResource("SuccessBrush");
        }
        else
        {
            StatusLabel.Text = "OFF";
            StatusLabel.Foreground = (Brush)FindResource("TextMutedBrush");
        }
    }

    #endregion
	#region Trigger Management

    private void NewTrigger_Click(object sender, RoutedEventArgs e)
    {
        ShowTriggerDialog(null);
    }

    private void EditTrigger_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Trigger trigger)
        {
            ShowTriggerDialog(trigger);
        }
    }

    private void ShowTriggerDialog(Trigger? existingTrigger)
    {
        var dialog = new TriggerDialog(existingTrigger, GetCategories());
        dialog.Owner = this;
        
        if (dialog.ShowDialog() == true)
        {
            if (existingTrigger != null)
            {
                existingTrigger.Input = dialog.TriggerInput;
                existingTrigger.Output = dialog.TriggerOutput;
                existingTrigger.Category = dialog.TriggerCategory;
            }
            else
            {
                TriggerDatabase.Instance.Triggers.Add(new Trigger
                {
                    Input = dialog.TriggerInput,
                    Output = dialog.TriggerOutput,
                    Category = dialog.TriggerCategory
                });
            }

            TriggerDatabase.Instance.Save();
            LoadCategories();
            RefreshTriggerList();
            ((App)Application.Current).RefreshKeyboardHook();
        }
    }

    private List<string> GetCategories()
    {
        return TriggerDatabase.Instance.Triggers
            .Select(t => t.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private void DeleteTrigger_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Trigger trigger)
        {
            var dialog = new ConfirmDialog(
                "Delete Trigger",
                $"Are you sure you want to delete the trigger '{trigger.Input}'?",
                "Delete",
                true);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                TriggerDatabase.Instance.Triggers.Remove(trigger);
                TriggerDatabase.Instance.Save();
                LoadCategories();
                RefreshTriggerList();
                ((App)Application.Current).RefreshKeyboardHook();
            }
        }
    }

    private void LoadCategories()
    {
        var categories = GetCategories();

        CategoryFilter.Items.Clear();
        CategoryFilter.Items.Add("All Categories");
        foreach (var cat in categories)
        {
            CategoryFilter.Items.Add(cat);
        }
        CategoryFilter.SelectedIndex = 0;
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshTriggerList();
    }

    private void RefreshTriggerList()
    {
        var triggers = TriggerDatabase.Instance.Triggers.AsEnumerable();

        if (CategoryFilter.SelectedIndex > 0)
        {
            var selectedCategory = CategoryFilter.SelectedItem?.ToString();
            triggers = triggers.Where(t => t.Category == selectedCategory);
        }

        var triggerList = triggers.OrderBy(t => t.Category).ThenBy(t => t.Input).ToList();
        TriggerList.ItemsSource = triggerList;
        EmptyMessage.Visibility = triggerList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region Info Dialog

    private void Info_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InfoDialog();
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    #endregion

    #region Placeholders

	private void LoadPlaceholders()
	{
		PlaceholderList.ItemsSource = PlaceholderService.AvailablePlaceholders.ToList();
	}

    private async void CopyPlaceholder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string placeholder)
        {
            try
            {
                Clipboard.SetText(placeholder);

                var originalContent = button.Content;
                button.Content = "\u2713";
                button.IsEnabled = false;

                await Task.Delay(800);

                button.Content = originalContent;
                button.IsEnabled = true;
            }
            catch { }
        }
    }

    #endregion

    #region Test Area


	
	private void ClearTestArea_Click(object sender, RoutedEventArgs e)
	{
		TestTextBox.Document.Blocks.Clear();
		TestTextBox.Focus();
	}
	

    #endregion
	#region Settings

    private void SettingsScrollViewer_Loaded(object sender, RoutedEventArgs e)
    {
        SettingsScrollViewer.ScrollToTop();
    }

    private void LoadSettings()
    {
        StartupCheckBox.IsChecked = StartupManager.IsRegisteredForStartup();
        SpeedSlider.Value = TriggerDatabase.Instance.Settings.TriggerSpeed;
        UpdateSpeedText();
        DatabasePathText.Text = TriggerDatabase.DatabasePath;
        DatabaseLocationText.Text = TriggerDatabase.DatabasePath;
        SettingsPathText.Text = TriggerDatabase.SettingsPath;
        
        _currentTheme = TriggerDatabase.Instance.Settings.Theme ?? "Light";
    }

    private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (StartupCheckBox.IsChecked == true)
        {
            StartupManager.RegisterForStartup();
        }
        else
        {
            StartupManager.UnregisterFromStartup();
        }
    }

    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SpeedValueText == null) return;

        TriggerDatabase.Instance.Settings.TriggerSpeed = (int)SpeedSlider.Value;
        TriggerDatabase.Instance.Save();
        UpdateSpeedText();
    }

    private void UpdateSpeedText()
    {
        if (SpeedValueText != null)
        {
            var delay = TriggerDatabase.Instance.Settings.GetDelayMs();
            SpeedValueText.Text = $"Delay: {delay}ms";
        }
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            FileName = "teehee-triggers.json",
            Title = "Export Triggers"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                TriggerDatabase.Instance.Export(dialog.FileName);
                var confirmDialog = new ConfirmDialog("Export Complete", "Triggers exported successfully!", "OK", false, null);
                confirmDialog.Owner = this;
                confirmDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var errorDialog = new ConfirmDialog("Error", $"Export failed: {ex.Message}", "OK", false, null);
                errorDialog.Owner = this;
                errorDialog.ShowDialog();
            }
        }
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Import Triggers"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = System.IO.File.ReadAllText(dialog.FileName);

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
                    throw new Exception("JSON must be an object with a 'Triggers' array");

                if (!root.TryGetProperty("Triggers", out var triggersElement))
                    throw new Exception("Missing required 'Triggers' array");

                if (triggersElement.ValueKind != System.Text.Json.JsonValueKind.Array)
                    throw new Exception("'Triggers' must be an array");

                var mergeDialog = new ConfirmDialog(
                    "Import Mode",
                    "Do you want to merge with existing triggers?\n\nClick 'Merge' to add to existing, or 'Replace' to overwrite all.",
                    "Merge",
                    false,
                    "Replace");
                mergeDialog.Owner = this;

                var result = mergeDialog.ShowDialog();
                if (result == null) return;

                bool merge = result == true;

                TriggerDatabase.Instance.Import(dialog.FileName, merge: merge);
                LoadCategories();
                RefreshTriggerList();
                
                var successDialog = new ConfirmDialog("Import Complete", "Triggers imported successfully!", "OK", false, null);
                successDialog.Owner = this;
                successDialog.ShowDialog();

                ((App)Application.Current).RefreshKeyboardHook();
            }
            catch (System.Text.Json.JsonException ex)
            {
                var errorDialog = new ConfirmDialog("Import Error", $"Invalid JSON file:\n{ex.Message}", "OK", false, null);
                errorDialog.Owner = this;
                errorDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var errorDialog = new ConfirmDialog("Import Error", $"Import failed:\n{ex.Message}", "OK", false, null);
                errorDialog.Owner = this;
                errorDialog.ShowDialog();
            }
        }
    }

    private void BrowseDatabase_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            FileName = "triggers.json",
            Title = "Choose Database Location"
        };

        if (dialog.ShowDialog() == true)
        {
            var oldPath = TriggerDatabase.DatabasePath;

            if (System.IO.File.Exists(oldPath) && oldPath != dialog.FileName)
            {
                if (System.IO.File.Exists(dialog.FileName))
                {
                    var confirmDialog = new ConfirmDialog(
                        "File Exists",
                        "A file already exists at this location. Overwrite it?",
                        "Overwrite",
                        true);
                    confirmDialog.Owner = this;

                    if (confirmDialog.ShowDialog() != true)
                        return;

                    System.IO.File.Delete(dialog.FileName);
                }

                System.IO.File.Move(oldPath, dialog.FileName);
            }

            TriggerDatabase.Instance.SetCustomDatabasePath(dialog.FileName);
            TriggerDatabase.Instance.Load();
            RefreshTriggerList();
            DatabaseLocationText.Text = dialog.FileName;
            DatabasePathText.Text = dialog.FileName;

            ((App)Application.Current).RefreshKeyboardHook();
        }
    }

    private void ResetDatabaseLocation_Click(object sender, RoutedEventArgs e)
    {
        TriggerDatabase.Instance.SetCustomDatabasePath(null);
        TriggerDatabase.Instance.Load();
        RefreshTriggerList();
        DatabaseLocationText.Text = TriggerDatabase.DatabasePath;
        DatabasePathText.Text = TriggerDatabase.DatabasePath;

        ((App)Application.Current).RefreshKeyboardHook();
    }

    private void Author_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/dayeggpi",
                UseShellExecute = true
            });
        }
        catch { }
    }

    #endregion

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Hide();
        e.Cancel = false;
    }
}