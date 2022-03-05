using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrivalCreditoWebApi.Models
{
    public class Response
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }
}
