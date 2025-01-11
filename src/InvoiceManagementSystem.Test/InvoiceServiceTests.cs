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
                    new InvoiceEntity { Id = 1, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() },
                    new InvoiceEntity { Id = 2, Amount = 200, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() }
                };
            var invoiceDomains = new List<InvoiceDomain>
                {
                    new InvoiceDomain { Id = 1, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() },
                    new InvoiceDomain { Id = 2, Amount = 200, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() }
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
            var invoiceEntity = new InvoiceEntity { Id = invoiceId, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() };
            var invoiceDomain = new InvoiceDomain { Id = invoiceId, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() };

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
        public async Task GetInvoiceById_ShouldReturnNull_WhenInvoiceDoesNotExist()
        {
            // Arrange
            var invoiceId = 1;
            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(invoiceId)).ReturnsAsync((InvoiceEntity)null);

            // Act
            var result = await m_invoiceService.GetInvoiceById(invoiceId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetInvoiceById_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var invoiceId = 1;
            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(invoiceId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.GetInvoiceById(invoiceId));
            Assert.Equal("Database error", exception.Message);
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
                PaidAmount = 0,
                DueDate = dueDate,
                Status = InvoicePaymentEnum.Pending.ToString()
            };
            var invoiceEntity = new InvoiceEntity
            {
                Amount = amount,
                PaidAmount = 0,
                DueDate = dueDate,
                Status = InvoicePaymentEnum.Pending.ToString()
            };

            m_mapperMock.Setup(m => m.Map<InvoiceEntity>(It.IsAny<InvoiceDomain>())).Returns(invoiceEntity);
            m_mapperMock.Setup(m => m.Map<InvoiceDomain>(It.IsAny<InvoiceEntity>())).Returns(invoiceDomain);
            m_invoiceRepositoryMock.Setup(repo => repo.AddInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await m_invoiceService.CreateInvoiceAsync(amount, dueDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(amount, result.Amount);
            Assert.Equal(0, result.PaidAmount);
            Assert.Equal(dueDate, result.DueDate);
            Assert.Equal(InvoicePaymentEnum.Pending.ToString(), result.Status);
            m_invoiceRepositoryMock.Verify(repo => repo.AddInvoiceAsync(It.IsAny<InvoiceEntity>()), Times.Once);
            m_invoiceRepositoryMock.Verify(repo => repo.SaveAsync(), Times.Once);
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
            var invoiceEntity = new InvoiceEntity { Id = id, Amount = 100.0, PaidAmount = 0, Status = InvoicePaymentEnum.Pending.ToString() };

            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(id)).ReturnsAsync(invoiceEntity);
            m_invoiceRepositoryMock.Setup(repo => repo.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await m_invoiceService.PayInvoiceAsync(id, amount);

            // Assert
            m_invoiceRepositoryMock.Verify(repo => repo.UpdateInvoiceAsync(It.Is<InvoiceEntity>(i => i.Status == InvoicePaymentEnum.Paid.ToString())), Times.Once);
            m_invoiceRepositoryMock.Verify(repo => repo.SaveAsync(), Times.Once);
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
            var overdueInvoices = new List<InvoiceEntity>
                {
                    new InvoiceEntity { Id = 1, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now.AddDays(-overdueDays), Status = InvoicePaymentEnum.Pending.ToString() },
                    new InvoiceEntity { Id = 2, Amount = 200, PaidAmount = 0, DueDate = DateTime.Now.AddDays(-overdueDays), Status = InvoicePaymentEnum.Pending.ToString() }
                };

            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ReturnsAsync(overdueInvoices);
            m_invoiceRepositoryMock.Setup(repo => repo.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await m_invoiceService.ProcessOverdueInvoicesAsync(lateFee, overdueDays);

            // Assert
            m_invoiceRepositoryMock.Verify(repo => repo.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>()), Times.Exactly(overdueInvoices.Count));
        }

        [Fact]
        public async Task ProcessOverdueInvoicesAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var overdueDays = 30;
            var lateFee = 10.0;
            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.ProcessOverdueInvoicesAsync(lateFee, overdueDays));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ProcessOverdueInvoicesAsync_ShouldHandlePartialPayment()
        {
            // Arrange
            var overdueDays = 30;
            var lateFee = 10.0;
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, PaidAmount = 50, DueDate = DateTime.Now.AddDays(-overdueDays), Status = InvoicePaymentEnum.Pending.ToString() };

            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ReturnsAsync(new List<InvoiceEntity> { invoiceEntity });
            m_invoiceRepositoryMock.Setup(repo => repo.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await m_invoiceService.ProcessOverdueInvoicesAsync(lateFee, overdueDays);

            // Assert
            m_invoiceRepositoryMock.Verify(repo => repo.UpdateInvoiceAsync(It.Is<InvoiceEntity>(i => i.Status == InvoicePaymentEnum.Paid.ToString())), Times.Once);
        }

        [Fact]
        public async Task ProcessOverdueInvoicesAsync_ShouldHandleNoPayment()
        {
            // Arrange
            var overdueDays = 30;
            var lateFee = 10.0;
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now.AddDays(-overdueDays), Status = InvoicePaymentEnum.Pending.ToString() };

            m_invoiceRepositoryMock.Setup(repo => repo.GetAllInvoicesAsync()).ReturnsAsync(new List<InvoiceEntity> { invoiceEntity });
            m_invoiceRepositoryMock.Setup(repo => repo.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await m_invoiceService.ProcessOverdueInvoicesAsync(lateFee, overdueDays);

            // Assert
            m_invoiceRepositoryMock.Verify(repo => repo.UpdateInvoiceAsync(It.Is<InvoiceEntity>(i => i.Status == InvoicePaymentEnum.Voided.ToString())), Times.Once);
        }

        [Fact]
        public async Task DeleteInvoiceAsync_ShouldDeleteInvoice()
        {
            // Arrange
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, DueDate = DateTime.Now.AddDays(30), Status = InvoicePaymentEnum.Pending.ToString() };

            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(It.IsAny<int>())).ReturnsAsync(invoiceEntity);
            m_invoiceRepositoryMock.Setup(repo => repo.DeleteInvoiceAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await m_invoiceService.DeleteInvoiceAsync(1);

            // Assert
            m_invoiceRepositoryMock.Verify(repo => repo.DeleteInvoiceAsync(It.IsAny<int>()), Times.Once);
            m_invoiceRepositoryMock.Verify(repo => repo.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteInvoiceAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var exception = new Exception("Database error");
            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(It.IsAny<int>())).ThrowsAsync(exception);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => m_invoiceService.DeleteInvoiceAsync(1));
            Assert.Equal("Database error", ex.Message);
        }

        [Fact]
        public async Task UpdateInvoiceAsync_ShouldUpdateInvoice()
        {
            // Arrange
            var invoiceDomain = new InvoiceDomain { Id = 1, Amount = 200, PaidAmount = 50, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() };
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, PaidAmount = 0, DueDate = DateTime.Now, Status = InvoicePaymentEnum.Pending.ToString() };

            m_invoiceRepositoryMock.Setup(repo => repo.GetInvoiceAsync(It.IsAny<int>())).ReturnsAsync(invoiceEntity);
            m_invoiceRepositoryMock.Setup(repo => repo.UpdateInvoiceAsync(It.IsAny<InvoiceEntity>())).Returns(Task.CompletedTask);
            m_invoiceRepositoryMock.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await  m_invoiceService.UpdateInvoiceAsync(invoiceDomain);

            // Assert
            m_invoiceRepositoryMock.Verify(repo => repo.UpdateInvoiceAsync(It.Is<InvoiceEntity>(i => i.Amount == 200 && i.PaidAmount == 50)), Times.Once);
        }
    }
}
