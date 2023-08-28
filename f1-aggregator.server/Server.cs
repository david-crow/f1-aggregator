using System.Net;
using System.Net.Sockets;
using System.Text;

namespace F1_Aggregator
{
    public static class Server
    {
        internal static void Main()
        {
            WebScraper.Initialize();

            TcpListener listener = new(IPAddress.Parse("127.0.0.1"), 42069);
            listener.Start();
            Console.WriteLine("Server started. Waiting for clients...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                Thread clientThread = new(HandleClient!);
                clientThread.Start(client);
            }
        }

        private static void HandleClient(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[2048];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + request);

                string response = request switch
                {
                    "View the next race weekend's schedule" => WebScraper.GetData("RaceSchedule"),
                    "View the remaining season schedule" => WebScraper.GetData("SeasonSchedule"),
                    "View the most recent race winner" => WebScraper.GetData("RaceResults"),
                    "View Driver Standings" => WebScraper.GetData("DriverStandings"),
                    "View Constructor Standings" => WebScraper.GetData("ConstructorStandings"),
                    _ => "Invalid request!"
                };

                buffer = Encoding.ASCII.GetBytes(response);
                stream.Write(buffer, 0, buffer.Length);
            }

            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}