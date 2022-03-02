using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrivalCreditoWebApi.Context;
using TrivalCreditoWebApi.Models;
using TribalCreditoWebApi.Utils;
using TribalCreditoWebApi.Decorator;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TrivalCreditoWebApi.Controllers
{
 
    [Route("api/[controller]")]
    [ApiController]
    public class CreditController : ControllerBase
    {

        ApiAppContext apiContext;
        public CreditController(ApiAppContext context)
        {
            apiContext = context;
            apiContext.Database.EnsureCreated();
        }

        [HttpGet]
        [Route("ObtenerCredito")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public ActionResult<string> SolicitarCredito(int id)
        //{
        //    if (id > 0)
        //    {
        //        return ("value");

        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //    // 429 Too Many Requests
        //}

        [HttpPost]
        [Route("SolicitarCredito")]
        [RateLimitDecorator(StrategyType = StrategyTypeEnum.IpAddress)]
        public ActionResult<Response> SolicitarCredito([FromBody] Request value)
        {
            Response respuesta = new Response();
            Comun funciones = new Comun();
     
            bool respuestaSolicitud = funciones.CalcularRiesgo(value);
            if (respuestaSolicitud)
            {
                respuesta.Message = "Se aceptó y se autorizó la linea de credito de " + value.RequestCreditLine;
            }
            else
            {
                respuesta.Message = "La solicitud linea de credito de " + value.RequestCreditLine + " fue rechazada.";
            }

            return Ok(respuesta);

        }
        //[HttpPost]
        //[Route("SolicitarCredito")]
        //public async Task SolicitarCredito([FromBody] Request value)
        //{
        //    Comun funciones = new Comun();
        //    bool respuestaSolicitud = funciones.CalcularRiesgo(value);
        //    apiContext.Requests.Add(value);
        //    await apiContext.SaveChangesAsync();
        //}

        [HttpGet]
        public IEnumerable<Request> Get()
        {
            return apiContext.Requests;
        }

        [HttpPost]
        public async Task Post([FromBody] Request value)
        {
            apiContext.Requests.Add(value);
            await apiContext.SaveChangesAsync();
        }

    }
}
