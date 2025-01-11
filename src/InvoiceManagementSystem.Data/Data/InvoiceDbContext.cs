using InvoiceManagementSystem.Data.Entity;
using Microsoft.EntityFrameworkCore;
namespace InvoiceManagementSystem.Data.Data
{
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext(DbContextOptions<InvoiceDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<InvoiceEntity> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<InvoiceEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired();
                entity.Property(e => e.PaidAmount).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });
        }
    }
}
