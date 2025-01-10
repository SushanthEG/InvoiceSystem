namespace InvoiceManagementSystem.API.DTO
{
    public class CreateInvoiceRequestDto
    {
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }

    }
}
