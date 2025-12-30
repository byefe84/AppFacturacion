namespace FacturacionDobleEje.Models
{
    public class Client
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string DocType { get; set; }
        public required string Document { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
