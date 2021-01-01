using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.Persistence.Repository.Contracts;
using Microsoft.EntityFrameworkCore;

namespace APIWithIdentity.Persistence.Repository
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        public ArtistRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Artist>> GetAllWithMusicsAsync()
        {
            return await Context.Artists.Include(a => a.Musics)
              .ToListAsync();
        }

        public Task<Artist> GetWithMusicsByIdAsync(int id)
        {
            return Context.Artists.Include(a => a.Musics)
                .FirstOrDefaultAsync(a => a.Id == id);

        }

        public async Task<List<Artist>> GetArtistsByNameAsync(string name)
        {
            return await Context.Artists.Where(x => x.Name.Contains(name))
                .AsNoTracking().ToListAsync();
        }
    }
}