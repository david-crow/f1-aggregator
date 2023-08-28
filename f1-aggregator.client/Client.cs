using System.Net.Sockets;

namespace F1_Aggregator
{
    internal class Client
    {
        internal static void Main()
        {
            UserInterface.ShowWelcome();
            Console.Write("Connecting to server... ");
            using TcpClient client = new();
            DateTime start = DateTime.Now;
            while (!client.Connected && DateTime.Now - start < TimeSpan.FromSeconds(10))
            {
                try
                {
                    client.Connect("127.0.0.1", 42069);
                }
                catch (SocketException)
                {
                    Thread.Sleep(100);
                }
            }

            if (!client.Connected)
            {
                Console.WriteLine("failed to connect.\nGoodbye!");
                return;
            }

            Console.WriteLine("connected!\n");
            var stream = client.GetStream();
            bool runProgram = true;
            while (runProgram)
            {
                UserInterface.ShowMenu();
                runProgram = UserInterface.SelectMenuItem(stream);
            }
            Console.WriteLine("Goodbye!");
        }
    }
}