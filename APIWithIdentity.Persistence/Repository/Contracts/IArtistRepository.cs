using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;

namespace APIWithIdentity.Persistence.Repository.Contracts
{
    public interface IArtistRepository : IRepository<Artist>
    {
        Task<IEnumerable<Artist>> GetAllWithMusicsAsync();
        Task<Artist> GetWithMusicsByIdAsync(int id);

        Task<List<Artist>> GetArtistsByNameAsync(string name);
    }
}