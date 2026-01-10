using System.Windows;
using System.IO;

namespace TeeHee;

public partial class SettingsLocationDialog : Window
{
    public SettingsLocationMode SelectedMode { get; private set; }
    public string? CustomPath { get; private set; }
    
    public bool ShowCancelButton
    {
        get => CancelButton.Visibility == Visibility.Visible;
        set => CancelButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public SettingsLocationDialog()
    {
        InitializeComponent();
    }

	private void CustomRadio_Checked(object sender, RoutedEventArgs e)
	{
		if (CustomPathTextBox != null)
			CustomPathTextBox.IsEnabled = true;
		if (BrowseButton != null)
			BrowseButton.IsEnabled = true;
	}
		
	private void CustomRadio_Unchecked(object sender, RoutedEventArgs e)
	{
		if (CustomPathTextBox != null)
			CustomPathTextBox.IsEnabled = false;
		if (BrowseButton != null)
			BrowseButton.IsEnabled = false;
	}

	private void BrowseButton_Click(object sender, RoutedEventArgs e)
	{
		var dialog = new System.Windows.Forms.FolderBrowserDialog
		{
			Description = "Select folder for TeeHee triggers",
			UseDescriptionForTitle = true
		};

		if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			CustomPathTextBox.Text = dialog.SelectedPath;
			ErrorMessage.Visibility = Visibility.Collapsed;
		}
	}

    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorMessage.Visibility = Visibility.Visible;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (PortableRadio.IsChecked == true)
        {
            SelectedMode = SettingsLocationMode.Portable;
        }
        else if (AppDataRadio.IsChecked == true)
        {
            SelectedMode = SettingsLocationMode.AppData;
        }
        else if (CustomRadio.IsChecked == true)
        {
            if (string.IsNullOrWhiteSpace(CustomPathTextBox.Text))
            {
                ShowError("Please select a custom folder.");
                return;
            }
            SelectedMode = SettingsLocationMode.Custom;
            CustomPath = CustomPathTextBox.Text;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}