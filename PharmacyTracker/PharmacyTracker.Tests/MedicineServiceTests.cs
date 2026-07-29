using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Repositories;
using PharmacyTracker.Api.Services;
using Xunit;

namespace PharmacyTracker.Tests
{
    public class MedicineServiceTests
    {
        [Fact]
        public async Task GetMedicinesAsync_ShouldComputeNearExpiryCorrectly()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), FullName = "Expiring Soon", ExpiryDate = today.AddDays(15), Quantity = 20 },
                new Medicine { Id = Guid.NewGuid(), FullName = "Safe Medicine", ExpiryDate = today.AddDays(45), Quantity = 20 }
            };

            var mockRepo = new Mock<IJsonRepository<Medicine>>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines);

            var service = new MedicineService(mockRepo.Object);

            // Act
            var result = await service.GetMedicinesAsync();

            // Assert
            var expiring = result.First(m => m.FullName == "Expiring Soon");
            var safe = result.First(m => m.FullName == "Safe Medicine");

            Assert.True(expiring.IsNearExpiry);
            Assert.False(safe.IsNearExpiry);
        }

        [Fact]
        public async Task GetMedicinesAsync_ShouldComputeLowStockCorrectly()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), FullName = "Low Stock", ExpiryDate = today.AddDays(50), Quantity = 5 },
                new Medicine { Id = Guid.NewGuid(), FullName = "Good Stock", ExpiryDate = today.AddDays(50), Quantity = 15 }
            };

            var mockRepo = new Mock<IJsonRepository<Medicine>>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines);

            var service = new MedicineService(mockRepo.Object);

            // Act
            var result = await service.GetMedicinesAsync();

            // Assert
            var low = result.First(m => m.FullName == "Low Stock");
            var good = result.First(m => m.FullName == "Good Stock");

            Assert.True(low.IsLowStock);
            Assert.False(good.IsLowStock);
        }

        [Fact]
        public async Task GetMedicinesAsync_ShouldFilterBySearchTerm()
        {
            // Arrange
            var medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), FullName = "Paracetamol 650mg", Brand = "Sun" },
                new Medicine { Id = Guid.NewGuid(), FullName = "Amoxicillin 500mg", Brand = "Cipla" }
            };

            var mockRepo = new Mock<IJsonRepository<Medicine>>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines);

            var service = new MedicineService(mockRepo.Object);

            // Act
            var result = await service.GetMedicinesAsync("para");

            // Assert
            Assert.Single(result);
            Assert.Equal("Paracetamol 650mg", result.First().FullName);
        }
    }
}
