using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrivalCreditoWebApi.Models;

namespace TrivalCreditoWebApi.Context
{
    public class ApiAppContext :DbContext
    {
        public DbSet<Request> Requests { get; set; }
        public ApiAppContext(DbContextOptions<ApiAppContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            List<Request> usersInitData = new List<Request>();
            builder.Entity<Request>().ToTable("Request").HasKey(p => p.RequestId);

        }
    }

}
