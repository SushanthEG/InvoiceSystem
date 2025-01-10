namespace InvoiceManagementSystem.API.DTO
{
    public class ProcessOverdueRequestDto
    {
        public double LateFee { get; set; }
        public int OverdueDays { get; set; }
    }
}
