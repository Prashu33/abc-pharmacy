using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmacyTracker.Api.Models.Dtos;
using PharmacyTracker.Api.Services;

namespace PharmacyTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MedicineListDto>>> GetMedicines([FromQuery] string? search)
        {
            var medicines = await _medicineService.GetMedicinesAsync(search);
            return Ok(medicines);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicineDetailDto>> GetMedicine(Guid id)
        {
            var medicine = await _medicineService.GetMedicineByIdAsync(id);
            if (medicine == null)
            {
                return NotFound(new { message = $"Medicine with ID {id} not found." });
            }
            return Ok(medicine);
        }

        [HttpPost]
        public async Task<ActionResult<MedicineDetailDto>> CreateMedicine([FromBody] CreateMedicineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _medicineService.AddMedicineAsync(dto);
            return CreatedAtAction(nameof(GetMedicine), new { id = created.Id }, created);
        }
    }
}
