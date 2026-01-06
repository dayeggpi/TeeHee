namespace TeeHee;

public static class PlaceholderService
{
    public static string Process(string text)
    {
        var result = text;

        // Process escape sequences
        result = result.Replace("\\n", "\n");
        result = result.Replace("\\r", "\r");
        result = result.Replace("\\t", "\t");

        // Date/Time placeholders
        var now = DateTime.Now;
        
        result = result.Replace("{{date}}", now.ToString("dd/MM/yyyy"));
        result = result.Replace("{{date-us}}", now.ToString("MM/dd/yyyy"));
        result = result.Replace("{{date-iso}}", now.ToString("yyyy-MM-dd"));
        result = result.Replace("{{time}}", now.ToString("HH:mm"));
        result = result.Replace("{{time12}}", now.ToString("hh:mm tt"));
        result = result.Replace("{{datetime}}", now.ToString("dd/MM/yyyy HH:mm"));
        result = result.Replace("{{day}}", now.ToString("dddd"));
        result = result.Replace("{{month}}", now.ToString("MMMM"));
        result = result.Replace("{{year}}", now.ToString("yyyy"));
        result = result.Replace("{{week}}", GetWeekNumber(now).ToString());

        // Relative dates
        result = result.Replace("{{yesterday}}", now.AddDays(-1).ToString("dd/MM/yyyy"));
        result = result.Replace("{{tomorrow}}", now.AddDays(1).ToString("dd/MM/yyyy"));
        result = result.Replace("{{lastweek}}", now.AddDays(-7).ToString("dd/MM/yyyy"));
        result = result.Replace("{{nextweek}}", now.AddDays(7).ToString("dd/MM/yyyy"));

        // System placeholders
        result = result.Replace("{{user}}", Environment.UserName);
        result = result.Replace("{{computer}}", Environment.MachineName);
        result = result.Replace("{{clipboard}}", GetClipboardText());

        // Random placeholders
        result = result.Replace("{{uuid}}", Guid.NewGuid().ToString());
        result = result.Replace("{{random}}", new Random().Next(1000, 9999).ToString());

        return result;
    }

    private static int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date, 
            System.Globalization.CalendarWeekRule.FirstFourDayWeek, 
            DayOfWeek.Monday);
    }

    private static string GetClipboardText()
    {
        string result = "";
        try
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Clipboard.ContainsText())
                    {
                        result = System.Windows.Clipboard.GetText();
                    }
                }
                catch { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(500); // Wait max 500ms
        }
        catch { }
        return result;
    }

    // For displaying available placeholders in UI
    public static readonly Dictionary<string, string> AvailablePlaceholders = new()
    {
        { "{{date}}", "Current date (DD/MM/YYYY)" },
        { "{{date-us}}", "Current date (MM/DD/YYYY)" },
        { "{{date-iso}}", "Current date (YYYY-MM-DD)" },
        { "{{time}}", "Current time 24h (HH:MM)" },
        { "{{time12}}", "Current time 12h (HH:MM AM/PM)" },
        { "{{datetime}}", "Date and time (DD/MM/YYYY HH:MM)" },
        { "{{day}}", "Day name (Monday, Tuesday...)" },
        { "{{month}}", "Month name (January, February...)" },
        { "{{year}}", "Current year (YYYY)" },
        { "{{week}}", "Week number" },
        { "{{yesterday}}", "Yesterday's date" },
        { "{{tomorrow}}", "Tomorrow's date" },
        { "{{lastweek}}", "Date 7 days ago" },
        { "{{nextweek}}", "Date in 7 days" },
        { "{{user}}", "Windows username" },
        { "{{computer}}", "Computer name" },
        { "{{clipboard}}", "Current clipboard content" },
        { "{{uuid}}", "Random UUID" },
        { "{{random}}", "Random 4-digit number" },
    };
}