namespace FacturacionDobleEje.Models
{
    namespace FacturacionConstruccion.Models
    {
        public class Material
        {
            public required int Id { get; set; }
            public required string Name { get; set; }
            public required decimal UnitPrice { get; set; }
            public required string Unit { get; set; }
        }
    }
}
