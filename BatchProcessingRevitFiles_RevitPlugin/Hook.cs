using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BatchProcessingRevitFiles_RevitPlugin
{
    internal static class Hook
    {
        #region Hook
        // Например:     
        //    SetupHook();
        //    ...Следующие действия могут привести к появлению диалога, 
        //    ...который необходимо закрыть.
        //    RemoveHook();

        public delegate int HookProc(
              int nCode,
              IntPtr wParam,
              IntPtr lParam);

        // Объявляем метку хука как целое.
        private static int hHook = 0;

        // Объявляем константу хука работы с окнами.
        // Для других типов хуков значения можно найти в
        // файле Winuser.h из Microsoft SDK.
        public const int WH_CBT = 5;

        public const uint WM_CLOSE = 0x0010;
        private const uint WM_COMMAND = 0x0111;
        private const uint WM_DESTROY = 0x0002;
        private static readonly long HCBT_ACTIVATE = 5;
        private static readonly long HCBT_CREATEWND = 3;
        private static HookProc CBTHookProcedure_;

        // Объявление функции SetWindowsHookEx.
        // Она устанавливает хуки.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        // Объявление функции UnhookWindowsHookEx.
        // Вызов ее убирает хуки.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // Объявление функции CallNextHookEx.
        // Вызов ее передает информации следующей функции в цепочке хуков 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static int CBTHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                StringBuilder wndName = new StringBuilder(300);
                GetWindowText(GetActiveWindow(), wndName, 300);

                if (wndName.ToString().Contains("Потеря") || wndName.ToString() == "Revit" || wndName.ToString() == "Модуль экспорта Navisworks NWC")
                {
                    SendMessage(GetActiveWindow(), WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        public static void SetupHook()
        {
            if (hHook == 0)
            {
                // Создаём экземпляр процедуры-обработчика хука.
                CBTHookProcedure_ = new HookProc(CBTHookProcedure);

                hHook = SetWindowsHookEx(WH_CBT, CBTHookProcedure_, (IntPtr)0, Thread.CurrentThread.ManagedThreadId);

                // В случае ошибки SetWindowsHookEx.
                if (hHook == 0)
                {
                    return;
                }
            }
        }

        public static void RemoveHook()
        {
            if (hHook != 0)
            {
                bool ret = UnhookWindowsHookEx(hHook);
                // В случае ошибки UnhookWindowsHookEx.
                if (ret)
                {
                    hHook = 0;
                }
            }
        }
        #endregion
    }
}
