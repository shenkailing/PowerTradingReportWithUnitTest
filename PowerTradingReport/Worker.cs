using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axpo;
using CsvHelper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerTradingReport.Services;
using static Axpo.PowerService;

namespace PowerTradingReport
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static readonly string TimeZoneId = "Europe/London";
        private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        private static readonly string OutputDirectory = "Reports";
        private static readonly int ScheduleIntervalMinutes = 5;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        private readonly IPowerTradeService _powerTradeService;

        public Worker(ILogger<Worker> logger, IPowerTradeService powerTradeService)
        {
            _logger = logger;
            _powerTradeService = powerTradeService;
        }

        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Fetching power trades...");
                    var trades = await GetTradeDataAsync();

                    if (trades == null || trades.Count == 0)
                    {
                        _logger.LogWarning("No trade data received for today.");
                    }
                    else
                    {
                        _logger.LogInformation("Generating CSV report...");
                        GenerateCsvReport(trades);
                        _logger.LogInformation("Report generation completed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error while processing trade data.");
                }

                await Task.Delay(TimeSpan.FromMinutes(ScheduleIntervalMinutes), stoppingToken);
            }
        }

        private async Task<List<PowerTrade>> GetTradeDataAsync()
        {
            DateTime today = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZone).Date;
            var tradeService = new PowerService();

            try
            {
                IEnumerable<PowerTrade> trades = await tradeService.GetTradesAsync(today);
                _logger.LogInformation("Successfully retrieved {count} trades from PowerService.", trades?.Count() ?? 0);
                return trades?.ToList() ?? new List<PowerTrade>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve trades from PowerService.");
                return new List<PowerTrade>();
            }
        }

        private void GenerateCsvReport(List<PowerTrade> trades)
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZone);
            string fileName = $"PowerPosition_{now:yyyyMMdd_HHmm}.csv";
            string filePath = Path.Combine(OutputDirectory, fileName);

            Directory.CreateDirectory(OutputDirectory);

            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteField("Local Time");
                    csv.WriteField("Volume");
                    csv.NextRecord();

                    var aggregatedData = trades
                        .SelectMany(t => t.Periods)
                        .GroupBy(p => p.Period)
                        .Select(g => new { Hour = (g.Key - 1 + 23) % 24, Volume = g.Sum(p => p.Volume) })
                        .ToDictionary(p => p.Hour, p => p.Volume);

                    for (int hour = 23; hour < 47; hour++)
                    {
                        int adjustedHour = hour % 24;
                        int volume = (int)(aggregatedData.ContainsKey(adjustedHour) ? aggregatedData[adjustedHour] : 0);

                        csv.WriteField($"{adjustedHour:D2}:00");
                        csv.WriteField(volume);
                        csv.NextRecord();
                    }
                }

                _logger.LogInformation("CSV Report Generated: {filePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write CSV report.");
            }
        }
    }
}
