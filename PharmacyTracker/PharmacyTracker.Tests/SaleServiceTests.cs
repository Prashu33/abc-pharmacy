using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Models.Dtos;
using PharmacyTracker.Api.Repositories;
using PharmacyTracker.Api.Services;
using Xunit;

namespace PharmacyTracker.Tests
{
    public class SaleServiceTests
    {
        [Fact]
        public async Task RecordSaleAsync_ShouldDecrementStock_WhenStockIsAvailable()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            var medicine = new Medicine
            {
                Id = medicineId,
                FullName = "Aspirin",
                Quantity = 50,
                Price = 10.00m
            };

            var mockMedicineRepo = new Mock<IJsonRepository<Medicine>>();
            mockMedicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<Func<Medicine, bool>>()))
                .ReturnsAsync(medicine);

            var mockSaleRepo = new Mock<IJsonRepository<SaleRecord>>();

            var service = new SaleService(mockMedicineRepo.Object, mockSaleRepo.Object);
            var dto = new CreateSaleDto { MedicineId = medicineId, QuantitySold = 10 };

            // Act
            var sale = await service.RecordSaleAsync(dto);

            // Assert
            mockMedicineRepo.Verify(r => r.UpdateAsync(
                It.IsAny<Func<Medicine, bool>>(),
                It.IsAny<Action<Medicine>>()
            ), Times.Once);

            mockSaleRepo.Verify(r => r.AddAsync(It.IsAny<SaleRecord>()), Times.Once);

            Assert.Equal(100.00m, sale.TotalAmount);
            Assert.Equal(10, sale.QuantitySold);
        }

        [Fact]
        public async Task RecordSaleAsync_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            var medicine = new Medicine
            {
                Id = medicineId,
                FullName = "Aspirin",
                Quantity = 5,
                Price = 10.00m
            };

            var mockMedicineRepo = new Mock<IJsonRepository<Medicine>>();
            mockMedicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<Func<Medicine, bool>>()))
                .ReturnsAsync(medicine);

            var mockSaleRepo = new Mock<IJsonRepository<SaleRecord>>();

            var service = new SaleService(mockMedicineRepo.Object, mockSaleRepo.Object);
            var dto = new CreateSaleDto { MedicineId = medicineId, QuantitySold = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RecordSaleAsync(dto));
        }

        [Fact]
        public async Task RecordSaleAsync_ShouldThrowKeyNotFoundException_WhenMedicineDoesNotExist()
        {
            // Arrange
            var mockMedicineRepo = new Mock<IJsonRepository<Medicine>>();
            mockMedicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<Func<Medicine, bool>>()))
                .ReturnsAsync((Medicine?)null);

            var mockSaleRepo = new Mock<IJsonRepository<SaleRecord>>();

            var service = new SaleService(mockMedicineRepo.Object, mockSaleRepo.Object);
            var dto = new CreateSaleDto { MedicineId = Guid.NewGuid(), QuantitySold = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RecordSaleAsync(dto));
        }
    }
}
