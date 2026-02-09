using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionDobleEje.Models
{
    public class Company
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string DocType { get; set; }
        public required string Document { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required string Account { get; set; }
        public string? LogoPath { get; set; }
        public string? WatermarkPath { get; set; }
        public required DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
