using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;

namespace APIWithIdentity.Services
{
    public interface IArtistServices
    {
        Task<IEnumerable<Artist>> GetAllArtists();
        Task<Artist> GetArtistById(int id);
        Task<Artist> CreateArtist(Artist newArtist);
        Task UpdateArtist(Artist artistToBeUpdated, Artist artist);
        Task DeleteArtist(Artist artist);

        Task<List<Artist>> GetArtistsByNameAsync(string name);
    }
}