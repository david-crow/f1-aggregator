namespace Project_1
{
    internal static class UserInterface
    {
        private static readonly ConsoleColor TextColor = ConsoleColor.White;
        private static readonly ConsoleColor EmphasisColor = ConsoleColor.Red;
        private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.Local;
        private static string[] MenuItems => Program.Options.Keys.ToArray();
        private static int MenuWidth => 3 + MenuItems.Max(str => str.Length);

        internal static void EmphasizeText()
        {
            Console.ForegroundColor = EmphasisColor;
        }

        internal static void DeEmphasizeText()
        {
            Console.ForegroundColor = TextColor;
        }

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
                $"{(TimeZone.IsDaylightSavingTime(DateTime.Now) ? TimeZone.DaylightName : TimeZone.StandardName)}.");
        }

        internal static void ShowMenu()
        {
            Console.WriteLine("\nMenu");
            Console.WriteLine(new String('-', MenuWidth));
            for (int i = 0; i < MenuItems.Length; i++)
                Console.WriteLine($"{i + 1}. {MenuItems[i]}");
            Console.WriteLine();
        }

        internal static string SelectMenuItem()
        {
            int selection;
            do
            {
                Console.Write($"Select one of the menu options (1-{MenuItems.Length}): ");
                _ = int.TryParse(Console.ReadLine(), out selection);
            }
            while (selection < 1 || MenuItems.Length + 1 <= selection);

            string menuItem = MenuItems[selection - 1];
            Console.WriteLine($"{menuItem}\n");
            return menuItem;
        }
    }
}