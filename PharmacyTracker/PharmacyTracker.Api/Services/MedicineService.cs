using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Models.Dtos;
using PharmacyTracker.Api.Repositories;

namespace PharmacyTracker.Api.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IJsonRepository<Medicine> _medicineRepository;

        public MedicineService(IJsonRepository<Medicine> medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<List<MedicineListDto>> GetMedicinesAsync(string? search = null)
        {
            var medicines = await _medicineRepository.GetAllAsync();
            var today = DateTime.UtcNow.Date;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLowerInvariant();
                medicines = medicines.Where(m => 
                    m.FullName.ToLowerInvariant().Contains(lowerSearch) || 
                    m.Brand.ToLowerInvariant().Contains(lowerSearch)
                ).ToList();
            }

            return medicines.Select(m => new MedicineListDto
            {
                Id = m.Id,
                FullName = m.FullName,
                ExpiryDate = m.ExpiryDate,
                Quantity = m.Quantity,
                Price = m.Price,
                Brand = m.Brand,
                IsNearExpiry = (m.ExpiryDate.Date - today).TotalDays < 30,
                IsLowStock = m.Quantity < 10
            }).ToList();
        }

        public async Task<MedicineDetailDto?> GetMedicineByIdAsync(Guid id)
        {
            var m = await _medicineRepository.GetByIdAsync(x => x.Id == id);
            if (m == null) return null;

            var today = DateTime.UtcNow.Date;
            return new MedicineDetailDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Notes = m.Notes,
                ExpiryDate = m.ExpiryDate,
                Quantity = m.Quantity,
                Price = m.Price,
                Brand = m.Brand,
                IsNearExpiry = (m.ExpiryDate.Date - today).TotalDays < 30,
                IsLowStock = m.Quantity < 10
            };
        }

        public async Task<MedicineDetailDto> AddMedicineAsync(CreateMedicineDto dto)
        {
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Notes = dto.Notes,
                ExpiryDate = dto.ExpiryDate,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Brand = dto.Brand
            };

            await _medicineRepository.AddAsync(medicine);

            var today = DateTime.UtcNow.Date;
            return new MedicineDetailDto
            {
                Id = medicine.Id,
                FullName = medicine.FullName,
                Notes = medicine.Notes,
                ExpiryDate = medicine.ExpiryDate,
                Quantity = medicine.Quantity,
                Price = medicine.Price,
                Brand = medicine.Brand,
                IsNearExpiry = (medicine.ExpiryDate.Date - today).TotalDays < 30,
                IsLowStock = medicine.Quantity < 10
            };
        }
    }
}
