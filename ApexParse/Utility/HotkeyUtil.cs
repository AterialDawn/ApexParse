using MouseKeyboardActivityMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApexParse.Utility
{
    //old shitty hotkey util class. works well enough i guess.
    class HotkeyUtil
    {
        private KeyboardHookListener kbListener = new KeyboardHookListener(new MouseKeyboardActivityMonitor.WinApi.GlobalHooker());
        private List<KeyContainer> RegisteredHokeys = new List<KeyContainer>();

        private bool ShiftState = false, CtrlState = false;

        public HotkeyUtil()
        {
            kbListener.KeyDown += KbListener_KeyDown;
            kbListener.KeyUp += KbListener_KeyUp;
            kbListener.Enabled = true;
        }

        private void KbListener_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateKeyState(e.KeyCode, false);
        }

        private void KbListener_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateKeyState(e.KeyCode, true);
        }

        private void UpdateKeyState(Keys keyCode, bool pressed)
        {
            //If key is Shift or Ctrl, set flags and return
            if ((keyCode == Keys.LShiftKey) || (keyCode == Keys.RShiftKey) || (keyCode == Keys.ShiftKey) || (keyCode == Keys.Shift))
            {
                ShiftState = pressed;
            }
            else if ((keyCode == Keys.LControlKey) || (keyCode == Keys.RControlKey) || (keyCode == Keys.ControlKey) || (keyCode == Keys.Control))
            {
                CtrlState = pressed;
            }
            else if(pressed)
            {
                //Check if the key is a previously registered hotkey and handle accordingly
                foreach (KeyContainer KeyCheck in RegisteredHokeys)
                {
                    if (KeyCheck.Key == keyCode)
                    {
                        if (KeyCheck.Ctrl == CtrlState && KeyCheck.Shift == ShiftState)
                        {
                            KeyCheck.SafeFireCallback(KeyCheck.UserObject);
                        }
                    }
                }
            }
        }

        public bool RegisterHotkey(KeyContainer Container)
        {
            //Check if hotkey is already registered, else add
            if (IsHotkeyRegistered(Container)) return false;
            //New key, register it.
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
    }

    public class KeyContainer : EventArgs
    {
        public Keys Key { get; private set; }
        public bool Shift { get; private set; }
        public bool Ctrl { get; private set; }
        public Action<object> HotkeyCallback { get; private set; }
        public object UserObject { get; private set; }

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

        public bool Equals(KeyContainer OtherContainer)
        {
            return ((OtherContainer.Key == this.Key) && (OtherContainer.Shift == this.Shift) && (OtherContainer.Ctrl == this.Ctrl));
        }
    }
}
