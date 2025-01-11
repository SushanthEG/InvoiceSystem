using InvoiceManagementSystem.Data.Entity;

namespace InvoiceManagementSystem.Service.Helper
{
    public static class InvoiceHelper
    {
        public static double PayInvoice(double amount, InvoiceEntity invoice) =>  invoice.PaidAmount + amount;
        
        public static double LateFeeWithPartialPay(double lateFee, InvoiceEntity? invoice) => invoice.Amount - invoice.PaidAmount + lateFee;
        
        public static double LateFeeWithWithoutPay(double lateFee, InvoiceEntity? invoice) => invoice.Amount + lateFee;
        
    }
}
