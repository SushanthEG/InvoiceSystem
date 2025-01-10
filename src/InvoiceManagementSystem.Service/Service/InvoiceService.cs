
using InvoiceManagementSystem.Data.Repository.Interface;
using InvoiceManagementSystem.Service.BusinessDomain;
using InvoiceManagementSystem.Service.Service.Interface;
using AutoMapper;
using InvoiceManagementSystem.Data.Entity;
using InvoiceManagementSystem.Service.Enum;
using Microsoft.Extensions.Logging;

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
                    paidAmount = 0,
                    DueDate = dueDate,
                    Status = InvoicePaymentEnum.pending.ToString()
                };

                var invoiceEntity = m_mapper.Map<InvoiceEntity>(invoice);
                await m_invoiceRepository.AddInvoiceAsync(invoiceEntity);
                await m_invoiceRepository.SaveAsync();
                invoice = m_mapper.Map<InvoiceDomain>(invoiceEntity);
                return invoice;
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
                var invoiceList = m_mapper.Map<List<InvoiceDomain>>(invoiceEntities);
                return invoiceList;
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
                var invoiceEntities = await m_invoiceRepository.GetInvoiceAsync(id);
                var invoiceList = m_mapper.Map<InvoiceDomain>(invoiceEntities);
                return invoiceList;
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while retrieving all invoices.");
                throw;
            }
        }

        public async Task PayInvoiceAsync(int id, double amount)
        {
            try
            {
                var invoice = await m_invoiceRepository.GetInvoiceAsync(id);
                if (invoice == null || invoice.Status != "pending") return;

                invoice.PaidAmount = invoice.PaidAmount + amount;
                if (invoice.PaidAmount >= invoice.Amount)
                {
                    invoice.Status = InvoicePaymentEnum.paid.ToString();
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
                var overdueInvoices = (await m_invoiceRepository.GetAllInvoicesAsync())
                    .Where(i => i.Status == "pending" && i.DueDate.AddDays(overdueDays) < DateTime.Now)
                    .ToList();

                foreach (var invoice in overdueInvoices)
                {
                    if (invoice.PaidAmount > 0)
                    {
                        var newAmount = invoice.Amount - invoice.PaidAmount + lateFee;
                        invoice.Status = InvoicePaymentEnum.paid.ToString();
                        await CreateInvoiceAsync(newAmount, DateTime.Now.AddDays(overdueDays));
                    }
                    else
                    {
                        var newAmount = invoice.Amount + lateFee;
                        invoice.Status = InvoicePaymentEnum.voided.ToString();
                        await CreateInvoiceAsync(newAmount, DateTime.Now.AddDays(overdueDays));
                    }
                    await m_invoiceRepository.UpdateInvoiceAsync(invoice);
                }
                await m_invoiceRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while processing overdue invoices.");
                throw;
            }
        }
    }
}
