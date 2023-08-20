namespace F1_Aggregator
{
    internal static class UserInterface
    {
        private static readonly TimeZoneInfo TIME_ZONE = TimeZoneInfo.Local;
        private static readonly ConsoleColor TEXT_COLOR = ConsoleColor.White;
        private static readonly ConsoleColor EMPHASIS_COLOR = ConsoleColor.Red;

        // menu items and their associated WebScraper methods
        private static readonly Dictionary<string, Action<WebScraper>?> MenuItems = new()
        {
            ["View the next race weekend's schedule"] = (scraper) => scraper.PrintRaceSchedule(),
            ["View the remaining season schedule"] = (scraper) => scraper.PrintSeasonSchedule(),
            ["View the most recent race winner"] = (scraper) => scraper.PrintRaceResults(),
            ["View Driver Standings"] = (scraper) => scraper.PrintDriverStandings(),
            ["View Constructor Standings"] = (scraper) => scraper.PrintConstructorStandings(),
            ["Clear the console window"] = (scraper) => Console.Clear(),
            ["Quit"] = (scraper) => { }
        };
        private static readonly string[] MenuOptions = MenuItems.Keys.ToArray();
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
                $"{(TIME_ZONE.IsDaylightSavingTime(DateTime.Now) ? TIME_ZONE.DaylightName : TIME_ZONE.StandardName)}.");
        }

        // print the menu
        internal static void ShowMenu()
        {
            Console.WriteLine("\nMenu");
            Console.WriteLine(new string('-', MenuWidth));
            for (int i = 0; i < MenuOptions.Length; i++)
                Console.WriteLine($"{i + 1}. {MenuOptions[i]}");
            Console.WriteLine();
        }

        // allow the user to select a menu item; execute the related function
        // return true/false to repeat/end the program
        internal static bool SelectMenuItem(WebScraper scraper)
        {
            int selection;
            do
            {
                Console.Write($"Select one of the menu options (1-{MenuItems.Count}): ");
                _ = int.TryParse(Console.ReadLine(), out selection);
            }
            while (selection < 1 || MenuItems.Count + 1 <= selection);

            string option = MenuOptions[selection - 1];
            Console.WriteLine($"{option}\n");
            if (option == MenuOptions.Last())
                return false;

            EmphasizeText();
            MenuItems[option]!(scraper);
            DeEmphasizeText();
            return true;
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