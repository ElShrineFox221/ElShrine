using System.Runtime.InteropServices;

namespace ElShrine.Common.Input
{
    public static class InputManager
    {


        
    }
    
    
    /*public partial class KeyboardHook
    {
        public event KeyEventHandler KeyDownEvent;
        public event KeyPressEventHandler KeyPressEvent;
        public event KeyEventHandler KeyUpEvent;

        private delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        private static int hKeyboardHook = 0;
        private const int WH_KEYBOARD_LL = 13;
        private HookProc KeyboardHookProcedure;

        [StructLayout(LayoutKind.Sequential)]
        private class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [LibraryImport("user32.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        private static partial int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [LibraryImport("user32.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);

        public void Start()
        {
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败");
                }
            }
        }

        public void Stop()
        {
            if (hKeyboardHook != 0)
            {
                bool retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
                if (!retKeyboard) throw new Exception("卸载钩子失败！");
            }
        }

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                if (KeyDownEvent != null && (wParam == 0x100 || wParam == 0x104))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyDownEvent(this, e);
                }
                if (KeyPressEvent != null && wParam == 0x100)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);
                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPressEvent(this, e);
                    }
                }
                if (KeyUpEvent != null && (wParam == 0x101 || wParam == 0x105))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyUpEvent(this, e);
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(byte[] pbKeyState);
    }*/
}
