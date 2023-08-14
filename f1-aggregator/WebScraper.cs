using HtmlAgilityPack;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace Project_1
{
    internal partial class WebScraper
    {
        // for saving
        private static readonly string RaceCalendarFile = "data/RaceCalendar.json";
        private static readonly string SeasonCalendarFile = "data/SeasonCalendar.json";
        private static readonly string ResultsFile = "data/Results.json";
        private static readonly string DriverStandingsFile = "data/DriverStandings.json";
        private static readonly string ConstructorStandingsFile = "data/ConstructorStandings.json";

        // for scraping
        private static readonly string CalendarUrl = "https://f1calendar.com";
        private static readonly string ResultsUrl = "https://www.formula1.com/en/results.html/2023/races.html";
        private static readonly string DriverStandingsUrl = "https://www.formula1.com/en/results.html/2023/drivers.html";
        private static readonly string ConstructorStandingsUrl = "https://www.formula1.com/en/results.html/2023/team.html";

        // get the five-event schedule for a single race weekend
        internal static void GetRaceSchedule()
        {
            // if we already have current data saved in a file, get it
            JsonData? data = FetchData(RaceCalendarFile);

            // otherwise, scrape the website
            if (data == null)
            {
                Console.WriteLine("Downloading data...\n");
                int eventNameIndex = 0;
                int maxEventLength = "Event".Length;
                var raceDetails = LoadEvents(CalendarUrl)[0].SelectNodes(".//tr").ToArray()[1..];
                data = new() { Name = "Grand Prix" };

                foreach (var row in raceDetails)
                {
                    var raceData = CleanTableRow(row.SelectNodes(".//td"));
                    var (event_, date, time) = (raceData[1], DateTime.Parse(raceData[2]).ToString("dd MMM yy"), ConvertTime(raceData[3]));

                    if (data.Name == "Grand Prix")
                    {
                        eventNameIndex = event_.IndexOf(data.Name) + data.Name.Length + 1;
                        data.Name = event_[..eventNameIndex];
                    }

                    event_ = event_[eventNameIndex..];
                    maxEventLength = Math.Max(maxEventLength, event_.Length);
                    data.Data.Add(new() { date, time, event_ });
                }

                data.Expiration = data.Data[^1][0];
                data.Labels = new() { "Date", "Time", "Event" };
                data.Widths = new() { "dd MMM yy".Length, "hh:mm tt".Length, maxEventLength };

                // save the scraped data for later
                File.WriteAllText(RaceCalendarFile, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }

            // finally, we can print the data
            Console.WriteLine($"{data.Name}\n");
            PrintTable(data);
        }

        // get the date for each of the remaining Grands Prix
        internal static void GetSeasonSchedule()
        {
            // if we already have current data saved in a file, get it
            JsonData? data = FetchData(SeasonCalendarFile);

            // otherwise, scrape the website
            if (data == null)
            {
                Console.WriteLine("Downloading data...\n");
                int maxEventLength = "Event".Length;
                var allRaceDetails = LoadEvents(CalendarUrl);
                data = new();

                foreach (var row in allRaceDetails)
                {
                    var allRaces = row.SelectNodes(".//tr").ToArray()[1..];
                    for (int i = 4; i < allRaces.Length; i += 5) // jump to the fifth event (the GP) of every weekend
                    {
                        var raceData = CleanTableRow(allRaces[i].SelectNodes(".//td"));
                        var (event_, date) = (raceData[1][..^(" Grand Prix".Length)], DateTime.Parse(raceData[2]).ToString("dd MMM yy"));
                        maxEventLength = Math.Max(maxEventLength, event_.Length);
                        data.Data.Add(new() { date, event_ });
                    }
                }

                data.Expiration = data.Data[0][0];
                data.Labels = new() { "Date", "Event" };
                data.Widths = new() { "dd MMM yy".Length, maxEventLength };

                // save the scraped data for later
                File.WriteAllText(SeasonCalendarFile, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }

            // finally, we can print the data
            PrintTable(data);
        }

        // get the winning driver/constructor for the most recent Grand Prix
        internal static void GetRaceWinner()
        {
            // if we already have current data saved in a file, get it
            JsonData? data = FetchData(ResultsFile);

            // otherwise, scrape the website
            if (data == null)
            {
                Console.WriteLine("Downloading data...\n");
                var raceDetails = CleanTableRow(LoadResults(ResultsUrl).SelectNodes(".//tr")[^1].SelectNodes(".//td"));
                data = new()
                {
                    Expiration = DateTime.Now.AddDays(7).ToString("dd MMM yy"),
                    Data = new() {
                        new() { raceDetails[1] },
                        new() { DateTime.Parse(raceDetails[2]).ToString("dd MMM yy") },
                        new() { CleanDriverName(raceDetails[3]) },
                        new() { raceDetails[4] }
                    }
                };

                // save the scraped data for later
                File.WriteAllText(ResultsFile, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }

            var (location, date, driver, constructor) = (data.Data[0][0], data.Data[1][0], data.Data[2][0], data.Data[3][0]);
            Console.WriteLine($"{driver}, of {constructor}, won the {date} Grand Prix in {location}.");
        }

        // get driver point totals so far
        internal static void GetDriverStandings()
        {
            // if we already have current data saved in a file, get it
            JsonData? data = FetchData(DriverStandingsFile);

            // otherwise, scrape the website
            if (data == null)
            {
                Console.WriteLine("Downloading data...\n");
                int maxDriverLength = "Driver".Length, maxConstructorLength = "Constructor".Length;
                var driverStandingsRows = LoadResults(DriverStandingsUrl).SelectNodes(".//tr").ToArray()[1..];
                data = new();

                foreach (var row in driverStandingsRows)
                {
                    var driverData = CleanTableRow(row.SelectNodes(".//td"));
                    var (position, driver, constructor, points) = (driverData[1], CleanDriverName(driverData[2]), driverData[4], driverData[5]);
                    maxDriverLength = Math.Max(maxDriverLength, driver.Length);
                    maxConstructorLength = Math.Max(maxConstructorLength, constructor.Length);
                    data.Data.Add(new() { position, driver, constructor, points });
                }

                data.Expiration = DateTime.Now.AddDays(7).ToString("dd MMM yy");
                data.Labels = new() { "Position", "Driver", "Constructor", "Points" };
                data.Widths = new() { "Position".Length, maxDriverLength, maxConstructorLength, "Points".Length };

                // save the scraped data for later
                File.WriteAllText(DriverStandingsFile, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }

            // finally, we can print the data
            PrintTable(data);

        }

        // get constructor point totals so far
        internal static void GetConstructorStandings()
        {
            // if we already have current data saved in a file, get it
            JsonData? data = FetchData(ConstructorStandingsFile);

            // otherwise, scrape the website
            if (data == null)
            {
                Console.WriteLine("Downloading data...\n");
                int maxConstructorLength = "Constructor".Length;
                var constructorStandings = LoadResults(ConstructorStandingsUrl).SelectNodes(".//tr").ToArray()[1..];
                data = new();

                foreach (var row in constructorStandings)
                {
                    var constructorData = CleanTableRow(row.SelectNodes(".//td"));
                    var (position, constructor, points) = (constructorData[1], constructorData[2], constructorData[3]);
                    maxConstructorLength = Math.Max(maxConstructorLength, constructor.Length);
                    data.Data.Add(new() { position, constructor, points });
                }

                data.Expiration = DateTime.Now.AddDays(7).ToString("dd MMM yy");
                data.Labels = new() { "Position", "Constructor", "Points" };
                data.Widths = new() { "Position".Length, maxConstructorLength, "Points".Length };

                // save the scraped data for later
                File.WriteAllText(ConstructorStandingsFile, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }

            // finally, we can print the data
            PrintTable(data);
        }

        // specific to f1calendar.com's format
        private static HtmlNodeCollection LoadEvents(string url)
        {
            var doc = new HtmlWeb().Load(CalendarUrl).DocumentNode.SelectSingleNode("//table[@id='events-table']");
            return doc.SelectNodes(".//tbody[not(contains(@class, 'hidden'))]");
        }

        // specific to formula1.com's format
        private static HtmlNode LoadResults(string url)
        {
            return new HtmlWeb().Load(url).DocumentNode.SelectSingleNode("//table[contains(@class, 'resultsarchive-table')]");
        }

        // get saved JSON data from a file
        private static JsonData? FetchData(string file)
        {
            JsonData data;

            // if we already have current data saved in a file, get it
            if (File.Exists(file))
            {
                data = JsonSerializer.Deserialize<JsonData>(File.ReadAllText(file))!;
                if (DateTime.Now <= DateTime.Parse(data.Expiration))
                    return data;
            }

            return null;
        }

        // remove all of the extra whitespace within a collection of nodes
        private static List<string> CleanTableRow(HtmlNodeCollection row)
        {
            List<string> cleanedRow = new();
            foreach (var r in row)
                cleanedRow.Add(r.InnerText.Trim());
            return cleanedRow;
        }

        // pretty-print a table with a header
        private static void PrintTable(JsonData j)
        {
            PrintTableRow(j.Labels, j.Widths);
            PrintTableRow(Enumerable.Repeat("-", j.Labels.Count).ToList(), j.Widths, delimiter: '-');
            foreach (var d in j.Data)
                PrintTableRow(d, j.Widths);
        }

        // pretty-print a table row
        private static void PrintTableRow(List<string> labels, List<int> widths, char delimiter = ' ')
        {
            for (int i = 0; i < labels.Count; i++)
                Console.Write($"{labels[i].PadRight(widths[i], delimiter)}  ");
            Console.WriteLine();
        }

        // convert from f1calendar.com's time (UK time) to the user's local time
        private static string ConvertTime(string time)
        {
            var givenTime = DateTime.ParseExact(time, "HH:mm", null);
            var ukTime = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTime(givenTime, ukTime, TimeZoneInfo.Local).ToString("h:mm tt");
        }

        // convert from FirstLastLAS to First Last
        private static string CleanDriverName(string driver)
        {
            var cleanedDriver = CleanDriverHtml().Replace(driver, "");
            return string.Join(" ", CleanDriverWhitespace().Split(cleanedDriver)[0..^3]);
        }

        // regexes for CleanDriverName()
        [GeneratedRegex("[^a-zA-Z]")]
        private static partial Regex CleanDriverHtml();
        [GeneratedRegex("(?<!^)(?=[A-Z])")]
        private static partial Regex CleanDriverWhitespace();
    }

    public class JsonData
    {
        public string Expiration { get; set; }
        public string Name { get; set; }
        public List<string> Labels { get; set; }
        public List<int> Widths { get; set; }
        public List<List<string>> Data { get; set; }

        public JsonData()
        {
            Expiration = string.Empty;
            Name = string.Empty;
            Labels = new();
            Widths = new();
            Data = new();
        }
    }
}