using FacturacionDobleEje.Models;
using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Data
{
    public class MockRepository
    {
        public List<Material> Materials { get; } = new List<Material>();
        public List<Client> Clients { get; } = new List<Client>();
        public Company MyCompany { get; } = new Company { Id = 0, Name = "", Adress = "", NIF = "", PhoneNumber = "", Email = ""};

        public MockRepository()
        {
            // 15 materiales básicos
            Materials.Add(new Material { Id = 1, Name = "Trasdosado", UnitPrice = 3.50m, Unit = "m2" });
            Materials.Add(new Material { Id = 2, Name = "Vivos", UnitPrice = 3.50m, Unit = "m2" });
            Materials.Add(new Material { Id = 3, Name = "Fosa", UnitPrice = 3.50m, Unit = "ml" });
            Materials.Add(new Material { Id = 4, Name = "Tapetas", UnitPrice = 3.50m, Unit = "ml" });
            Materials.Add(new Material { Id = 5, Name = "Techo", UnitPrice = 4m, Unit = "m2" });
            Materials.Add(new Material { Id = 6, Name = "Horas administración", UnitPrice = 18m, Unit = "h" });

            // 2 clientes inventados
            Clients.Add(new Client
            {
                Id = 1,
                Name = "Aistatec Verdillo S.L.",
                Adress = "Lg. Moucho - Verdillo, s/n, 15100, Carballo",
                NIF = "B70489166",
                PhoneNumber = "",
                Email = ""
            });
            Clients.Add(new Client
            {
                Id = 1,
                Name = "Construcciones Ejemplo 2 S.L.",
                Adress = "C/ Falsa 123, 28000 Madrid",
                NIF = "B12345678",
                PhoneNumber = "600111222",
                Email = "info@construcciones-ejemplo.es"
            });

            //Datos de la empresa
            MyCompany.Id = 1;
            MyCompany.Name = "Bertin Fernandez Ferreiros";
            MyCompany.Adress = "Lg. Vilar de Uz, 20, 15107, Carballo";
            MyCompany.NIF = "";
            MyCompany.PhoneNumber = "600510120";
            MyCompany.Email = "bertinfernandez06@gmail.com";
            MyCompany.LogoPath = "D:\\Proyectos\\FacturacionDobleEje\\FacturacionDobleEje\\Logo\\Logo.png";
        }
    }
}
