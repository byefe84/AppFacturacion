namespace FacturacionDobleEje.Models
{
    public class Client
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Adress { get; set; }
        public required string NIF { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
    }
}
