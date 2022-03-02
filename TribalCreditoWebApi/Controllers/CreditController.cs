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

namespace TrivalCreditoWebApi.Controllers
{
 
    [Route("api/[controller]")]
    [ApiController]
    public class CreditController : ControllerBase
    {
        public const int limiteIntentos = 3;
        public const string SesionKeyAceptar = "_Aceptar";
        public const string SesionKeyNumeroIntento = "_NumeroIntento";

        ApiAppContext apiContext;

        public CreditController(ApiAppContext context)
        {
            apiContext = context;
            apiContext.Database.EnsureCreated();
        }

        [HttpPost]
        [Route("SolicitarCredito")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [RateLimitDecorator(StrategyType = StrategyTypeEnum.IpAddress)]
        public ActionResult<Response> SolicitarCredito([FromBody] Request value)
        {

            Response respuesta = new Response();
            Comun funciones = new Comun();
            bool verificarSolicitudAceptada = ConsultarSolicitudRegistrada();

            if (!verificarSolicitudAceptada)
            {
                //Consultar el número de intentos
                int numeroIntento = GetIntentoSesion();

                bool respuestaSolicitud = funciones.CalcularRiesgo(value);

                //Si la solicitud fue aprobada entonces se registra la solicitud, de lo contrario se rechaza
                if (respuestaSolicitud)
                {
                    if (string.IsNullOrEmpty(HttpContext.Session.GetString(SesionKeyAceptar)))
                    {
                        RegistrarSolicitud(value);
                    }
                    respuesta.Message = "Se aceptó y se autorizó la linea de credito de " + value.RequestCreditLine;

                }
                else
                {
                    numeroIntento++;
                    SetIntentoSesion(numeroIntento);
                    respuesta.Message = numeroIntento > limiteIntentos ? "Un agente de ventas lo contactará" : "La solicitud linea de credito de " + value.RequestCreditLine + " fue rechazada.";
                }
            }
            else
            {
                respuesta.Message = "Su solicitud ya fue aprobada";
            }

            return Ok(respuesta);

        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<Request> Get()
        {
            return apiContext.Requests;
        }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Post([FromBody] Request value)
        {
            apiContext.Requests.Add(value);
            await apiContext.SaveChangesAsync();
        }

        public void RegistrarSolicitud(Request value)
        {
            apiContext.Requests.Add(value);
            apiContext.SaveChangesAsync();
            HttpContext.Session.SetString(SesionKeyAceptar, "Aceptada");
        }

        public void SetIntentoSesion (int value)
        {
            HttpContext.Session.SetInt32(SesionKeyNumeroIntento, value);
        }

        public int GetIntentoSesion()
        {

            if (HttpContext.Session.GetInt32(SesionKeyNumeroIntento) == null)
            {
                SetIntentoSesion(0);
            }

            return HttpContext.Session.GetInt32(SesionKeyNumeroIntento).Value;
        }

        public bool ConsultarSolicitudRegistrada()
        {
            //Verificar si ya tiene un registro en sesión
            return string.IsNullOrEmpty(HttpContext.Session.GetString(SesionKeyAceptar)) ? false : true;
        }

    }
}
