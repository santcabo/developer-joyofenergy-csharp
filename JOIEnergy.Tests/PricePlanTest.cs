using System;
using System.Collections.Generic;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using Xunit;

namespace JOIEnergy.Tests
{
    public class PricePlanTest
    {
        private PricePlan _pricePlan;

        public PricePlanTest()
        {
            this._pricePlan = new PricePlan(supplier: Supplier.TheGreenEco, unitRate: 20m, new Dictionary<DayOfWeek, decimal>()
                                                                                                                                {
                                                                                                                                    { DayOfWeek.Saturday, 2m },
                                                                                                                                    { DayOfWeek.Sunday, 10m }
                                                                                                                                });
        }

        [Fact]
        public void TestGetEnergySupplier() 
        {
            Assert.Equal(Supplier.TheGreenEco, this._pricePlan.EnergySupplier);
        }

        [Fact]
        public void TestGetBasePrice() 
        {
            Assert.Equal(20m, this._pricePlan.GetPrice(new DateTime(2018, 1, 2)));
        }

        [Fact]
        public void TestGetPeakTimePrice()
        {
            Assert.Equal(40m, _pricePlan.GetPrice(new DateTime(2018, 1, 6)));
        }

    }
}
