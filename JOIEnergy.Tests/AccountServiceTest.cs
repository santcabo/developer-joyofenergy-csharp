using System;
using System.Collections.Generic;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using Xunit;

namespace JOIEnergy.Tests
{
    public class AccountServiceTest
    {
        private const Supplier PRICE_PLAN_ID = Supplier.PowerForEveryone;
        private const string SMART_METER_ID = "smart-meter-id";

        private AccountService _accountService;

        public AccountServiceTest()
        {
            var smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>
            {
                { SMART_METER_ID, PRICE_PLAN_ID }
            };

            this._accountService = new AccountService(smartMeterToPricePlanAccounts);
        }

        [Fact]
        public void GivenTheSmartMeterIdReturnsThePricePlanId()
        {
            var result = this._accountService.GetPricePlanIdForSmartMeterId("smart-meter-id");
            Assert.Equal(Supplier.PowerForEveryone, result);
        }

        [Fact]
        public void GivenAnUnknownSmartMeterIdReturnsANullSupplier()
        {
            var result = this._accountService.GetPricePlanIdForSmartMeterId("bob-carolgees");
            Assert.Equal(Supplier.NullSupplier, result);
        }
    }
}
