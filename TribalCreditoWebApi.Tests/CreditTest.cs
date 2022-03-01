using System;
using System.Collections.Generic;
using System.Text;
using TrivalCreditoWebApi.Controllers;
using webapi.tests;
using Xunit;

namespace TrivalCreditoWebApi.Tests
{
    public class CreditTest
    {
        public void CreditGet()
        {
            using var apiContext = ApiTestContext.GetApiAppContext();
            var creditController = new CreditController(apiContext);

            var result = creditController.Get();

            Assert.NotEmpty(result);
        }
    }
}
