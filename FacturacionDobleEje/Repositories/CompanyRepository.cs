using FacturacionDobleEje.Models;

namespace FacturacionDobleEje.Repositories
{
    public class CompanyRepository
    {
        public List<Company> GetAll()
        {
            using var db = new AppDbContext();
            return db.Companies.OrderBy(m => m.Id).ToList();
        }
    }
}
