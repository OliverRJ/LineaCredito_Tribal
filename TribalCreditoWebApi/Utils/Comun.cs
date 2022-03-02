using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrivalCreditoWebApi.Models;

namespace TribalCreditoWebApi.Utils
{
    public class Comun
    {

        public bool CalcularRiesgo(Request peticion)
        {
            bool estadopeticion = false;
            decimal lineaCreditoRecomendada = 0;

            if (peticion.FoundingType == "Startup")
            {
                lineaCreditoRecomendada = Math.Max(peticion.MontlyRevenue / 5, peticion.CashBalance / 3);

            } else if(peticion.FoundingType == "SME")
            {
                lineaCreditoRecomendada = peticion.MontlyRevenue / 5;
            }

            estadopeticion = ConfirmarSolicitud(lineaCreditoRecomendada, peticion.RequestCreditLine);

            return estadopeticion;
        }


        public bool ConfirmarSolicitud(decimal lineaRecomendada, decimal lineaSolicitada)
        {
            bool estadoSolicitud = false;
            if(lineaRecomendada > lineaSolicitada )
            {
                estadoSolicitud = true;
            }

            return estadoSolicitud;
        }



    }
}
