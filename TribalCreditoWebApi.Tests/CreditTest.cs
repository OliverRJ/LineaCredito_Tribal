using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;
using System.Text;
using TribalCreditoWebApi.Utils;
using TrivalCreditoWebApi.Controllers;
using TrivalCreditoWebApi.Models;
using webapi.tests;
using Xunit;

namespace TrivalCreditoWebApi.Tests
{
    public class CreditTest
    {
        [Fact]
        public void CreditGet()
        {
            //Arrange
            using var apiContext = ApiTestContext.GetApiAppContext();
            var creditController = new CreditController(apiContext);
            //Act
            var result = creditController.Get();
            //Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Theory]
        [InlineData("Startup", 120, 1200, 239, true)]
        [InlineData("Startup", 120, 1200, 240, false)]
        [InlineData("SME", 100, 1000, 200, false)]
        [InlineData("SME", 100, 1000, 199, true)]
        public void ValidaSocitudCredito(string foundingType, decimal cashBalance, decimal montlyRevenue, int requestedCreditLine, bool expected)
        {
            //Arrange
            var comun = new Comun();
            Request miSolicitud = new Request { FoundingType = foundingType, CashBalance = cashBalance, MontlyRevenue = montlyRevenue, RequestCreditLine = requestedCreditLine };
            //Act
            var result = comun.CalcularRiesgo(miSolicitud);
            bool isValid = result;
            //Assert
            Assert.Equal(isValid, expected);
        }

        /* Intenté realizar pruebas con el controlador pero por la sesión no me permite ejecutar.
         * Se debe configurar Mock para realizar pruebas con variable de sesión, pero por problemas de compatibilidad con las librerías
         * no podía hacer las pruebas correctamente.
         */

        //[Theory]
        //[InlineData("Startup", 120, 1200, 150, false)]
        //public void ValidaSocitudCredito(string foundingType, decimal cashBalance, decimal montlyRevenue, int requestedCreditLine, bool expected)
        //{
        //    //Arrange
        //    using var apiContext = ApiTestContext.GetApiAppContext();
        //    var solicitudCredito = new CreditController(apiContext);
        //    Request miSolicitud = new Request { FoundingType = foundingType, CashBalance = cashBalance, MontlyRevenue = montlyRevenue, RequestCreditLine = requestedCreditLine };
        //    //Act
        //    var creditController = new CreditController(apiContext);
        //    var result = creditController.SolicitarCredito(miSolicitud);
        //    bool isValid = result.Value.Status == "Rechazado";
        //    //Assert
        //    Assert.IsType<OkResult>(result);
        //    //Assert.Equal(isValid, expected);
        //}
    }


}
