using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionDobleEje.Models
{
    public class Quote
    {
        public long Id;
        public string Reference { get; set; } = $"P-{DateTime.Now:yyyyMMddHHmmss}";
        public DateTime Date { get; set; } = DateTime.Now;
        public required Client Client { get; set; }
        public List<QuoteLine> Lines { get; set; } = new List<QuoteLine>();
        public double VatType { get; set; } = 0.21;

        public double Subtotal => Lines.Sum(l => l.Amount);
        public double Vat => Math.Round(Subtotal * VatType, 2);
        public double Total => Subtotal + Vat;
        public required string Status { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
