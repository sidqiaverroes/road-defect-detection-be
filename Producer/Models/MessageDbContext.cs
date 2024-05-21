using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Producer.Models
{
    public class MessageDbContext: DbContext
    {
        public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options) { }
        public DbSet<MessageUpdateRequest> MessageUpdates { get; set; }
    }
}