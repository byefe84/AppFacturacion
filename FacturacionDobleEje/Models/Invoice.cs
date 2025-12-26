namespace FacturacionDobleEje.Models
{
    public class Invoice
    {
        public string Reference { get; set; } = $"P-{DateTime.Now:yyyyMMddHHmmss}";
        public DateTime Date { get; set; } = DateTime.Now;
        public required Client Client { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public decimal VATType { get; set; } = 0.21m;

        public decimal Subtotal => Lines.Sum(l => l.Amount);
        public decimal VAT => Math.Round(Subtotal * VATType, 2);
        public decimal Total => Subtotal + VAT;
    }
}
