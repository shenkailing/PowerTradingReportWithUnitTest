using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PowerTradingReport;
using PowerTradingReport.Services;
using Xunit;
using Axpo;

public class WorkerTests
{
    
    public interface IPowerPeriod
    {
        DateTime Start { get; set; }
        DateTime End { get; set; }
    }

    public class PowerPeriod
{
    public virtual int Period { get; set; }
    
    public virtual decimal Volume { get; set; }
}

    [Fact]
    public void TestPowerPeriodWithCallBase()
    {
        var mockPowerPeriod = new Mock<PowerPeriod> { CallBase = true };

        mockPowerPeriod.Setup(pp => pp.Period).Returns(10);
        mockPowerPeriod.Setup(pp => pp.Volume).Returns(500);

        var period = mockPowerPeriod.Object.Period;
        var volume = mockPowerPeriod.Object.Volume;

        Assert.Equal(10, period);
        Assert.Equal(500, volume);
    }
}
