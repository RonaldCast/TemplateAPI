using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.Persistence;

namespace APIWithIdentity.Services
{
    public class MusicServices : IMusicServices
    {
        
        private readonly IUnitOfWork _uow;
        public MusicServices(IUnitOfWork uow)
        {
            _uow = uow;
        }

        
        public async Task<IEnumerable<Music>> GetAllMusics()
        {
            return await _uow.Musics
                .GetAllWithArtistAsync();
        }

        public async Task<Music> GetMusicById(int id)
        {
            return await _uow.Musics
                .GetWithArtistByIdAsync(id);
        }

        public async Task<Music> CreateMusic(Music newMusic)
        {
            await _uow.Musics.AddAsync(newMusic);
            await _uow.CommitAsync();
            return newMusic;
        }

        public async Task UpdateMusic(Music musicToBeUpdated, Music music)
        {
            musicToBeUpdated.Name = music.Name;
            musicToBeUpdated.ArtistId = music.ArtistId;

            await _uow.CommitAsync();
        }

        public async Task DeleteMusic(Music music)
        {
            _uow.Musics.Remove(music);
            await _uow.CommitAsync();
        }
    }
}