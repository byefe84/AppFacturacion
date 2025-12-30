using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Models
{
    public class InvoiceLine
    {
        public required long Id { get; set; }
        public required Invoice Invoice { get; set; }
        public required decimal Quantity {  get; set; }
        public required decimal UnitPrice {  get; set; }
        public required decimal DiscountAmount { get; set; }
        public decimal GrossAmount => Quantity * UnitPrice;
        public decimal Amount => Math.Round(Quantity * UnitPrice - DiscountAmount, 2, MidpointRounding.AwayFromZero );
        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
