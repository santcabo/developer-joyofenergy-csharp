﻿using JOIEnergy.Controllers;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;

namespace JOIEnergy.Tests
{
    public class PricePlanComparisonTest
    {
        private readonly MeterReadingService _meterReadingService;
        private readonly PricePlanComparatorController _controller;
        private readonly Dictionary<string, Supplier> _smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
        private static readonly string SMART_METER_ID = "smart-meter-id";

        public PricePlanComparisonTest()
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            this._meterReadingService = new MeterReadingService(readings);
            var pricePlans = new List<PricePlan>() 
            { 
                new PricePlan(supplier: Supplier.DrEvilsDarkEnergy, unitRate: 10m), 
                new PricePlan(supplier: Supplier.TheGreenEco, unitRate: 2m),
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
            Assert.Equal(100m, actualCosts[Supplier.DrEvilsDarkEnergy.ToString()], 3);
            Assert.Equal(20m, actualCosts[Supplier.TheGreenEco.ToString()], 3);
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

            Assert.Equal(Supplier.PowerForEveryone.ToString(), recommendations[0].Key);
            Assert.Equal(Supplier.TheGreenEco.ToString(), recommendations[1].Key);
            Assert.Equal(Supplier.DrEvilsDarkEnergy.ToString(), recommendations[2].Key);
            Assert.Equal(38m, recommendations[0].Value, 3);
            Assert.Equal(76m, recommendations[1].Value, 3);
            Assert.Equal(380m, recommendations[2].Value, 3);
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

            Assert.Equal(Supplier.PowerForEveryone.ToString(), recommendations[0].Key);
            Assert.Equal(Supplier.TheGreenEco.ToString(), recommendations[1].Key);
            Assert.Equal(16.667m, recommendations[0].Value, 3);
            Assert.Equal(33.333m, recommendations[1].Value, 3);
            Assert.Equal(2, recommendations.Count);
        }

        [Fact]
        public void ShouldRecommendCheapestPricePlansMoreThanLimitAvailableForMeterUsage()
        {
            this._meterReadingService.StoreReadings(SMART_METER_ID, new List<ElectricityReading>() 
            {
                new ElectricityReading() { Time = DateTime.Now.AddMinutes(-30), Reading = 35m },
                new ElectricityReading() { Time = DateTime.Now, Reading = 3m }
            });

            object result = this._controller.RecommendCheapestPricePlans(SMART_METER_ID, 5).Value;
            var recommendations = ((IEnumerable<KeyValuePair<string, decimal>>)result).ToList();

            Assert.Equal(3, recommendations.Count);
        }

        [Fact]
        public void GivenNoMatchingMeterIdShouldReturnNotFound()
        {
            Assert.Equal(404, this._controller.CalculatedCostForEachPricePlan("not-found").StatusCode);
        }
    }
}
