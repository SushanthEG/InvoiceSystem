namespace InvoiceManagementSystem.Service.BusinessDomain
{
    public class InvoiceDomain
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public double PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } 
    }
}
