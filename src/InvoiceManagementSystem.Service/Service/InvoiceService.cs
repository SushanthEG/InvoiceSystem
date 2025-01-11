
using InvoiceManagementSystem.Data.Repository.Interface;
using InvoiceManagementSystem.Service.BusinessDomain;
using InvoiceManagementSystem.Service.Service.Interface;
using AutoMapper;
using InvoiceManagementSystem.Data.Entity;
using InvoiceManagementSystem.Service.Enum;
using Microsoft.Extensions.Logging;
using InvoiceManagementSystem.Service.Helper;

namespace InvoiceManagementSystem.Service.Service
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository m_invoiceRepository;
        private readonly IMapper m_mapper;
        private readonly ILogger<InvoiceService> m_logger;

        public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper, ILogger<InvoiceService> logger)
        {
            m_invoiceRepository = invoiceRepository;
            m_mapper = mapper;
            m_logger = logger;
        }

        public async Task<InvoiceDomain> CreateInvoiceAsync(double amount, DateTime dueDate)
        {
            try
            {
                var invoice = new InvoiceDomain
                {
                    Amount = amount,
                    PaidAmount = 0,
                    DueDate = dueDate,
                    Status = InvoicePaymentEnum.Pending.ToString()
                };

                var invoiceEntity = m_mapper.Map<InvoiceEntity>(invoice);
                await m_invoiceRepository.AddInvoiceAsync(invoiceEntity);
                await m_invoiceRepository.SaveAsync();

                return m_mapper.Map<InvoiceDomain>(invoiceEntity);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while creating an invoice.");
                throw;
            }
        }

        public async Task<List<InvoiceDomain>> GetAllInvoicesAsync()
        {
            try
            {
                var invoiceEntities = await m_invoiceRepository.GetAllInvoicesAsync();
                return m_mapper.Map<List<InvoiceDomain>>(invoiceEntities);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while retrieving all invoices.");
                throw;
            }
        }

        public async Task<InvoiceDomain> GetInvoiceById(int id)
        {
            try
            {
                var invoiceEntity = await m_invoiceRepository.GetInvoiceAsync(id);
                return m_mapper.Map<InvoiceDomain>(invoiceEntity);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while retrieving the invoice with ID {InvoiceId}", id);
                throw;
            }
        }

        public async Task PayInvoiceAsync(int id, double amount)
        {
            try
            {
                var invoice = await m_invoiceRepository.GetInvoiceAsync(id);
                if (invoice == null || invoice.Status != InvoicePaymentEnum.Pending.ToString()) return;

                invoice.PaidAmount = InvoiceHelper.PayInvoice(amount, invoice);
                if (invoice.PaidAmount >= invoice.Amount)
                {
                    invoice.Status = InvoicePaymentEnum.Paid.ToString();
                    if (invoice.PaidAmount > invoice.Amount)
                    {
                        double overpaidAmount = invoice.PaidAmount - invoice.Amount;
                        m_logger.LogInformation("Invoice with ID {InvoiceId} has been overpaid by {OverpaidAmount}.", id, overpaidAmount);
                    }
                }

                await m_invoiceRepository.UpdateInvoiceAsync(invoice);
                await m_invoiceRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while paying the invoice with ID {InvoiceId}", id);
                throw;
            }
        }

        public async Task ProcessOverdueInvoicesAsync(double lateFee, int overdueDays)
        {
            try
            {
                var overdueInvoices = await GetOverdueInvoicesAsync(overdueDays);

                foreach (var invoice in overdueInvoices)
                {
                    await ProcessSingleOverdueInvoiceAsync(invoice, lateFee, overdueDays);
                }

                await m_invoiceRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while processing overdue invoices.");
                throw;
            }
        }

        private async Task<List<InvoiceEntity>> GetOverdueInvoicesAsync(int overdueDays)
        {
            var allInvoices = await m_invoiceRepository.GetAllInvoicesAsync();
            return allInvoices
                .Where(i => i.Status == InvoicePaymentEnum.Pending.ToString() && i.DueDate.AddDays(overdueDays) < DateTime.Now)
                .ToList();
        }

        private async Task ProcessSingleOverdueInvoiceAsync(InvoiceEntity invoice, double lateFee, int overdueDays)
        {
            if (invoice.PaidAmount > 0)
            {
                await HandlePartialPaymentAsync(invoice, lateFee, overdueDays);
            }
            else
            {
                await HandleNoPaymentAsync(invoice, lateFee, overdueDays);
            }

            await m_invoiceRepository.UpdateInvoiceAsync(invoice);
            await m_invoiceRepository.SaveAsync();
        }

        private async Task HandlePartialPaymentAsync(InvoiceEntity invoice, double lateFee, int overdueDays)
        {
            double newAmount = InvoiceHelper.LateFeeWithPartialPay(lateFee, invoice);
            invoice.Status = InvoicePaymentEnum.Paid.ToString();
            await CreateInvoiceAsync(newAmount, DateTime.Now.AddDays(overdueDays));
        }

        private async Task HandleNoPaymentAsync(InvoiceEntity invoice, double lateFee, int overdueDays)
        {
            double newAmount = InvoiceHelper.LateFeeWithWithoutPay(lateFee, invoice);
            invoice.Status = InvoicePaymentEnum.Voided.ToString();
            await CreateInvoiceAsync(newAmount, DateTime.Now.AddDays(overdueDays));
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            try
            {
                var invoice = await m_invoiceRepository.GetInvoiceAsync(id);
                if (invoice == null) return;

                await m_invoiceRepository.DeleteInvoiceAsync(id);
                await m_invoiceRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while deleting the invoice with ID {InvoiceId}", id);
                throw;
            }
        }

        public async Task UpdateInvoiceAsync(InvoiceDomain invoice)
        {
            try
            {
                var existingInvoice = await m_invoiceRepository.GetInvoiceAsync(invoice.Id);
                if (existingInvoice == null)
                {
                    m_logger.LogWarning("Invoice with ID {InvoiceId} not found.", invoice.Id);
                    return;
                }

                existingInvoice.Amount = invoice.Amount;
                existingInvoice.PaidAmount = invoice.PaidAmount;
                existingInvoice.DueDate = invoice.DueDate;
                existingInvoice.Status = invoice.Status;

                await m_invoiceRepository.UpdateInvoiceAsync(existingInvoice);
                await m_invoiceRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while updating the invoice with ID {InvoiceId}", invoice.Id);
                throw;
            }
        }
    }
}
