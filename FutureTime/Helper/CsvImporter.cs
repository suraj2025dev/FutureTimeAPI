using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using MongoDB.Driver;
using System.Globalization;

namespace FutureTime.Helper
{
    public class CsvImporter
    {
        public async Task ImportAsync(IFormFile csvFile)
        { 
            if(csvFile == null || csvFile.Length == 0)
                throw new InvalidOperationException("CSV file is empty.");

            var _collection = MongoDBService.ConnectCollection<DailyHoroscopeUpdatesModel>(MongoDBService.COLLECTION_NAME.DailyHoroscopeUpdatesModel);

            var parsedRows = new List<(DateTime Date, int RashiId, decimal Rating, string Description)>();

            // 1. Read CSV Stream
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);

            string? line;
            bool isHeader = true;
            int lineNumber = 0;

            while((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;

                if(isHeader)
                {
                    isHeader = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if(string.IsNullOrWhiteSpace(line))
                    continue;

                var columns = line.Split(',');
                if (columns.Length != 4)
                    throw new InvalidOperationException($"Invalid CSV format at line {lineNumber}.");

                if (!DateTime.TryParseExact(columns[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    throw new InvalidOperationException($"Invalid date format at line {lineNumber}. Expected format: yyyy-MM-dd.");
                }

                if (!int.TryParse(columns[1], out var rashiId))
                {
                    throw new InvalidOperationException($"Invalid Rashi Id at line {lineNumber}.");
                }

                if (!decimal.TryParse(columns[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var rating))
                {
                    throw new InvalidOperationException($"Invalid rating format at line {lineNumber}.");
                }

                parsedRows.Add((date, rashiId, rating, columns[3]));
            }

            // 2. Group by date 
            var groupedByDate = parsedRows.GroupBy(row => row.Date).ToList();

            foreach(var day in groupedByDate)
            {
                // 3. Validate per day
                if(day.Count() != 12)
                {
                    throw new InvalidOperationException($"Expected 12 entries for date {day.Key:yyyy-MM-dd}, but found {day.Count()}.");
                }

                if(day.Select(x => x.RashiId).Distinct().Count() != 12)
                {
                    throw new InvalidOperationException($"Duplicate Rashi Ids found for date {day.Key:yyyy-MM-dd}.");
                }

                // 4. Skip if already exists in the database (this part is not implemented here, but you can add your database check logic)
                var exists = await _collection
                    .Find(x => x.transaction_date == day.Key.ToString("yyyy-MM-dd"))
                    .AnyAsync();

                if (exists)
                {
                    Console.WriteLine($"Daily update for date {day.Key:yyyy-MM-dd} already exists. Skipping.");
                    continue;
                }


                var dailyUpdate = new DailyHoroscopeUpdatesModel
                {
                    transaction_date = day.Key.ToString("yyyy-MM-dd"),
                    items = day
                    .OrderBy(x => x.RashiId)
                    .Select(row => new DailyHoroscopeUpdatesDetail
                    {
                        rashi_id = row.RashiId,
                        rating = row.Rating,
                        description = row.Description
                    }).ToList()
                };

                await _collection.InsertOneAsync(dailyUpdate);
            }
        }
    }
}
