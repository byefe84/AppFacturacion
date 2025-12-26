using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Models
{
    public class InvoiceLine
    {
        public required Material Material { get; set; }
        public required decimal Quantity { get; set; }
        public decimal UnitPrice => Material.UnitPrice;
        public decimal DiscountAmount { get; set; }
        public decimal GrossAmount => Quantity * UnitPrice;
        public decimal Amount => Math.Round(Quantity * UnitPrice - DiscountAmount, 2, MidpointRounding.AwayFromZero );
    }
}
