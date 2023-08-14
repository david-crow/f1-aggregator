namespace Project_1
{
    class Program
    {
        internal static readonly Dictionary<String, Action?> Options = new()
        {
            // menu items paired with their under-the-hood functions
            ["View the next race weekend's schedule"] = WebScraper.GetRaceSchedule,
            ["View the remaining season schedule"] = WebScraper.GetSeasonSchedule,
            ["View the most recent race winner"] = WebScraper.GetRaceWinner,
            ["View Driver Standings"] = WebScraper.GetDriverStandings,
            ["View Constructor Standings"] = WebScraper.GetConstructorStandings,
            ["Clear the console window"] = Console.Clear,
            ["Quit"] = null
        };

        static void Main()
        {
            UserInterface.ShowWelcome();
            string selection;
            do
            {
                UserInterface.ShowMenu();
                if ((selection = UserInterface.SelectMenuItem()) != Options.Keys.Last())
                {
                    UserInterface.EmphasizeText();
                    Options[selection]!();
                    UserInterface.DeEmphasizeText();
                }
            }
            while (selection != Options.Keys.Last());
            Console.WriteLine("Goodbye!");
        }
    }
}