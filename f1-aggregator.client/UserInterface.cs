using System.Net.Sockets;
using System.Text;

namespace F1_Aggregator
{
    internal static class UserInterface
    {
        private static readonly TimeZoneInfo TIME_ZONE = TimeZoneInfo.Local;
        private static readonly ConsoleColor TEXT_COLOR = ConsoleColor.White;
        private static readonly ConsoleColor EMPHASIS_COLOR = ConsoleColor.Red;
        private static readonly string CLEAR = "Clear the console window";

        // all WebScraper options and UI options
        private static readonly List<string> MenuOptions = new()
        {
            "View the next race weekend's schedule",
            "View the remaining season schedule",
            "View the most recent race winner",
            "View Driver Standings",
            "View Constructor Standings",
            CLEAR,
            "Quit"
        };
        private static readonly int MenuWidth = 3 + MenuOptions.Max(str => str.Length);

        // greet the user
        internal static void ShowWelcome()
        {
            string message = "Good " + DateTime.Now.Hour switch
            {
                < 12 => "morning",
                < 18 => "afternoon",
                _ => "evening"
            };

            // the system timezone affects the race/season schedules, so the user can see here whether we actually have it right
            Console.WriteLine($"{message}! It is {DateTime.Now:h:mm tt}. Your time zone is " +
                $"{(TIME_ZONE.IsDaylightSavingTime(DateTime.Now) ? TIME_ZONE.DaylightName : TIME_ZONE.StandardName)}.\n");
        }

        // print the menu
        internal static void ShowMenu()
        {
            Console.WriteLine("Menu");
            Console.WriteLine(new string('-', MenuWidth));
            for (int i = 0; i < MenuOptions.Count; i++)
                Console.WriteLine($"{i + 1}. {MenuOptions[i]}");
            Console.WriteLine();
        }

        // allow the user to select a menu item; execute the related function
        // return true/false to repeat/end the program
        internal static bool SelectMenuItem(NetworkStream stream)
        {
            int selection;
            do
            {
                Console.Write($"Select one of the menu options (1-{MenuOptions.Count}): ");
                _ = int.TryParse(Console.ReadLine(), out selection);
            }
            while (selection < 1 || MenuOptions.Count + 1 <= selection);

            string option = MenuOptions[selection - 1];
            Console.WriteLine($"{option}\n");
            if (option == MenuOptions.Last())
                return false;
            else if (option == CLEAR)
                Console.Clear();
            else
            {
                EmphasizeText();
                Console.WriteLine(GetServerResponse(stream, option) + "\n");
                DeEmphasizeText();
            }
            return true;
        }

        // send the menu option to the server; process and return the server's response
        private static string GetServerResponse(NetworkStream stream, string option)
        {
            byte[] request = Encoding.ASCII.GetBytes(option);
            stream.Write(request, 0, request.Length);
            byte[] response = new byte[2048];
            int bytes = stream.Read(response, 0, response.Length);
            return Encoding.ASCII.GetString(response, 0, bytes);
        }

        // make the output a little more obvious
        private static void EmphasizeText()
        {
            Console.ForegroundColor = EMPHASIS_COLOR;
        }

        // back to normal
        private static void DeEmphasizeText()
        {
            Console.ForegroundColor = TEXT_COLOR;
        }
    }
}