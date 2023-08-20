using HtmlAgilityPack;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace F1_Aggregator
{
    internal partial class WebScraper
    {
        // paths for saving/scraping data
        private static readonly string DATA_FILE = "data/Data.json";
        private static readonly string SCHEDULE_URL = "https://f1calendar.com";
        private static readonly string RESULTS_URL = "https://www.formula1.com/en/results.html/2023/races.html";
        private static readonly string DRIVER_STANDINGS_URL = "https://www.formula1.com/en/results.html/2023/drivers.html";
        private static readonly string CONSTRUCTOR_STANDINGS_URL = "https://www.formula1.com/en/results.html/2023/team.html";

        // mapping between output file and scraping function
        private static readonly Dictionary<string, Func<MultiData.Data>> Scrapers = new()
        {
            ["RaceSchedule"] = ScrapeRaceSchedule,
            ["SeasonSchedule"] = ScrapeSeasonSchedule,
            ["RaceResults"] = ScrapeRaceResults,
            ["DriverStandings"] = ScrapeDriverStandings,
            ["ConstructorStandings"] = ScrapeConstructorStandings
        };

        #region DataFetching

        private static MultiData? F1Data;

        internal WebScraper()
        {
            F1Data = FetchData();
            if (F1Data == null || F1Data.Expiration < DateTime.Now)
            {
                F1Data = new();
                foreach (var (key, scraper) in Scrapers)
                    F1Data.DataCollection.Add(key, scraper());
                File.WriteAllText(DATA_FILE, JsonSerializer.Serialize(F1Data, new JsonSerializerOptions { WriteIndented = true }));
            }
        }


        // get saved data from a file
        private static MultiData? FetchData()
        {
            if (File.Exists(DATA_FILE))
                return JsonSerializer.Deserialize<MultiData>(File.ReadAllText(DATA_FILE))!;
            return null;
        }

        // get the five-event schedule for a single race weekend
        private static MultiData.Data ScrapeRaceSchedule()
        {
            Console.WriteLine("Downloading data...");
            MultiData.Data data = new() { Output = "Grand Prix" };
            int eventNameIndex = 0;
            int maxEventLength = "Event".Length;
            var raceDetails = LoadEvents(SCHEDULE_URL)[0].SelectNodes(".//tr").ToArray()[1..];

            foreach (var row in raceDetails)
            {
                var raceData = CleanTableRow(row.SelectNodes(".//td"));
                var (event_, date, time) = (raceData[1], DateTime.Parse(raceData[2]).ToString("dd MMM yy"), ConvertTime(raceData[3]));

                if (data.Output == "Grand Prix")
                {
                    eventNameIndex = event_.IndexOf(data.Output) + data.Output.Length + 1;
                    data.Output = event_[..eventNameIndex];
                }

                event_ = event_[eventNameIndex..];
                maxEventLength = Math.Max(maxEventLength, event_.Length);
                data.Info.Add(new() { date, time, event_ });
            }

            // save the scraped data for later
            F1Data!.Expiration = DateTime.Parse(data.Info[^1][0]);
            data.Labels = new() { "Date", "Time", "Event" };
            data.Widths = new() { "dd MMM yy".Length, "hh:mm tt".Length, maxEventLength };
            data.Output += $"\n\n{BuildTable(data)}";
            return data;
        }

        // get the date for each of the remaining Grands Prix
        private static MultiData.Data ScrapeSeasonSchedule()
        {
            MultiData.Data data = new();
            int maxEventLength = "Event".Length;
            var allRaceDetails = LoadEvents(SCHEDULE_URL);

            foreach (var row in allRaceDetails)
            {
                var allRaces = row.SelectNodes(".//tr").ToArray()[1..];
                for (int i = 4; i < allRaces.Length; i += 5) // jump to the fifth event (the GP) of every weekend
                {
                    var raceData = CleanTableRow(allRaces[i].SelectNodes(".//td"));
                    var (event_, date) = (raceData[1][..^(" Grand Prix".Length)], DateTime.Parse(raceData[2]).ToString("dd MMM yy"));
                    maxEventLength = Math.Max(maxEventLength, event_.Length);
                    data.Info.Add(new() { date, event_ });
                }
            }

            // save the scraped data for later
            data.Labels = new() { "Date", "Event" };
            data.Widths = new() { "dd MMM yy".Length, maxEventLength };
            data.Output = BuildTable(data);
            return data;
        }

        // get the winning driver/constructor for the most recent Grand Prix
        private static MultiData.Data ScrapeRaceResults()
        {
            var raceDetails = CleanTableRow(LoadResults(RESULTS_URL).SelectNodes(".//tr")[^1].SelectNodes(".//td"));
            MultiData.Data data = new()
            {
                Info = new() {
                    new() { raceDetails[1] },
                    new() { DateTime.Parse(raceDetails[2]).ToString("dd MMM yy") },
                    new() { CleanDriverName(raceDetails[3]) },
                    new() { raceDetails[4] }
                }
            };

            // save the scraped data for later
            var (location, date, driver, constructor) = (data.Info[0][0], data.Info[1][0], data.Info[2][0], data.Info[3][0]);
            data.Output = $"{driver}, of {constructor}, won the {date} Grand Prix in {location}.";
            return data;
        }

        // get driver point totals so far
        internal static MultiData.Data ScrapeDriverStandings()
        {
            MultiData.Data data = new();
            int maxDriverLength = "Driver".Length, maxConstructorLength = "Constructor".Length;
            var driverStandingsRows = LoadResults(DRIVER_STANDINGS_URL).SelectNodes(".//tr").ToArray()[1..];

            foreach (var row in driverStandingsRows)
            {
                var driverData = CleanTableRow(row.SelectNodes(".//td"));
                var (position, driver, constructor, points) = (driverData[1], CleanDriverName(driverData[2]), driverData[4], driverData[5]);
                maxDriverLength = Math.Max(maxDriverLength, driver.Length);
                maxConstructorLength = Math.Max(maxConstructorLength, constructor.Length);
                data.Info.Add(new() { position, driver, constructor, points });
            }

            // save the scraped data for later
            data.Labels = new() { "Position", "Driver", "Constructor", "Points" };
            data.Widths = new() { "Position".Length, maxDriverLength, maxConstructorLength, "Points".Length };
            data.Output = BuildTable(data);
            return data;
        }

        // get constructor point totals so far
        private static MultiData.Data ScrapeConstructorStandings()
        {
            MultiData.Data data = new();
            int maxConstructorLength = "Constructor".Length;
            var constructorStandings = LoadResults(CONSTRUCTOR_STANDINGS_URL).SelectNodes(".//tr").ToArray()[1..];

            foreach (var row in constructorStandings)
            {
                var constructorData = CleanTableRow(row.SelectNodes(".//td"));
                var (position, constructor, points) = (constructorData[1], constructorData[2], constructorData[3]);
                maxConstructorLength = Math.Max(maxConstructorLength, constructor.Length);
                data.Info.Add(new() { position, constructor, points });
            }

            // save the scraped data for later
            data.Labels = new() { "Position", "Constructor", "Points" };
            data.Widths = new() { "Position".Length, maxConstructorLength, "Points".Length };
            data.Output = BuildTable(data);
            return data;
        }

        // specific to f1calendar.com's format
        private static HtmlNodeCollection LoadEvents(string url)
        {
            var doc = new HtmlWeb().Load(SCHEDULE_URL).DocumentNode.SelectSingleNode("//table[@id='events-table']");
            return doc.SelectNodes(".//tbody[not(contains(@class, 'hidden'))]");
        }

        // specific to formula1.com's format
        private static HtmlNode LoadResults(string url)
        {
            return new HtmlWeb().Load(url).DocumentNode.SelectSingleNode("//table[contains(@class, 'resultsarchive-table')]");
        }

        #endregion DataFetching
        #region DataPresenting

        internal void PrintRaceSchedule()
        {
            PrintData("RaceSchedule");
        }

        internal void PrintSeasonSchedule()
        {
            PrintData("SeasonSchedule");
        }

        internal void PrintRaceResults()
        {
            PrintData("RaceResults");
        }

        internal void PrintDriverStandings()
        {
            PrintData("DriverStandings");
        }

        internal void PrintConstructorStandings()
        {
            PrintData("ConstructorStandings");
        }

        private void PrintData(string key)
        {
            Console.WriteLine($"{F1Data!.DataCollection[key].Output}");
        }

        private static string BuildTable(MultiData.Data data)
        {
            var labels = data.Labels!;
            var widths = data.Widths!;

            StringBuilder sb = new();
            AddTableRow(sb, labels, widths);
            AddTableRow(sb, Enumerable.Repeat("-", labels.Count).ToList(), widths, delimiter: '-');
            foreach (var d in data.Info)
                AddTableRow(sb, d, widths);
            return sb.ToString().TrimEnd();
        }

        private static void AddTableRow(StringBuilder sb, List<string> labels, List<int> widths, char delimiter = ' ')
        {
            for (int i = 0; i < labels.Count; i++)
                sb.Append($"{labels[i].PadRight(widths[i], delimiter)}  ");
            sb.AppendLine();
        }

        #endregion DataPresenting

        #region DataCleaning

        // remove all of the extra whitespace within a collection of nodes
        private static List<string> CleanTableRow(HtmlNodeCollection row)
        {
            List<string> cleanedRow = new();
            foreach (var r in row)
                cleanedRow.Add(r.InnerText.Trim());
            return cleanedRow;
        }

        // convert from f1calendar.com's time (UK time) to the user's local time
        private static string ConvertTime(string time)
        {
            var givenTime = DateTime.ParseExact(time, "HH:mm", null);
            var ukTime = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTime(givenTime, ukTime, TimeZoneInfo.Local).ToString("hh:mm tt");
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

        #endregion DataCleaning
    }
}