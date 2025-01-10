
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using InvoiceManagementSystem.Data.Repository.Interface;
using InvoiceManagementSystem.Service.Service;
using InvoiceManagementSystem.Data.Entity;
using InvoiceManagementSystem.Service.BusinessDomain;
using InvoiceManagementSystem.Service.Enum;

namespace InvoiceManagementSystem.Test
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> m_invoiceRepositoryMock;
        private readonly Mock<IMapper> m_mapperMock;
        private readonly Mock<ILogger<InvoiceService>> m_loggerMock;
        private readonly InvoiceService m_invoiceService;

        public InvoiceServiceTests()
        {
            m_invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            m_mapperMock = new Mock<IMapper>();
            m_loggerMock = new Mock<ILogger<InvoiceService>>();
            m_invoiceService = new InvoiceService(m_invoiceRepositoryMock.Object, m_mapperMock.Object, m_loggerMock.Object);
        }
        [Fact]
        public async Task GetAllInvoicesAsync_ReturnsInvoiceList()
        {
            // Arrange
            var invoiceEntities = new List<InvoiceEntity>
            {
                new InvoiceEntity { Id = 1, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = "pending" },
                new InvoiceEntity { Id = 2, Amount = 200, PaidAmount = 0, DueDate = DateTime.Now, Status = "pending" }
            };
            var invoiceDomains = new List<InvoiceDomain>
            {
                new InvoiceDomain { Id = 1, Amount = 100, paidAmount = 0, DueDate = DateTime.Now, Status = "pending" },
                new InvoiceDomain { Id = 2, Amount = 200, paidAmount = 0, DueDate = DateTime.Now, Status = "pending" }
            };

            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ReturnsAsync(invoiceEntities);
            m_mapperMock.Setup(mapper => mapper.Map<List<InvoiceDomain>>(invoiceEntities)).Returns(invoiceDomains);

            // Act
            var result = await m_invoiceService.GetAllInvoicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(invoiceDomains, result);
        }

        [Fact]
        public async Task GetAllInvoicesAsync_ThrowsException_LogsError()
        {
            // Arrange
            var exception = new Exception("Database error");
            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ThrowsAsync(exception);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.GetAllInvoicesAsync());
            Assert.Equal("Database error", ex.Message);
            
        }

        [Fact]
        public async Task GetInvoiceById_ShouldReturnInvoice_WhenInvoiceExists()
        {
            // Arrange
            var invoiceId = 1;
            var invoiceEntity = new InvoiceEntity { Id = invoiceId, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = "pending" };
            var invoiceDomain = new InvoiceDomain { Id = invoiceId, Amount = 100, paidAmount = 0, DueDate = DateTime.Now, Status = "pending" };

            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(invoiceId)).ReturnsAsync(invoiceEntity);
            m_mapperMock.Setup(mapper => mapper.Map<InvoiceDomain>(invoiceEntity)).Returns(invoiceDomain);

            // Act
            var result = await m_invoiceService.GetInvoiceById(invoiceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invoiceId, result.Id);
            Assert.Equal(invoiceEntity.Amount, result.Amount);
        }

        [Fact]
        public async Task GetInvoiceById_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var invoiceId = 1;
            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(invoiceId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.GetInvoiceById(invoiceId));    
        }

        [Fact]
        public async Task CreateInvoiceAsync_ShouldCreateInvoice()
        {
            // Arrange
            var amount = 100.0;
            var dueDate = DateTime.Now.AddDays(30);
            var invoiceDomain = new InvoiceDomain
            {
                Amount = amount,
                paidAmount = 0,
                DueDate = dueDate,
                Status = InvoicePaymentEnum.pending.ToString()
            };
            var invoiceEntity = new InvoiceEntity
            {
                Amount = amount,
                PaidAmount = 0,
                DueDate = dueDate,
                Status = InvoicePaymentEnum.pending.ToString()
            };

            m_mapperMock.Setup(m => m.Map<InvoiceEntity>(It.IsAny<InvoiceDomain>())).Returns(invoiceEntity);
            m_mapperMock.Setup(m => m.Map<InvoiceDomain>(It.IsAny<InvoiceEntity>())).Returns(invoiceDomain);
            m_invoiceRepositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await m_invoiceService.CreateInvoiceAsync(amount, dueDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(0, result.paidAmount);
            Assert.Equal(dueDate, result.DueDate);
            Assert.Equal(InvoicePaymentEnum.pending.ToString(), result.Status);
            m_invoiceRepositoryMock.Verify(r => r.AddInvoiceAsync(It.IsAny<InvoiceEntity>()), Times.Once);
            m_invoiceRepositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateInvoiceAsync_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var amount = 100.0;
            var dueDate = DateTime.Now.AddDays(30);
            var exception = new Exception("Test exception");

            m_mapperMock.Setup(m => m.Map<InvoiceEntity>(It.IsAny<InvoiceDomain>())).Throws(exception);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.CreateInvoiceAsync(amount, dueDate));
            Assert.Equal("Test exception", ex.Message);
        }

        [Fact]
        public async Task PayInvoiceAsync_ShouldUpdateInvoiceStatusToPaid()
        {
            // Arrange
            var id = 1;
            var amount = 100.0;
            var invoiceEntity = new InvoiceEntity { Id = id, Amount = 100.0, PaidAmount = 0, Status = "pending" };

            m_invoiceRepositoryMock.Setup(r => r.GetInvoiceAsync(id)).ReturnsAsync(invoiceEntity);
            m_invoiceRepositoryMock.Setup(r => r.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await m_invoiceService.PayInvoiceAsync(id, amount);

            // Assert
            m_invoiceRepositoryMock.Verify(r => r.UpdateInvoiceAsync(It.Is<InvoiceEntity>(i => i.Status == "paid")), Times.Once);
            m_invoiceRepositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task PayInvoiceAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var invoiceId = 1;
            var amount = 100.0;
            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(invoiceId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.PayInvoiceAsync(invoiceId, amount));
            Assert.Equal("Database error", exception.Message);          
        }

        [Fact]
        public async Task ProcessOverdueInvoicesAsync_ShouldProcessOverdueInvoices()
        {
            // Arrange
            var overdueDays = 30;
            var lateFee = 10.0;
            var overdueInvoice = new InvoiceEntity { Id = 1, Amount = 100.0, PaidAmount = 0, DueDate = DateTime.Now.AddDays(-overdueDays - 1), Status = "pending" };
            var overdueInvoices = new List<InvoiceEntity> { overdueInvoice };

            m_invoiceRepositoryMock.Setup(r => r.GetAllInvoicesAsync()).ReturnsAsync(overdueInvoices);
            m_invoiceRepositoryMock.Setup(r => r.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
            m_mapperMock.Setup(m => m.Map<InvoiceEntity>(It.IsAny<InvoiceDomain>())).Returns(overdueInvoice);
            m_mapperMock.Setup(m => m.Map<InvoiceDomain>(It.IsAny<InvoiceEntity>())).Returns(new InvoiceDomain());

            // Act
            await m_invoiceService.ProcessOverdueInvoicesAsync(lateFee, overdueDays);

            // Assert
            m_invoiceRepositoryMock.Verify(r => r.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>()), Times.Once);          
        }

        [Fact]
        public async Task ProcessOverdueInvoicesAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.ProcessOverdueInvoicesAsync(50, 30));
            Assert.Equal("Database error", exception.Message);
        }
    }
}