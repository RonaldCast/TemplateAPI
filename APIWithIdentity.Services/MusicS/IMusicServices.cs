using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;

namespace APIWithIdentity.Services
{
    public interface IMusicServices
    {
        Task<IEnumerable<Music>> GetAllMusics();
        Task<Music> GetMusicById(int id);
        Task<Music> CreateMusic(Music newMusic);
        Task UpdateMusic(Music musicToBeUpdated, Music music);
        Task DeleteMusic(Music music);
    }


}