using InvoiceManagementSystem.Data.Entity;

namespace InvoiceManagementSystem.Service.Helper
{
    public static class InvoiceHelper
    {
        public static double PayInvoice(double amount, InvoiceEntity invoice) =>invoice.Amount - invoice.PaidAmount;//less than or eq       
        public static double LateFeeWithWithoutPay(double lateFee, InvoiceEntity? invoice) => invoice.Amount + lateFee;

    }
}
