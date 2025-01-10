using InvoiceManagementSystem.Data.Data;
using InvoiceManagementSystem.Data.Entity;
using InvoiceManagementSystem.Data.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementSystem.Data
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly InvoiceDbContext m_invoiceDbContext;

        public InvoiceRepository(InvoiceDbContext invoiceDbContext)
        {
            m_invoiceDbContext = invoiceDbContext;
        }
        public async Task AddInvoiceAsync(InvoiceEntity invoice)
        {
            await m_invoiceDbContext.Invoice.AddAsync(invoice);
        }

        public async Task<List<InvoiceEntity>> GetAllInvoicesAsync() => await m_invoiceDbContext.Invoice.ToListAsync();

        public async Task<InvoiceEntity> GetInvoiceAsync(int id) => await m_invoiceDbContext.Invoice.FindAsync(id);

        public async Task SaveAsync() => await m_invoiceDbContext.SaveChangesAsync();

        public async Task UpdateInvoiceAsync(InvoiceEntity invoice) => m_invoiceDbContext.Invoice.Update(invoice);

    }
}
