namespace InvoiceManagementSystem.API.DTO
{
    public class UpdateInvoiceRequestDto
    {
        public double Amount { get; set; }
        public double PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }
}
