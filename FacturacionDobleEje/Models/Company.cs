using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionDobleEje.Models
{
    public class Company
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Adress { get; set; }
        public required string NIF { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public string? LogoPath { get; set; }
    }
}
