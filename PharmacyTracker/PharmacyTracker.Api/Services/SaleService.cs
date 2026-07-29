using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Models.Dtos;
using PharmacyTracker.Api.Repositories;

namespace PharmacyTracker.Api.Services
{
    public class SaleService : ISaleService
    {
        private readonly IJsonRepository<Medicine> _medicineRepository;
        private readonly IJsonRepository<SaleRecord> _saleRepository;
        private static readonly SemaphoreSlim _saleLock = new SemaphoreSlim(1, 1);

        public SaleService(
            IJsonRepository<Medicine> medicineRepository,
            IJsonRepository<SaleRecord> saleRepository)
        {
            _medicineRepository = medicineRepository;
            _saleRepository = saleRepository;
        }

        public async Task<SaleRecord> RecordSaleAsync(CreateSaleDto dto)
        {
            await _saleLock.WaitAsync();
            try
            {
                var medicine = await _medicineRepository.GetByIdAsync(m => m.Id == dto.MedicineId);
                if (medicine == null)
                {
                    throw new KeyNotFoundException($"Medicine with ID {dto.MedicineId} was not found.");
                }

                if (medicine.Quantity < dto.QuantitySold)
                {
                    throw new InvalidOperationException($"Insufficient stock. Requested: {dto.QuantitySold}, Available: {medicine.Quantity}.");
                }

                // Decrement stock
                await _medicineRepository.UpdateAsync(
                    m => m.Id == dto.MedicineId,
                    m => m.Quantity -= dto.QuantitySold
                );

                // Create sale record
                var saleRecord = new SaleRecord
                {
                    Id = Guid.NewGuid(),
                    MedicineId = medicine.Id,
                    MedicineName = medicine.FullName,
                    QuantitySold = dto.QuantitySold,
                    UnitPriceAtSale = medicine.Price,
                    TotalAmount = dto.QuantitySold * medicine.Price,
                    SaleDate = DateTime.UtcNow
                };

                await _saleRepository.AddAsync(saleRecord);

                return saleRecord;
            }
            finally
            {
                _saleLock.Release();
            }
        }

        public async Task<List<SaleRecord>> GetSalesHistoryAsync()
        {
            var sales = await _saleRepository.GetAllAsync();
            return sales.OrderByDescending(s => s.SaleDate).ToList();
        }
    }
}
