using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.Persistence;

namespace APIWithIdentity.Services
{
    public class ArtistServices : IArtistServices
    {
        private readonly IUnitOfWork _uow;

        public ArtistServices(IUnitOfWork uow)
        {
            _uow = uow;
        }
            
        public async Task<IEnumerable<Artist>> GetAllArtists()
        {
            return await _uow.Artists.GetAllWithMusicsAsync();
        }

        public async Task<Artist> GetArtistById(int id)
        {
            return await _uow.Artists.GetByIdAsync(id);
        }

        public async Task<Artist> CreateArtist(Artist newArtist)
        {
            await _uow.Artists
                .AddAsync(newArtist);
            
            await _uow.SaveAsync();

            return newArtist;
        }

        public async Task UpdateArtist(Artist artistToBeUpdated, Artist artist)
        {
            artistToBeUpdated.Name = artist.Name;

            await _uow.CommitAsync();
        }

        public async Task DeleteArtist(Artist artist)
        {
            _uow.Artists.Remove(artist);

            await _uow.CommitAsync();
        }

       public async Task<List<Artist>> GetArtistsByNameAsync(string name)
       {
           return  await _uow.Artists.GetArtistsByNameAsync(name);
           
       }
    }
}