using InvoiceManagementSystem.Data.Entity;

namespace InvoiceManagementSystem.Service.Helper
{
    public static class InvoiceHelper
    {
        public static double PayInvoice(double amount, InvoiceEntity invoice) =>invoice.Amount - invoice.PaidAmount;    
        public static double LateFeePay(double lateFee, InvoiceEntity? invoice) => invoice.Amount + lateFee;
        public static void PayInvoiceCalculate(double amount, InvoiceEntity invoice)
        {
            invoice.Amount = invoice.Amount - amount;
            invoice.PaidAmount = invoice.PaidAmount + amount;
        }
    }
}
