using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrivalCreditoWebApi.Context;
using TrivalCreditoWebApi.Models;

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
        public ActionResult<string> CalcularCredito(int id)
        {
            if (id > 0)
            {
                return ("value");
            }
            else
            {
                return NotFound();
            }
            // 429 Too Many Requests
        }

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
