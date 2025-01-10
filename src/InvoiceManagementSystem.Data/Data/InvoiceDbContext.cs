using InvoiceManagementSystem.Data.Entity;
using Microsoft.EntityFrameworkCore;
namespace InvoiceManagementSystem.Data.Data
{
    public class InvoiceDbContext : DbContext
    {

        public InvoiceDbContext(DbContextOptions<InvoiceDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        public DbSet<InvoiceEntity> Invoice { get; set; }
    }
}
