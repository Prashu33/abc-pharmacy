using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyTracker.Api.Models.Dtos;

namespace PharmacyTracker.Api.Services
{
    public interface IMedicineService
    {
        Task<List<MedicineListDto>> GetMedicinesAsync(string? search = null);
        Task<MedicineDetailDto?> GetMedicineByIdAsync(Guid id);
        Task<MedicineDetailDto> AddMedicineAsync(CreateMedicineDto dto);
    }
}
