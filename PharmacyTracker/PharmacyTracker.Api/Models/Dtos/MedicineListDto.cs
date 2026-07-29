using System;

namespace PharmacyTracker.Api.Models.Dtos
{
    public class MedicineListDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
        public bool IsNearExpiry { get; set; }
        public bool IsLowStock { get; set; }
    }
}
