using System.Windows;

namespace TeeHee;

public partial class InfoDialog : Window
{
    public InfoDialog()
    {
        InitializeComponent();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}