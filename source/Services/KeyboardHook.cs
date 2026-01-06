using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
namespace TeeHee;

public class KeyboardHook
{

    public static bool DebugLogging { get; set; } = false;
    // then anywhere in the app, i can use : KeyboardHook.DebugLogging = true;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    private LowLevelKeyboardProc? _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private string _buffer = "";
    private bool _isReplacing = false;

    public bool IsEnabled { get; set; } = true;
    
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll")]
    private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    public void Start()
    {
        _proc = HookCallback;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(module!.ModuleName!), 0);
        
        Log($"Hook installed: {_hookId != IntPtr.Zero}");
    }

    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }



    private void Log(string message)
    {
        if (!DebugLogging) return;
        
        try
        {
            File.AppendAllText(@"C:\temp\teehee_debug.log", $"{DateTime.Now:HH:mm:ss.fff} {message}\n");
        }
        catch { }
    }
        

    public void RefreshTriggerCache()
    {
        // Triggers are read directly from database each time
    }



    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        int vkCode = Marshal.ReadInt32(lParam);
        Log($"HookCallback: nCode={nCode}, wParam={wParam}, vkCode={vkCode}, IsEnabled={IsEnabled}");

        if (nCode < 0 || wParam != (IntPtr)WM_KEYDOWN || _isReplacing || !IsEnabled)
        {
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        Log($"Processing KEYDOWN: vkCode={vkCode} (0x{vkCode:X2})");

        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN && !_isReplacing)
        {
            
            
            Log($"Key pressed: {vkCode}");

            // Backspace
            if (vkCode == 0x08)
            {
                if (_buffer.Length > 0)
                    _buffer = _buffer.Substring(0, _buffer.Length - 1);
                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            // Reset on Enter, Tab, Escape
            if (vkCode == 0x0D || vkCode == 0x09 || vkCode == 0x1B)
            {
                _buffer = "";
                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            // Space - check trigger first, then reset
            if (vkCode == 0x20)
            {
                _buffer = "";
                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            char? c = VkCodeToChar(vkCode);
            Log($"VkCode: {vkCode} (0x{vkCode:X2}) -> Char: {(c.HasValue ? $"'{c.Value}' (0x{((int)c.Value):X4})" : "null")}");

            if (c.HasValue)
            {
                _buffer += c.Value;
                Log($"Buffer: '{_buffer}' (bytes: {string.Join(" ", _buffer.Select(x => $"0x{((int)x):X4}"))})");

                // Keep buffer small
                if (_buffer.Length > 50)
                    _buffer = _buffer.Substring(_buffer.Length - 50);

                // Check triggers
                var triggers = TriggerDatabase.Instance.GetTriggerDictionary();
                foreach (var kvp in triggers)
                {
                    Log($"Comparing buffer '{_buffer}' with trigger '{kvp.Key}'");
                    Log($"  Buffer bytes: {string.Join(" ", _buffer.Select(x => $"0x{((int)x):X4}"))}");
                    Log($"  Trigger bytes: {string.Join(" ", kvp.Key.Select(x => $"0x{((int)x):X4}"))}");
                    Log($"  EndsWith result: {_buffer.EndsWith(kvp.Key, StringComparison.Ordinal)}");
                    
                    if (_buffer.EndsWith(kvp.Key, StringComparison.Ordinal))
        
                    {
                        Log($"Trigger matched: {kvp.Key} -> {kvp.Value}");
                        
                        int len = kvp.Key.Length;
                        string replacement = kvp.Value;
                        _buffer = "";
                        _isReplacing = true;

                        // Let current key through, then replace
                        Task.Run(() =>
                        {
                            Thread.Sleep(50);
                            
                            // Delete trigger
                            for (int i = 0; i < len; i++)
                            {
                                SendKey(0x08); // Backspace
                                Thread.Sleep(10);
                            }

                            Thread.Sleep(20);

                            // Process placeholders
                            string processed = PlaceholderService.Process(replacement);
                            
                            // Paste via clipboard
                            PasteText(processed);

                            Thread.Sleep(30);
                            _isReplacing = false;
                        });

                        break;
                    }
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }



    private void PasteText(string text)
    {
        // Save current clipboard
        string? oldClipboard = null;
        Thread thread = new Thread(() =>
        {
            try
            {
                if (System.Windows.Clipboard.ContainsText())
                {
                    oldClipboard = System.Windows.Clipboard.GetText();
                }
                System.Windows.Clipboard.SetText(text);
            }
            catch { }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join(500);

        Thread.Sleep(30);

        // Send Ctrl+V
        INPUT[] inputs = new INPUT[4];
        
        // Ctrl down
        inputs[0].type = 1;
        inputs[0].ki.wVk = 0x11; // VK_CONTROL
        
        // V down
        inputs[1].type = 1;
        inputs[1].ki.wVk = 0x56; // V
        
        // V up
        inputs[2].type = 1;
        inputs[2].ki.wVk = 0x56;
        inputs[2].ki.dwFlags = 0x0002;
        
        // Ctrl up
        inputs[3].type = 1;
        inputs[3].ki.wVk = 0x11;
        inputs[3].ki.dwFlags = 0x0002;
        
        SendInput(4, inputs, Marshal.SizeOf<INPUT>());

        // Restore old clipboard
        if (oldClipboard != null)
        {
            Thread.Sleep(100);
            Thread restoreThread = new Thread(() =>
            {
                try
                {
                    System.Windows.Clipboard.SetText(oldClipboard);
                }
                catch { }
            });
            restoreThread.SetApartmentState(ApartmentState.STA);
            restoreThread.Start();
            restoreThread.Join(500);
        }
    }


    private char? VkCodeToChar(int vkCode)
    {
        // Skip modifier keys themselves
        if (vkCode == 0xA0 || vkCode == 0xA1 || // Shift
            vkCode == 0xA2 || vkCode == 0xA3 || // Ctrl
            vkCode == 0xA4 || vkCode == 0xA5 || // Alt
            vkCode == 0x10 || vkCode == 0x11 || vkCode == 0x12) // Generic Shift/Ctrl/Alt
        {
            return null;
        }

        byte[] keyboardState = new byte[256];
        
        // Get the current state of all keys
        for (int i = 0; i < 256; i++)
        {
            short state = GetAsyncKeyState(i);
            // High bit set means key is currently down
            if ((state & 0x8000) != 0)
            {
                keyboardState[i] = 0x80;
            }
            // Low bit set means key was pressed since last call (toggle state for caps lock etc)
            if ((state & 0x0001) != 0 && (i == 0x14)) // Caps Lock
            {
                keyboardState[i] |= 0x01;
            }
        }

        uint scanCode = MapVirtualKey((uint)vkCode, 0);
        StringBuilder sb = new StringBuilder(5);

        int result = ToUnicode((uint)vkCode, scanCode, keyboardState, sb, sb.Capacity, 0);
        
        if (result == -1)
        {
            // Dead key - call again to clear
            sb.Clear();
            result = ToUnicode((uint)vkCode, scanCode, keyboardState, sb, sb.Capacity, 0);
        }
        
        if (result == 1)
        {
            Log($"VkCode {vkCode} -> '{sb[0]}' (0x{((int)sb[0]):X4})");
            return sb[0];
        }

        return null;
    }
    

    private void SendKey(ushort vk)
    {
        INPUT[] inputs = new INPUT[2];
        inputs[0].type = 1;
        inputs[0].ki.wVk = vk;
        inputs[1].type = 1;
        inputs[1].ki.wVk = vk;
        inputs[1].ki.dwFlags = 0x0002;
        SendInput(2, inputs, Marshal.SizeOf<INPUT>());
    }

    private void SendChar(char c)
    {
        INPUT[] inputs = new INPUT[2];
        inputs[0].type = 1;
        inputs[0].ki.wScan = c;
        inputs[0].ki.dwFlags = 0x0004;
        inputs[1].type = 1;
        inputs[1].ki.wScan = c;
        inputs[1].ki.dwFlags = 0x0004 | 0x0002;
        SendInput(2, inputs, Marshal.SizeOf<INPUT>());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
        public uint pad1;
        public uint pad2;
    }
}