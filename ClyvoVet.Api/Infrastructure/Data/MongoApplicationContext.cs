using ClyvoVet.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data
{
    public class MongoApplicationContext : DbContext
    {
        public MongoApplicationContext(DbContextOptions<MongoApplicationContext> options) : base(options)
        {
        }

        public DbSet<HistoricoClinico> HistoricosClinicos { get; set; }
    }
}
