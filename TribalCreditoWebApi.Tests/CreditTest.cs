using System;
using System.Collections.Generic;
using System.Text;
using TrivalCreditoWebApi.Controllers;
using TrivalCreditoWebApi.Models;
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

        [Theory]
        [InlineData("Startup", 120,1200,150,false)]
        public void ValidaSocitudCredito(string foundingType, decimal cashBalance, decimal montlyRevenue, int requestedCreditLine,bool expected)
        {
            //Arrange
            using var apiContext = ApiTestContext.GetApiAppContext();
            var solicitudCredito = new CreditController(apiContext);
            //Act

            //Assert
            var creditController = new CreditController(apiContext);
            Request miSolicitud = new Request { FoundingType = foundingType, CashBalance = cashBalance, MontlyRevenue = montlyRevenue, RequestCreditLine = requestedCreditLine };
            var result = creditController.SolicitarCredito(miSolicitud);
            bool isValid = result.Value.Status == "Rechazado";
            Assert.Equal(isValid,expected);
        }
    }


}
