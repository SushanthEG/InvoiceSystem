using InvoiceManagementSystem.Data.Entity;
using InvoiceManagementSystem.Service.BusinessDomain;

namespace InvoiceManagementSystem.Service.Service.Interface
{
    public interface IInvoiceService
    {
        Task<InvoiceDomain> CreateInvoiceAsync(double amount, DateTime dueDate);
        Task<List<InvoiceDomain>> GetAllInvoicesAsync();
        Task<InvoiceDomain> GetInvoiceById(int id);
        Task PayInvoiceAsync(int id, double amount);
        Task ProcessOverdueInvoicesAsync(double lateFee, int overdueDays);
    }
}
