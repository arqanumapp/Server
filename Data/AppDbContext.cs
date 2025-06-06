using ArqanumServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArqanumServer.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
    }
}
