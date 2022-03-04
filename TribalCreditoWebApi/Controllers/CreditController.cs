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
        private const int limiteIntentos = 3;
        private const int minutosEspera = 2;
        private const int segundosEspera = 30;
        private const string SesionKeyEstadoSolicitud = "_Estado";
        private const string SesionKeyNumeroIntento = "_NumeroIntento";
        private const string SesionKeyTiempoEspera = "_TiempoEspera";

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
        public ActionResult<Response> SolicitarCredito([FromBody] Request miSolicitud)
        {

            Response respuesta = new Response();
            Comun funciones = new Comun();

            int _numeroIntento = GetIntentoSesion();
            string estadoSolicitud = GetEstadoSesion();

            /* Validar si existe una solicitud previamente aprobada o rechazada.
             * Si la petición es nueva o anteriormente rechazada entonces se vuelve a calcular el riesgo.
             * Si la solitud ya fue aprobada sólo validar el número de intentos en el tiempo límite.
             */
            if (string.IsNullOrEmpty(estadoSolicitud) || estadoSolicitud == "Rechazado")
            {
                //Invocar a función para calcular el riesgo
                bool respuestaSolicitud = funciones.CalcularRiesgo(miSolicitud);

                //Si la solicitud fue aprobada entonces se registra, de lo contrario se rechaza.
                if (respuestaSolicitud)
                {
                    return RegistrarSolicitudAprobada(miSolicitud);
                }
                else
                {
                    /* Primero sumar el número de intentos RECHAZADOS.
                     * Luego validar la cantidad de intentos y el tiempo limite.
                     */
                    _numeroIntento++;
                    return ValidarCantidadPeticionRechazada(_numeroIntento, miSolicitud.RequestCreditLine);
                }
            }
            else
            {
                /* Primero sumar el número de intentos ACEPTADOS.
                 * Luego validar la cantidad de intentos y el tiempo limite.
                 */
                _numeroIntento++;
                return ValidarCantidadPeticionAceptada(_numeroIntento);
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
        ///Servicio adicional para Registrar Solicitud de Crédito.
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
        public ObjectResult RegistrarSolicitudAprobada(Request miSolicitud)
        {
            Response respuesta = new Response();
            /*Registrar petición en una BD temporal el memoria.
             * Actualizar las variables de sesión y el mensaje a mostrar en el request.
             */
            apiContext.Requests.Add(miSolicitud);
            apiContext.SaveChangesAsync();
            SetIntentoSesion(SesionKeyNumeroIntento, 0);
            SetEstadoSesion("Aceptada");
            SetTiempoSesion(DateTime.Now.AddMinutes(minutosEspera).ToString());
            respuesta.Message = "Se aceptó y se autorizó la linea de credito de " + miSolicitud.RequestCreditLine;

            return Ok(respuesta);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ObjectResult ValidarCantidadPeticionRechazada(int intento, int montoLineaRechazada) {

            Response respuesta = new Response();
            SetEstadoSesion("Rechazado");
            DateTime tiempoLimite = Convert.ToDateTime(GetTiempoSesion());

            /* Validar el numero de intentos de acuerdo al tiempo límite.
             * Bloqueaer peticiones dentro de los 30 seg a la petición anterior, si es asi devolver código http 429.
             * Después de 3 intentos devolver mensaje: Un agente de ventas lo contactará.
             */
            if (string.IsNullOrEmpty(GetTiempoSesion()) || DateTime.Now > tiempoLimite)
            {
                if (intento >= limiteIntentos)
                {
                    respuesta.Message = "Un agente de ventas lo contactará.";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Message = $"La solicitud linea de credito de {montoLineaRechazada} fue rechazada.";
                    SetTiempoSesion(DateTime.Now.AddSeconds(segundosEspera).ToString());
                    SetIntentoSesion(SesionKeyNumeroIntento, intento);
                    return Ok(respuesta);
                }
            }
            else
            {

                tiempoLimite = Convert.ToDateTime(GetTiempoSesion());
                if (DateTime.Now < tiempoLimite)
                {
                    SetIntentoSesion(SesionKeyNumeroIntento, 0);
                    TimeSpan tiempoEspera = tiempoLimite - DateTime.Now;
                    return StatusCode(429, $"Exceso de peticiones, por favor vuelva a intentar en {tiempoEspera.Seconds} segundos.");
                }
                else
                {
                    respuesta.Message = $"La solicitud linea de credito de {montoLineaRechazada} fue rechazada.";
                    SetTiempoSesion(DateTime.Now.AddSeconds(segundosEspera).ToString());
                    SetIntentoSesion(SesionKeyNumeroIntento, intento);
                    return Ok(respuesta);
                }
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ObjectResult ValidarCantidadPeticionAceptada(int intento)
        {
            Response respuesta = new Response();
            SetIntentoSesion(SesionKeyNumeroIntento, intento);
            DateTime tiempoLimite = Convert.ToDateTime(HttpContext.Session.GetString(SesionKeyTiempoEspera));
            /* Validar el numero de intentos de acuerdo al tiempo límite.
             * Si en 2 min sigue intentando, entonces enviar código http 429, de lo contrario mostra mensaje de petición aceptada.
             * La línea de crédito debe ser la misma independientemente de las entradas.
             */

            if (DateTime.Now < tiempoLimite && intento >= limiteIntentos)
            {
                return StatusCode(429, $"Exceso de peticiones, recuerde que ya tiene una línea de crédito aprobada.");
            }
            else
            {
                respuesta.Message = "Ya tiene una línea de crédito autorizada.";
                return Ok(respuesta);
            }
        }

        #region Manejo de Sesión

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetIntentoSesion (string key, int value)
        {
            HttpContext.Session.SetInt32(key, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetIntentoSesion()
        {

            if (HttpContext.Session.GetInt32(SesionKeyNumeroIntento) == null)
            {
                SetIntentoSesion(SesionKeyNumeroIntento, 0);
            }

            return HttpContext.Session.GetInt32(SesionKeyNumeroIntento).Value;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetEstadoSesion(string value)
        {
            HttpContext.Session.SetString(SesionKeyEstadoSolicitud, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public string GetEstadoSesion()
        {
            return HttpContext.Session.GetString(SesionKeyEstadoSolicitud);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetTiempoSesion(string value)
        {
            HttpContext.Session.SetString(SesionKeyTiempoEspera, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public string GetTiempoSesion()
        {
            return HttpContext.Session.GetString(SesionKeyTiempoEspera);
        }

        #endregion
    }
}
