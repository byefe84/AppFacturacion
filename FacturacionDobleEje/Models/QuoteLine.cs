using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Models
{
    public class QuoteLine
    {
        public long Id { get; set; }
        public required Quote Quote { get; set; }
        public required Material Material { get; set; }
        public required int Quantity { get; set; }
        public decimal UnitPrice => Material.UnitPrice;
        public decimal DiscountAmount { get; set; }
        public decimal GrossAmount => Quantity * UnitPrice;
        public decimal Amount => Math.Round(Quantity * UnitPrice - DiscountAmount, 2, MidpointRounding.AwayFromZero);
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
