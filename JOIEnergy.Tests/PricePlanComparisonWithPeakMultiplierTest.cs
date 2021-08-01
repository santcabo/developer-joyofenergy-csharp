using JOIEnergy.Controllers;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace JOIEnergy.Tests
{
    public class PricePlanComparisonWithPeakMultiplierTest
    {
        private readonly MeterReadingService _meterReadingService;
        private readonly PricePlanComparatorController _controller;
        private readonly Dictionary<string, Supplier> _smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
        private static readonly string SMART_METER_ID = "smart-meter-id";

        public PricePlanComparisonWithPeakMultiplierTest()
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            this._meterReadingService = new MeterReadingService(readings);
            var pricePlans = new List<PricePlan>()
            {
                new PricePlan(supplier: Supplier.DrEvilsDarkEnergy, unitRate: 10m, new Dictionary<DayOfWeek, decimal> { { DayOfWeek.Saturday, 0.05m }, { DayOfWeek.Sunday, 0.05m } } ),
                new PricePlan(supplier: Supplier.TheGreenEco, unitRate: 2m, new Dictionary<DayOfWeek, decimal> { { DayOfWeek.Saturday, 0.75m }, { DayOfWeek.Sunday, 0.75m } } ),
                new PricePlan(supplier: Supplier.PowerForEveryone, unitRate: 1m)
            };
            var pricePlanService = new PricePlanService(pricePlans, _meterReadingService);
            var accountService = new AccountService(_smartMeterToPricePlanAccounts);
            this._controller = new PricePlanComparatorController(pricePlanService, accountService);
        }

        [Fact]
        public void ShouldCalculateCostForMeterReadingsForEveryPricePlan()
        {
            var timeNow = DateTime.Now;
            var electricityReading = new ElectricityReading() { Time = timeNow.AddHours(-1), Reading = 15.0m };
            var otherReading = new ElectricityReading() { Time = timeNow, Reading = 5.0m };

            this._meterReadingService.StoreReadings(SMART_METER_ID, new List<ElectricityReading>() { electricityReading, otherReading });

            var result = this._controller.CalculatedCostForEachPricePlan(SMART_METER_ID).Value;

            var actualCosts = ((JObject)result).ToObject<Dictionary<string, decimal>>();

            Assert.Equal(3, actualCosts.Count);
            Assert.Equal(5m, actualCosts[Supplier.DrEvilsDarkEnergy.ToString()], 3);
            Assert.Equal(15m, actualCosts[Supplier.TheGreenEco.ToString()], 3);
            Assert.Equal(10m, actualCosts[Supplier.PowerForEveryone.ToString()], 3);
        }

        [Fact]
        public void ShouldRecommendCheapestPricePlansNoLimitForMeterUsage()
        {
            var timeNow = DateTime.Now;
            this._meterReadingService.StoreReadings(SMART_METER_ID, new List<ElectricityReading>()
            {
                new ElectricityReading() { Time = timeNow.AddMinutes(-30), Reading = 35m },
                new ElectricityReading() { Time = timeNow, Reading = 3m }
            });

            var result = this._controller.RecommendCheapestPricePlans(SMART_METER_ID, null).Value;

            var recommendations = ((IEnumerable<KeyValuePair<string, decimal>>)result).ToList();

            Assert.Equal(Supplier.DrEvilsDarkEnergy.ToString(), recommendations[0].Key);
            Assert.Equal(Supplier.PowerForEveryone.ToString(), recommendations[1].Key);
            Assert.Equal(Supplier.TheGreenEco.ToString(), recommendations[2].Key);
            Assert.Equal(19m, recommendations[0].Value, 3);
            Assert.Equal(38m, recommendations[1].Value, 3);
            Assert.Equal(57m, recommendations[2].Value, 3);
            Assert.Equal(3, recommendations.Count);
        }

        [Fact]
        public void ShouldRecommendLimitedCheapestPricePlansForMeterUsage()
        {
            var timeNow = DateTime.Now;
            this._meterReadingService.StoreReadings(SMART_METER_ID, new List<ElectricityReading>()
            {
                new ElectricityReading() { Time = timeNow.AddMinutes(-45), Reading = 5m },
                new ElectricityReading() { Time = timeNow, Reading = 20m }
            });

            object result = this._controller.RecommendCheapestPricePlans(SMART_METER_ID, 2).Value;
            var recommendations = ((IEnumerable<KeyValuePair<string, decimal>>)result).ToList();

            Assert.Equal(Supplier.DrEvilsDarkEnergy.ToString(), recommendations[0].Key);
            Assert.Equal(Supplier.PowerForEveryone.ToString(), recommendations[1].Key);
            Assert.Equal(8.333m, recommendations[0].Value, 3);
            Assert.Equal(16.667m, recommendations[1].Value, 3);
            Assert.Equal(2, recommendations.Count);
        }

    }
}
