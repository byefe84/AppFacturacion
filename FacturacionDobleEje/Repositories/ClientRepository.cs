using FacturacionDobleEje.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturacionDobleEje.Repositories
{
    public class ClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Client client)
        {
            _context.Clients.Add(client);
            _context.SaveChanges();
        }

        public IEnumerable<Client> GetAll()
        {
            return _context.Clients.OrderBy(c => c.Name).ToList();
        }
    }
}
