namespace FacturacionDobleEje.Models
{
    namespace FacturacionConstruccion.Models
    {
        public class Material
        {
            public long Id { get; set; }
            public string? Code { get; set; }
            public required string Name { get; set; }
            public string? Description { get; set; }
            public required string Unit { get; set; }
            public required decimal UnitPrice { get; set; }
            public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        }
    }
}
