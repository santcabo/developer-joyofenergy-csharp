using System;
using System.Collections.Generic;
using JOIEnergy.Enums;

namespace JOIEnergy.Services
{
    public class AccountService : Dictionary<string, Supplier>, IAccountService
    { 
        private readonly Dictionary<string, Supplier> _smartMeterToPricePlanAccounts;

        public AccountService(Dictionary<string, Supplier> smartMeterToPricePlanAccounts) 
        {
            this._smartMeterToPricePlanAccounts = smartMeterToPricePlanAccounts;
        }

        public Supplier GetPricePlanIdForSmartMeterId(string smartMeterId) => (!this._smartMeterToPricePlanAccounts.ContainsKey(smartMeterId))
                                                                                ? Supplier.NullSupplier
                                                                                : this._smartMeterToPricePlanAccounts[smartMeterId];       
    }
}
