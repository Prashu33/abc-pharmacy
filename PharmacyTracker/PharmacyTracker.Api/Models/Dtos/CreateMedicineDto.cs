using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyTracker.Api.Models.Dtos
{
    public class CreateMedicineDto
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(200, ErrorMessage = "Full Name cannot exceed 200 characters.")]
        public string FullName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry Date is required.")]
        public DateTime ExpiryDate { get; set; }

        [Range(0, 100000, ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public int Quantity { get; set; }

        [Range(0.01, 1000000.00, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Brand is required.")]
        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters.")]
        public string Brand { get; set; } = string.Empty;
    }
}
