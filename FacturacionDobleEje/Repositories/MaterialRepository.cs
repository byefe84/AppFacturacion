using FacturacionDobleEje.Models;
using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Repositories
{
    public class MaterialRepository
    {
        private readonly AppDbContext _context;

        public MaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Material> GetAll()
        {
            return _context.Materials.OrderBy(m => m.Id).ToList();
        }

        public void Add(Material material)
        {
            _context.Materials.Add(material);
            _context.SaveChanges();
        }
    }
}
