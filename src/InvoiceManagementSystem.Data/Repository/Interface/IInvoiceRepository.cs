using InvoiceManagementSystem.Data.Entity;

namespace InvoiceManagementSystem.Data.Repository.Interface
{
    public interface IInvoiceRepository
    {
        Task<InvoiceEntity> GetInvoiceAsync(int id);
        Task<List<InvoiceEntity>> GetAllInvoicesAsync();
        Task AddInvoiceAsync(InvoiceEntity invoice);
        Task UpdateInvoiceAsync(InvoiceEntity invoice);
        Task SaveAsync();
    }
}
