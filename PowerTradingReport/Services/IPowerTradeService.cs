using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Axpo;

namespace PowerTradingReport.Services
{
    public interface IPowerTradeService
    {
        Task<List<PowerTrade>> GetTradesAsync(DateTime date);
    }
}
