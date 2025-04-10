using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axpo;

namespace PowerTradingReport.Services
{
    public class PowerTradeService : IPowerTradeService
    {
        public async Task<List<PowerTrade>> GetTradesAsync(DateTime date)
        {
            var service = new PowerService();
            var trades = await service.GetTradesAsync(date);
            return trades?.ToList() ?? new List<PowerTrade>();
        }
    }
}
