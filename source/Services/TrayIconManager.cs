using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TeeHee;

public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private Icon? _enabledIcon;
    private Icon? _disabledIcon;
    private ToolStripMenuItem? _toggleItem;
    
    public event Action? OnOpenRequested;
    public event Action? OnExitRequested;
    public event Action<bool>? OnToggleRequested;

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "TeeHee - Text Expander",
            Visible = true
        };

        // Load or create icons
        LoadIcons();
        UpdateIcon(TriggerDatabase.Instance.Settings.IsEnabled);

        var contextMenu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem("Open TeeHee");
        openItem.Click += (s, e) => OnOpenRequested?.Invoke();
        openItem.Font = new Font(openItem.Font, FontStyle.Bold);
        contextMenu.Items.Add(openItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        _toggleItem = new ToolStripMenuItem("Enabled");
        _toggleItem.Checked = TriggerDatabase.Instance.Settings.IsEnabled;
        _toggleItem.Click += (s, e) => 
        {
            var newState = !_toggleItem.Checked;
            _toggleItem.Checked = newState;
            UpdateIcon(newState);
            OnToggleRequested?.Invoke(newState);
        };
        contextMenu.Items.Add(_toggleItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => OnExitRequested?.Invoke();
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => OnOpenRequested?.Invoke();
    }

    private void LoadIcons()
    {
        // Try to load from embedded resource
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("app.ico", StringComparison.OrdinalIgnoreCase));
            
            if (resourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    _enabledIcon = new Icon(stream);
                }
            }
        }
        catch { }

        // Fallback to file
        if (_enabledIcon == null)
        {
            try
            {
                string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "app.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    _enabledIcon = new Icon(iconPath);
                }
            }
            catch { }
        }

        // Fallback to exe icon
        if (_enabledIcon == null)
        {
            try
            {
                string? exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    _enabledIcon = Icon.ExtractAssociatedIcon(exePath);
                }
            }
            catch { }
        }

        // Create disabled icon (grayscale version)
        if (_enabledIcon != null)
        {
            _disabledIcon = CreateGrayscaleIcon(_enabledIcon);
        }
        else
        {
            // Create default icons
            _enabledIcon = CreateColorIcon(Color.FromArgb(99, 102, 241));
            _disabledIcon = CreateColorIcon(Color.Gray);
        }
    }

    private Icon CreateColorIcon(Color color)
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.Clear(color);
        return Icon.FromHandle(bmp.GetHicon());
    }

    private Icon CreateGrayscaleIcon(Icon original)
    {
        try
        {
            using var bmp = original.ToBitmap();
            using var grayBmp = new Bitmap(bmp.Width, bmp.Height);
            
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    grayBmp.SetPixel(x, y, Color.FromArgb(pixel.A, gray, gray, gray));
                }
            }
            
            return Icon.FromHandle(grayBmp.GetHicon());
        }
        catch
        {
            return CreateColorIcon(Color.Gray);
        }
    }

    public void UpdateIcon(bool enabled)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Icon = enabled ? _enabledIcon : _disabledIcon;
            _notifyIcon.Text = enabled ? "TeeHee - Active" : "TeeHee - Paused";
        }
    }

    public void UpdateToggleState(bool enabled)
    {
        if (_toggleItem != null)
        {
            _toggleItem.Checked = enabled;
        }
        UpdateIcon(enabled);
    }

    public void Dispose()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
        _enabledIcon?.Dispose();
        _disabledIcon?.Dispose();
    }
}