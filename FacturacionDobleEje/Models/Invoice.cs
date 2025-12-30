namespace FacturacionDobleEje.Models
{
    public class Invoice
    {
        public required long Id;
        public string Reference { get; set; } = $"P-{DateTime.Now:yyyyMMddHHmmss}";
        public DateTime Date { get; set; } = DateTime.Now;
        public required Client Client { get; set; }
        public required Quote Quote { get; set; }
        public required List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public decimal VatType { get; set; } = 0.21m;

        public decimal Subtotal => Lines.Sum(l => l.Amount);
        public decimal Vat => Math.Round(Subtotal * VatType, 2);
        public decimal Total => Subtotal + Vat;
        public required string Status {  get; set; }
        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
