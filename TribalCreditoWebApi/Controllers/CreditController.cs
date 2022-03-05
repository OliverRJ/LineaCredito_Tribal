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
        private const int minutosEspera = 1;
        private const int segundosEspera = 10;
        private const string SesionKeyEstadoSolicitud = "_Estado";
        private const string SesionKeyNumeroIntento = "_NumeroIntento";
        private const string SesionKeyTiempoEsperaAceptado = "_TiempoEsperaA";
        private const string SesionKeyTiempoEsperaRechazado = "_TiempoEsperaR";

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
            Comun funciones = new Comun();

            int _numeroIntento = GetIntentoSesion();
            string estadoSolicitud = GetEstadoSesion();
            DateTime tiempoLimiteRechazado = Convert.ToDateTime(GetTiempoSesion(SesionKeyTiempoEsperaRechazado));

            /*validar si existe tiempo de espera por límite de peticiones rechazadas*/
            if (DateTime.Now > tiempoLimiteRechazado)
            {
                /* Validar si existe una solicitud previamente aprobada o rechazada.
                 * Si la petición es nueva o anteriormente rechazada entonces se vuelve a calcular el riesgo.
                 * Si la solitud ya fue aprobada sólo validar el número de intentos en el tiempo límite.
                 */
                if (estadoSolicitud == "Rechazado")
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
                       //Primero suma el número de intentos RECHAZADOS y luego valida la cantidad de intentos.
                        _numeroIntento++;
                        return ValidarCantidadPeticionRechazada(_numeroIntento, miSolicitud.RequestCreditLine);
                    }
                }
                else
                {
                    //Primero suma el número de intentos ACEPTADOS y luego valida la cantidad de intentos y el tiempo limite.
                    _numeroIntento++;
                    return ValidarCantidadPeticionAceptada(_numeroIntento);
                }
            }
            else
            {
                //Se muestra mensaje con la cantidad de segundos a esperar antes de realizar otra petición.
                TimeSpan tiempoEspera = tiempoLimiteRechazado - DateTime.Now;
                return StatusCode(429, $"Exceso de peticiones, por favor vuelva a intentar en {tiempoEspera.Seconds} segundos.");
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
            //Registrar petición en una BD temporal en memoria.
            apiContext.Requests.Add(miSolicitud);
            apiContext.SaveChangesAsync();
            //Actualizar las variables de sesión y el mensaje a mostrar en el request.
            SetIntentoSesion(0);
            SetEstadoSesion("Aceptada");
            SetTiempoSesion(SesionKeyTiempoEsperaAceptado, DateTime.Now.AddMinutes(minutosEspera).ToString());
            respuesta.Message = "Se aceptó y se autorizó la linea de credito de " + miSolicitud.RequestCreditLine;

            return Ok(respuesta);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ObjectResult ValidarCantidadPeticionRechazada(int intento, int montoLineaRechazada) {

            Response respuesta = new Response();
            SetEstadoSesion("Rechazado");

            /* Validar el numero de intentos de una solicitud rechazada.
             * Si el número de intentos es mayor a 3, entonces se muentra mensaje de "Agente lo contactará".
             * De lo contrario se muestra el credito rechazado y se establece tiempo limite de tiempo para volver a realizar una petición.
             */
            if (intento > limiteIntentos)
            {
                respuesta.Message = "Un agente de ventas lo contactará.";
                return Ok(respuesta);
            }
            else
            {
                respuesta.Message = $"La solicitud linea de credito de {montoLineaRechazada} fue rechazada.";
                SetTiempoSesion(SesionKeyTiempoEsperaRechazado, DateTime.Now.AddSeconds(segundosEspera).ToString());
                SetIntentoSesion(intento);
                return Ok(respuesta);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ObjectResult ValidarCantidadPeticionAceptada(int intento)
        {
            Response respuesta = new Response();
            SetIntentoSesion(intento);
            DateTime tiempoLimite = Convert.ToDateTime(GetTiempoSesion(SesionKeyTiempoEsperaAceptado));
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
        public void SetIntentoSesion (int value)
        {
            HttpContext.Session.SetInt32(SesionKeyNumeroIntento, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetIntentoSesion()
        {

            if (HttpContext.Session.GetInt32(SesionKeyNumeroIntento) == null)
            {
                SetIntentoSesion(0);
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
            string estadoSolicitud = HttpContext.Session.GetString(SesionKeyEstadoSolicitud);
            if (string.IsNullOrEmpty(estadoSolicitud)){
                estadoSolicitud = "Rechazado";
            }
            return estadoSolicitud;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetTiempoSesion(string key, string value)
        {
            HttpContext.Session.SetString(key, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public string GetTiempoSesion(string key)
        {
            string tiempoLimite = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(tiempoLimite)){
                tiempoLimite = DateTime.Now.ToString();
            }
            return tiempoLimite;
        }

        #endregion
    }
}
