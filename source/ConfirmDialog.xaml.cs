using System.Windows;

namespace TeeHee;

public partial class ConfirmDialog : Window
{
    private readonly string? _secondaryButtonText;

    public ConfirmDialog(string title, string message, string primaryButtonText, bool isDanger, string? secondaryButtonText = "Cancel")
    {
        InitializeComponent();
        
        _secondaryButtonText = secondaryButtonText;
        
        TitleText.Text = title;
        MessageText.Text = message;
        PrimaryActionButton.Content = primaryButtonText;
        
        if (isDanger)
        {
            PrimaryActionButton.Style = (Style)FindResource("DangerButton");
            PrimaryActionButton.Padding = new Thickness(14, 8, 14, 8);
        }
        
        if (secondaryButtonText != null)
        {
            SecondaryActionButton.Content = secondaryButtonText;
            SecondaryActionButton.Visibility = Visibility.Visible;
        }
        else
        {
            SecondaryActionButton.Visibility = Visibility.Collapsed;
        }
    }

    private void PrimaryAction_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void SecondaryAction_Click(object sender, RoutedEventArgs e)
    {
        if (_secondaryButtonText != "Cancel")
        {
            DialogResult = false;
        }
        Close();
    }
}