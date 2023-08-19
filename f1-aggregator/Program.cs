namespace F1_Aggregator
{
    class Program
    {
        static void Main()
        {
            UserInterface.ShowWelcome();
            WebScraper scraper = new();

            bool runProgram = true;
            while (runProgram)
            {
                UserInterface.ShowMenu();
                runProgram = UserInterface.SelectMenuItem(scraper);
            }

            Console.WriteLine("Goodbye!");
        }
    }
}