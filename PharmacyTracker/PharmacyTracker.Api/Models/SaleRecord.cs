using System;

namespace PharmacyTracker.Api.Models
{
    public class SaleRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal UnitPriceAtSale { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    }
}
