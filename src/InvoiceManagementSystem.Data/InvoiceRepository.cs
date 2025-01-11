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
            await m_invoiceDbContext.Invoices.AddAsync(invoice);
            await SaveAsync();
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            var invoice = await GetInvoiceAsync(id);
            if (invoice != null)
            {
                m_invoiceDbContext.Invoices.Remove(invoice);
                await SaveAsync();
            }
        }

        public async Task<List<InvoiceEntity>> GetAllInvoicesAsync() => await m_invoiceDbContext.Invoices.ToListAsync();

        public async Task<InvoiceEntity> GetInvoiceAsync(int id) => await m_invoiceDbContext.Invoices.FindAsync(id);

        public async Task SaveAsync() => await m_invoiceDbContext.SaveChangesAsync();

        public async Task UpdateInvoiceAsync(InvoiceEntity invoice)
        {
            m_invoiceDbContext.Invoices.Update(invoice);
            await SaveAsync();
        }
    }
}
