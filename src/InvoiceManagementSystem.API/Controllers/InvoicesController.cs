using InvoiceManagementSystem.API.DTO;
using InvoiceManagementSystem.Service.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService m_invoiceService;
        private readonly ILogger<InvoicesController> m_logger;

        public InvoicesController(IInvoiceService service, ILogger<InvoicesController> logger)
        {
            m_invoiceService = service;
            m_logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var invoice = await m_invoiceService.CreateInvoiceAsync(requestDto.Amount, requestDto.DueDate);
                return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while creating the invoice.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the invoice.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            try
            {
                var invoices = await m_invoiceService.GetAllInvoicesAsync();
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while retrieving all invoices.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving all invoices.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            try
            {
                var invoice = await m_invoiceService.GetInvoiceById(id);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }
                m_logger.LogInformation($"Retrieved invoice with ID {id}.");
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occurred while retrieving the invoice with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the invoice.");
            }
        }

        [HttpPost("{id}/payments")]
        public async Task<IActionResult> PayInvoice(int id, [FromBody] PayInvoiceRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                await m_invoiceService.PayInvoiceAsync(id, requestDto.Amount);
                return Ok();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occurred while paying the invoice with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while paying the invoice.");
            }
        }

        [HttpPost("process-overdue")]
        public async Task<IActionResult> ProcessOverdueInvoices([FromBody] ProcessOverdueRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                await m_invoiceService.ProcessOverdueInvoicesAsync(requestDto.LateFee, requestDto.OverdueDays);
                return Ok();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "An error occurred while processing overdue invoices.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing overdue invoices.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            try
            {
                var invoice = await m_invoiceService.GetInvoiceById(id);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                await m_invoiceService.DeleteInvoiceAsync(id);
                return Ok($"Invoice with ID {id} has been deleted.");
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occurred while deleting the invoice with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the invoice.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] UpdateInvoiceRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var invoice = await m_invoiceService.GetInvoiceById(id);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                invoice.Amount = requestDto.Amount;
                invoice.DueDate = requestDto.DueDate;
                invoice.Status = requestDto.Status;

                await m_invoiceService.UpdateInvoiceAsync(invoice);
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occurred while updating the invoice with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the invoice.");
            }
        }
    }
}
