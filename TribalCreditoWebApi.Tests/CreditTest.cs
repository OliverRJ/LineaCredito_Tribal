using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
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

        [Fact]
        public void IntentoSesionGet()
        {
            //Arrange
            using var apiContext = ApiTestContext.GetApiAppContext();
            //var mockContext = new Mock<HttpContext>();
            //var mockSession = new Moq.Mock<ISession>();

            //var sessionMock = new Mock<ISession>();
//            sessionMock.Setup(s => s.GetString("userdata")).Returns(Object); --failing
//sessionMock.Setup(s => s.SetString("userdata", object)); --failing

            var creditController = new CreditController(apiContext);
            byte[] dummy = System.Text.Encoding.UTF8.GetBytes("1");
            //mockSession.Setup(x => x.TryGetValue(It.IsAny<string>(), out dummy)).Returns(true); //the out dummy does the trick
            //mockContext.Setup(s => s.Session).Returns(mockSession.Object);
            var result = creditController.GetIntentoSesion();


        }

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
