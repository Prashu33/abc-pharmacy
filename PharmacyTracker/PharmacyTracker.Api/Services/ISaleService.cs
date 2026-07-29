using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Models.Dtos;

namespace PharmacyTracker.Api.Services
{
    public interface ISaleService
    {
        Task<SaleRecord> RecordSaleAsync(CreateSaleDto dto);
        Task<List<SaleRecord>> GetSalesHistoryAsync();
    }
}
