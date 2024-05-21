using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models
{
    public class ConsumerDbContext: DbContext
    {
        public ConsumerDbContext(DbContextOptions<ConsumerDbContext> options): base(options)
        {
            
        }

        public DbSet<ConsumerGetRequest> ConsumerGets {get; set;}
    }
}