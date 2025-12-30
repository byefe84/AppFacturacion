using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Models
{
    public class QuoteLine
    {
        public long Id { get; set; }
        public required Quote Quote { get; set; }
        public required Material Material { get; set; }
        public required double Quantity { get; set; }
        public double UnitPrice => Material.UnitPrice;
        public double DiscountAmount { get; set; }
        public double GrossAmount => Quantity * UnitPrice;
        public double Amount => Math.Round(Quantity * UnitPrice - DiscountAmount, 2, MidpointRounding.AwayFromZero);
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
