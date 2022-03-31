using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.Windows.Input;
using System.Diagnostics;

namespace KeyboardHook
{
    public partial class Form1 : Form
    {
        LowLevelKeyboardListener KbHook;
        public Form1()
        {
            InitializeComponent();
            KbHook = new LowLevelKeyboardListener();
            KbHook.HookKey += KbHook_pressed;

            KbHook.HookKeyboard();
        }

        //получение самой кнопки
        public class KeyPressedArgs : EventArgs
        {
            public Key KeyPressed { get; private set; }

            public KeyPressedArgs(Key key)
            {
                KeyPressed = key;
            }
        }

        void KbHook_pressed(object sender, KeyPressedArgs e)
        {
            textBox1.Text += "Was pressed: " + e.KeyPressed.ToString() + Environment.NewLine;
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // убираем хук при выключении приложения
            KbHook.UnHookKeyboard();
        }

        public class LowLevelKeyboardListener
        {
            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int WM_SYSKEYDOWN = 0x0104;


            //Получение всех нужных функций
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
            //Получение всех нужных функций


            public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            public event EventHandler<KeyPressedArgs> HookKey;

            private LowLevelKeyboardProc keyhook;
            private IntPtr hookID = IntPtr.Zero;

            //Отслеживание кнопки для хука
            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)//для вывода единожды
                {
                    int CodeRead = Marshal.ReadInt32(lParam);//Чтение самой кнопки

                    if (HookKey != null) { HookKey(this, new KeyPressedArgs(KeyInterop.KeyFromVirtualKey(CodeRead))); }
                }

                return CallNextHookEx(hookID, nCode, wParam, lParam);
            }
            public LowLevelKeyboardListener()
            {
                keyhook = HookCallback;
            }

            //Вывод id hook для закрытия хука по завершению программы
            private IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            public void HookKeyboard()
            {
                hookID = SetHook(keyhook);
            }

            //Отключение хука
            public void UnHookKeyboard()
            {
                UnhookWindowsHookEx(hookID);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
