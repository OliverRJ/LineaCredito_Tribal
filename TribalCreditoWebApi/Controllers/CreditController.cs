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

namespace TrivalCreditoWebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CreditController : ControllerBase
    {
        public const int limiteIntentos = 3;
        public const int minutosEspera = 1;
        public const int segundosEspera = 5;
        public const string SesionKeyEstadoSolicitud = "_Estado";
        public const string SesionKeyNumeroIntento = "_NumeroIntento";
        public const string SesionKeyNumeroSolicitud = "_NumeroSolicitud";
        //public const string SesionKeyNumeroIntentoRechazo = "_NumeroIntentoRec";
        //public const string SesionKeyNumeroIntentoAceptado = "_NumeroIntentoAce";
        public const string SesionKeyTiempoEspera = "_TiempoEspera";

        ApiAppContext apiContext;

        public CreditController(ApiAppContext context)
        {
            apiContext = context;
            apiContext.Database.EnsureCreated();
        }

        //<sumary> 
        ///Método para Solicitar Crédito para Pyme y Stratup
        ///</sumary>
        [HttpPost]
        [Route("SolicitarCredito")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Response> SolicitarCredito([FromBody] Request value)
        {

            Response respuesta = new Response();
            Comun funciones = new Comun();
            int _numeroIntento = GetIntentoSesion(SesionKeyNumeroIntento);

            string estadoSolicitud = HttpContext.Session.GetString(SesionKeyEstadoSolicitud);
            if (string.IsNullOrEmpty(estadoSolicitud) || estadoSolicitud == "Rechazado")
            {
                bool respuestaSolicitud = funciones.CalcularRiesgo(value);

                //Si la solicitud fue aprobada entonces se registra la solicitud, de lo contrario se rechaza
                if (respuestaSolicitud)
                {
                    RegistrarSolicitud(value);
                    SetIntentoSesion(SesionKeyNumeroIntento, 0);
                    respuesta.Message = "Se aceptó y se autorizó la linea de credito de " + value.RequestCreditLine;
                    return Ok(respuesta);
                }
                else
                {
                    //_numeroIntento++;
                    respuesta.Message = "La solicitud linea de credito de " + value.RequestCreditLine + " fue rechazada.";
                    _numeroIntento++;
                    SetIntentoSesion(SesionKeyNumeroIntento, _numeroIntento);
                    HttpContext.Session.SetString(SesionKeyEstadoSolicitud, "Rechazado");
                    if (string.IsNullOrEmpty(HttpContext.Session.GetString(SesionKeyTiempoEspera)))
                    {
                        HttpContext.Session.SetString(SesionKeyTiempoEspera, DateTime.Now.AddSeconds(segundosEspera).ToString());
                        return Ok(respuesta);
                    }
                    else
                    {
                        
                        DateTime tiempoLimite = Convert.ToDateTime(HttpContext.Session.GetString(SesionKeyTiempoEspera));
                        if (DateTime.Now < tiempoLimite)
                        {
                            SetIntentoSesion(SesionKeyNumeroIntento, 0);
                            return StatusCode(429, "Exceso de peticiones");
                        }
                        else
                        {
                            if (_numeroIntento >= 3)
                            {
                                respuesta.Message = "Un agente de ventas lo contactará";
                                return Ok(respuesta);
                            }
                            else
                            {
                                HttpContext.Session.SetString(SesionKeyTiempoEspera, DateTime.Now.AddSeconds(segundosEspera).ToString());
                                respuesta.Message = "La solicitud linea de credito de " + value.RequestCreditLine + " fue rechazada.";
                                return Ok(respuesta);
                            }
                        }
                    }
                }
            }
            else
            {

                _numeroIntento++;
                SetIntentoSesion(SesionKeyNumeroIntento, _numeroIntento);
                DateTime tiempoLimite = Convert.ToDateTime(HttpContext.Session.GetString(SesionKeyTiempoEspera));
                if (DateTime.Now < tiempoLimite && _numeroIntento >= 3)
                {
                    return StatusCode(429, "Exceso de peticiones");
                }
                else
                {
                    respuesta.Message = "Ya tiene una linea de de credito autorizada";
                    return Ok(respuesta);
                }
            }
        }

        ///<sumary> 
        ///Método para Listar Solicitar Crédito Aprobadas
        ///</sumary>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<Request> Get()
        {
            return apiContext.Requests;
        }

        ///<sumary> 
        ///Método para Registrar Solicitar Crédito
        ///</sumary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Post([FromBody] Request value)
        {
            apiContext.Requests.Add(value);
            await apiContext.SaveChangesAsync();
        }



        [ApiExplorerSettings(IgnoreApi = true)]
        public void RegistrarSolicitud(Request value)
        {
            apiContext.Requests.Add(value);
            apiContext.SaveChangesAsync();
            HttpContext.Session.SetString(SesionKeyEstadoSolicitud, "Aceptada");
            HttpContext.Session.SetString(SesionKeyTiempoEspera, DateTime.Now.AddMinutes(minutosEspera).ToString());
        }

    
        #region Manejo de Sesión

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetIntentoSesion (string key, int value)
        {
            HttpContext.Session.SetInt32(key, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetIntentoSesion(string key)
        {

            if (HttpContext.Session.GetInt32(key) == null)
            {
                SetIntentoSesion(key, 0);
            }

            return HttpContext.Session.GetInt32(key).Value;
        }

        #endregion
    }
}
