using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Models.Dtos;
using PharmacyTracker.Api.Services;

namespace PharmacyTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpPost]
        public async Task<ActionResult<SaleRecord>> CreateSale([FromBody] CreateSaleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var record = await _saleService.RecordSaleAsync(dto);
                return CreatedAtAction(nameof(GetSalesHistory), null, record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<SaleRecord>>> GetSalesHistory()
        {
            var history = await _saleService.GetSalesHistoryAsync();
            return Ok(history);
        }
    }
}
