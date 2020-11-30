using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.Persistence.Repository.Contracts;
using Microsoft.EntityFrameworkCore;

namespace APIWithIdentity.Persistence.Repository
{
    public class MusicRepository : Repository<Music>, IMusicRepository
    {
        public MusicRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Music>> GetAllWithArtistAsync()
        {
            return await Context.Musics
                .Include(m => m.Artist)
                .ToListAsync();
        }

        public async Task<Music> GetWithArtistByIdAsync(int id)
        {
            return await Context.Musics.Include(a => a.Artist)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Music>> GetAllWithArtistByArtistIdAsync(int artistId)
        {
            return await Context.Musics.Include(a => a.Artist)
                .Where(a => a.ArtistId == artistId).ToListAsync();
        }
    }
}