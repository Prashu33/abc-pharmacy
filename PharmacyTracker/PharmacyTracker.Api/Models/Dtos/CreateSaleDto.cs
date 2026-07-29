using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyTracker.Api.Models.Dtos
{
    public class CreateSaleDto
    {
        [Required(ErrorMessage = "MedicineId is required.")]
        public Guid MedicineId { get; set; }

        [Range(1, 10000, ErrorMessage = "Quantity sold must be at least 1.")]
        public int QuantitySold { get; set; }
    }
}
