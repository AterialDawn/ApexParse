using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ApexParse.Utility
{
    //updated to use win32 registerhotkey system instead of low level kb hooking
    class HotkeyUtil
    {
        private const int WM_HOTKEY = 786;
        private IntPtr _windowHandle;
        private List<KeyContainer> RegisteredHokeys = new List<KeyContainer>();

        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, KeyModifiers modifiers, Keys vKeyCode);

        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        Dispatcher appDispatcher;

        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        int keyCounter = 0;

        public HotkeyUtil(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
            appDispatcher = App.Current.Dispatcher;
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != WM_HOTKEY) return;
            int hotkeyId = msg.wParam.ToInt32();
            Console.WriteLine($"Hotkey {hotkeyId} is being called");
            var hotkey = RegisteredHokeys.Where(c => c.HotkeyId == hotkeyId).FirstOrDefault();
            if (hotkey != null)
            {
                appDispatcher.BeginInvoke(new Action(() => { hotkey.SafeFireCallback(hotkey.UserObject); }));
            }
        }

        public bool RegisterHotkey(KeyContainer Container)
        {
            //Check if hotkey is already registered, else add
            if (IsHotkeyRegistered(Container)) return false;
            Container.HotkeyId = ++keyCounter;
            //New key, register it.
            var retVal = RegisterHotKey(_windowHandle, Container.HotkeyId, Container.GetKeyModifiers(), Container.Key);
            Console.WriteLine($"RegisterHotKey for id {Container.HotkeyId} returned {retVal}");
            if (retVal == 0) return false;
            RegisteredHokeys.Add(Container);
            return true;
        }

        public bool IsHotkeyRegistered(KeyContainer Container)
        {
            foreach (KeyContainer CurKey in RegisteredHokeys)
            {
                if (CurKey.Equals(Container)) return true;
            }
            return false;
        }

        public class KeyContainer : EventArgs
        {
            public Keys Key { get; private set; }
            public bool Shift { get; private set; }
            public bool Ctrl { get; private set; }
            public Action<object> HotkeyCallback { get; private set; }
            public object UserObject { get; private set; }
            public int HotkeyId { get; set; } = -1;

            public KeyContainer(Keys key, bool shift, bool ctrl, Action<object> callback = null, object userObject = null)
            {
                Key = key;
                Shift = shift;
                Ctrl = ctrl;
                HotkeyCallback = callback;
                UserObject = userObject;
            }

            //safe as in swallow all exceptions lmao
            public void SafeFireCallback(object userState)
            {
                if (HotkeyCallback == null) return;
                try
                {
                    HotkeyCallback(userState);
                }
                catch { }
            }

            public HotkeyUtil.KeyModifiers GetKeyModifiers()
            {
                var mod = new HotkeyUtil.KeyModifiers();
                mod |= Shift ? HotkeyUtil.KeyModifiers.Shift : HotkeyUtil.KeyModifiers.None;
                mod |= Ctrl ? HotkeyUtil.KeyModifiers.Control : HotkeyUtil.KeyModifiers.None;
                return mod;
            }

            public bool Equals(KeyContainer OtherContainer)
            {
                return ((OtherContainer.Key == this.Key) && (OtherContainer.Shift == this.Shift) && (OtherContainer.Ctrl == this.Ctrl));
            }
        }
    }
}
