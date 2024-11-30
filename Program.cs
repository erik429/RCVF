using Newtonsoft.Json.Linq;
using QueryMaster;
using QueryMaster.GameServer;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        string serverIp = "";
        ushort queryPort = 0;
        ushort gamePort = 0;
        bool serverUp = false;
        bool shouldRestart = false;
        int delay = 500;
        int timeout = 200;
        string token = "your_battlemetrics_token";
        Console.Title = "WHY YOU COME FAST";
        string[] lines = new[]
        {
            "                                                                                     ",
            " @@@@@@@   @@@@@@   @@@@@@@@@@   @@@@@@@@     @@@@@@@@   @@@@@@    @@@@@@   @@@@@@@  ",
            "@@@@@@@@  @@@@@@@@  @@@@@@@@@@@  @@@@@@@@     @@@@@@@@  @@@@@@@@  @@@@@@@   @@@@@@@  ",
            "!@@       @@!  @@@  @@! @@! @@!  @@!          @@!       @@!  @@@  !@@         @@!    ",
            "!@!       !@!  @!@  !@! !@! !@!  !@!          !@!       !@!  @!@  !@!         !@!    ",
            "!@!       @!@  !@!  @!! !!@ @!@  @!!!:!       @!!!:!    @!@!@!@!  !!@@!!      @!!    ",
            "!!!       !@!  !!!  !@!   ! !@!  !!!!!:       !!!!!:    !!!@!!!!   !!@!!!     !!!    ",
            ":!!       !!:  !!!  !!:     !!:  !!:          !!:       !!:  !!!       !:!    !!:    ",
            ":!:       :!:  !:!  :!:     :!:  :!:          :!:       :!:  !:!      !:!     :!:    ",
            " ::: :::  ::::: ::  :::     ::    :: ::::      ::       ::   :::  :::: ::      ::    ",
            " :: :: :   : :  :    :      :    : :: ::       :         :   : :  :: : :       :     ",
        };
        ConsoleColor[] gradient = new[]
        {
            ConsoleColor.Magenta,
            ConsoleColor.Magenta,
            ConsoleColor.Magenta,
            ConsoleColor.Magenta,
            ConsoleColor.Magenta,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkMagenta
        };
        for (int i = 0; i < lines.Length; i++)
        {
            int colorIndex = i * (gradient.Length - 1) / (lines.Length - 1);
            Console.ForegroundColor = gradient[colorIndex];
            Console.WriteLine(lines[i]);
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("My steam https://steamcommunity.com/id/34211155035578432/\nMade by Bing cilling :>");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nHave Rust running \nDon’t have the console open \nConsole bound to F1 (default)");
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\nPaste the BattleMetrics server URL: > ");
            string url = Console.ReadLine();
            if (url == "localhost")
            {
                serverIp = "127.0.0.1";
                gamePort = 28015;
                queryPort = 28016;
                shouldRestart = true;
                break;
            }
            else
            {
            string serverId = ExtractServerIdFromUrl(url);
            Console.WriteLine("Fetching server details...\n");
            var (fetchedIp, fetchedGamePort, fetchedQueryPort, playersOnline, timeUntilWipe) =
                await GetServerDetailsFromBattleMetrics(serverId, token);
            if (fetchedIp != null)
            {
                serverIp = fetchedIp;
                gamePort = fetchedGamePort;
                queryPort = fetchedQueryPort;
                var info = PingServer(serverIp, queryPort, timeout);
                if (info != null)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Status: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("ONLINE\n");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Name: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(info.Name + "\n");
                    shouldRestart = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Status: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("OFFLINE\n");
                    shouldRestart = false;
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("IP: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(serverIp + "\n");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Game Port: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(gamePort + "\n");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Query Port: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(queryPort + "\n");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Players Online: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(playersOnline + "\n");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Next Wipe: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(timeUntilWipe + "\n");
                if (!IsRustRunning())
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Rust client: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Not Running\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Rust client: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Running\n");
                }
                break;
            }
            else
            {
                Console.WriteLine("Failed to retrieve server details from battlemetrics.");
            }
            }
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("\nPress any key to start!");
        Console.ReadKey();
        Console.Write("\nStarting...");
        Console.Write("\n");
        double elapsedMilliseconds = 0;
        double pingsPerSecond = 0;
        Console.Title = $"delay: {delay.ToString()} ms  timeout: {timeout.ToString()} ms";
        while (true) 
        {
            try
            {

                var startTime = DateTime.Now;
                var info = PingServer(serverIp, queryPort, timeout);
                if (info != null && !serverUp)
                {
                    if (shouldRestart)
                    {
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Ping: SERVER IS ONLINE!  |  Time: {DateTime.Now}  |  PINGS/SECOND: {pingsPerSecond:F1}");
                    }
                    else
                    {
                        serverUp = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"SERVER IS ONLINE!");
                        FocusRustClient(serverIp, gamePort);
                        break;
                    }
                }
                else if (info == null)
                {
                    shouldRestart = false;
                    Console.Title = $"delay: {delay.ToString()} ms  timeout: {timeout.ToString()} ms";
                    delay = 20;
                    timeout = 50;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Ping: SERVER IS OFFLINE!  |  Time: {DateTime.Now}  |  PINGS/SECOND: {pingsPerSecond:F1}");
                }
                Thread.Sleep(delay);
                var endTime = DateTime.Now;
                elapsedMilliseconds = (endTime - startTime).TotalMilliseconds;
                pingsPerSecond = 1000.0 / (elapsedMilliseconds);
            }
            catch
            {
               
            }
        }
    }
    static ServerInfo PingServer(string serverIp, ushort queryPort, int timeout)
    {
        try
        {
            var server = ServerQuery.GetServerInstance(
                EngineType.Source,
                serverIp,
                queryPort,
                false,
                timeout,
                timeout,
                1
            );

            var info = server.GetInfo();
            return info; 
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error pinging server: {ex.Message}");
        }

        return null;
    }
    static string ExtractServerIdFromUrl(string url)
    {
        try
        {
            var match = Regex.Match(url, @"https://www\.battlemetrics\.com/servers/[^/]+/(\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error extracting server ID: {ex.Message}");
        }
        return null;
    }
    static async Task<(string, ushort, ushort, int, string)> GetServerDetailsFromBattleMetrics(string serverId, string token)
    {
        string apiUrl = $"https://api.battlemetrics.com/servers/{serverId}";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                var attributes = data["data"]["attributes"];
                string ip = attributes["ip"].ToString();
                ushort gamePort = ushort.Parse(attributes["port"].ToString());
                ushort queryPort = ushort.Parse(attributes["portQuery"].ToString());
                int playersOnline = int.Parse(attributes["players"].ToString());
                string nextWipe = attributes["details"]?["rust_next_wipe"]?.ToString();
                string timeUntilWipe = "Unknown";

                if (!string.IsNullOrEmpty(nextWipe) && DateTime.TryParse(nextWipe, out DateTime wipeTime))
                {
                    TimeSpan timeRemaining = wipeTime - DateTime.UtcNow;

                    if (timeRemaining.TotalSeconds > 0)
                    {
                        timeUntilWipe = $"{(int)timeRemaining.TotalHours:D2}h:{timeRemaining.Minutes:D2}m";
                    }
                    else
                    {
                        timeUntilWipe = "Wipe has already occurred.";
                    }
                }

                return (ip, gamePort, queryPort, playersOnline, timeUntilWipe);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error fetching server details: {ex.Message}");
        }

        return (null, 0, 0, 0, "Unknown");
    }
    static void SetClipboardText(string text)
    {
        Thread staThread = new Thread(() =>
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error setting clipboard text: {ex.Message}");
                Console.ResetColor();
            }
        });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }
    private static bool IsRustRunning()
    {
        return Process.GetProcessesByName("RustClient").Length > 0;
    }
    static void SendInput(string ip, int port)
    {
        try
        {
        Console.ForegroundColor = ConsoleColor.Yellow;
        string connectCommand = $"connect {ip}:{port}";
        Console.WriteLine($"Copying to clipboard: {connectCommand}");
        SetClipboardText(connectCommand);
        Console.WriteLine("Sending keys: (F1)");
        SendKeys.SendWait("{F1}");
        Thread.Sleep(100);
        Console.WriteLine("Sending keys: (Ctrl+V)");
        SendKeys.SendWait("^(v)");
        Thread.Sleep(50);
        Console.WriteLine("Sending keys: (Enter)");
        SendKeys.SendWait("{ENTER}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error in SendInput: {ex.Message}");
            Console.ResetColor();
        }
    }
    private static void FocusRustClient(string ip, int port)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Attempting to find Rust client window...");

            IntPtr rustClientWindow = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                Process process = Process.GetProcessById((int)pid);
                if (process.ProcessName.Equals("RustClient", StringComparison.OrdinalIgnoreCase)
                    && GetWindowText(hWnd).Contains("Rust"))
                {
                    rustClientWindow = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (rustClientWindow == IntPtr.Zero)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Rust client window not found.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Rust client window found: Handle {rustClientWindow}");

            for (int attempt = 1; attempt <= 10; attempt++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Attempt {attempt} to focus Rust client...");
                SetWindowPos(rustClientWindow, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                bool success = SetForegroundWindow(rustClientWindow);
                SetWindowPos(rustClientWindow, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                if (success)
                {
                    IntPtr foregroundWindow = GetForegroundWindow();
                    if (foregroundWindow == rustClientWindow)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Rust client successfully focused.");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Sending input...");
                        SendInput(ip, port);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\nComplete :) Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Rust client focus attempt failed. Retrying...");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("SetForegroundWindow failed. Retrying...");
                }

                Thread.Sleep(50);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to focus Rust client after multiple attempts.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error in FocusRustClient: {ex.Message}");
            Console.ResetColor();
        }
    }
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    private static string GetWindowText(IntPtr hWnd)
    {
        const int nChars = 256;
        System.Text.StringBuilder buff = new System.Text.StringBuilder(nChars);
        if (GetWindowText(hWnd, buff, nChars) > 0)
        {
            return buff.ToString();
        }
        return string.Empty;
    }
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;
}
