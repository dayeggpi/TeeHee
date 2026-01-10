using System.Windows;
using System.Windows.Documents;

namespace TeeHee;

public partial class TriggerDialog : Window
{
    private readonly Trigger? _existingTrigger;
    
    public string TriggerInput { get; private set; } = "";
    public string TriggerOutput { get; private set; } = "";
    public string TriggerCategory { get; private set; } = "";

    public TriggerDialog(Trigger? existingTrigger, List<string> categories)
    {
        InitializeComponent();

InputTextBox.PreviewTextInput += InputTextBox_PreviewTextInput;
InputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
System.Windows.DataObject.AddPastingHandler(InputTextBox, InputTextBox_Pasting);

		
        _existingTrigger = existingTrigger;
        
        CategoryComboBox.Items.Clear();
        CategoryComboBox.Items.Add("");
        foreach (var cat in categories)
        {
            CategoryComboBox.Items.Add(cat);
        }
        CategoryComboBox.SelectedIndex = 0;
        
        if (existingTrigger != null)
        {
            DialogTitle.Text = "Edit Trigger";
            SaveButton.Content = "Update";
            SetRichTextBoxText(InputTextBox, existingTrigger.Input);
            SetRichTextBoxText(OutputTextBox, existingTrigger.Output);
            CategoryComboBox.Text = existingTrigger.Category;
        }
        
        Loaded += (s, e) => InputTextBox.Focus();
    }



// Add these methods:
private void InputTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
{
    var currentText = new TextRange(InputTextBox.Document.ContentStart, InputTextBox.Document.ContentEnd).Text.TrimEnd('\r', '\n');
    if (currentText.Length + e.Text.Length > 62)
    {
        e.Handled = true;
    }
}

private void InputTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
{
    if (e.Key == System.Windows.Input.Key.Space)
    {
        var currentText = new TextRange(InputTextBox.Document.ContentStart, InputTextBox.Document.ContentEnd).Text.TrimEnd('\r', '\n');
        if (currentText.Length >= 62)
        {
            e.Handled = true;
        }
    }
}

private void InputTextBox_Pasting(object sender, System.Windows.DataObjectPastingEventArgs e)
{
    if (e.DataObject.GetDataPresent(typeof(string)))
    {
        var pastedText = (string)e.DataObject.GetData(typeof(string));
        var currentText = new TextRange(InputTextBox.Document.ContentStart, InputTextBox.Document.ContentEnd).Text.TrimEnd('\r', '\n');
        if (currentText.Length + pastedText.Length > 62)
        {
            e.CancelCommand();
        }
    }
}


	private static string GetRichTextBoxText(Emoji.Wpf.RichTextBox rtb)
	{
		var sb = new System.Text.StringBuilder();
		bool firstBlock = true;
		
		foreach (var block in rtb.Document.Blocks)
		{
			if (block is Paragraph paragraph)
			{
				if (!firstBlock)
					sb.AppendLine();
				
				foreach (var inline in paragraph.Inlines)
				{
					if (inline is Run run)
					{
						sb.Append(run.Text);
					}
					else
					{
						// For emoji elements, try to get Text property via reflection
						var textProp = inline.GetType().GetProperty("Text");
						if (textProp != null)
						{
							var text = textProp.GetValue(inline) as string;
							if (!string.IsNullOrEmpty(text))
								sb.Append(text);
						}
					}
				}
				
				firstBlock = false;
			}
		}
		
		return sb.ToString();
	}
	
    private static void SetRichTextBoxText(Emoji.Wpf.RichTextBox rtb, string text)
    {
        rtb.Text = text;
    }
	

    private void Save_Click(object sender, RoutedEventArgs e)
    {


		
        var input = GetRichTextBoxText(InputTextBox).Trim();
        var output = GetRichTextBoxText(OutputTextBox);
        var category = CategoryComboBox.Text?.Trim() ?? "";

		if (input.Length > 62)
		{
			ShowError("Trigger input cannot exceed 62 characters.");
            InputTextBox.Focus();
			return;
		}
		
        if (string.IsNullOrEmpty(input))
        {
            ShowError("Trigger cannot be empty.");
            InputTextBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(output))
        {
            ShowError("Expansion cannot be empty.");
            OutputTextBox.Focus();
            return;
        }

        var duplicate = TriggerDatabase.Instance.Triggers
            .FirstOrDefault(t => t.Input == input && t != _existingTrigger);

        if (duplicate != null)
        {
            ShowError($"A trigger with input '{input}' already exists.");
            InputTextBox.Focus();
            return;
        }

        TriggerInput = input;
        TriggerOutput = output;
        TriggerCategory = category;
        
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorMessage.Visibility = Visibility.Visible;
    }
}