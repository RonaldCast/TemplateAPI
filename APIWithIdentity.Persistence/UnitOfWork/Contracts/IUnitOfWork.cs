
using System;
using System.Threading.Tasks;
using APIWithIdentity.Persistence.Repository.Contracts;

namespace APIWithIdentity.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository Contract
        IArtistRepository Artists{ get;  }
        IMusicRepository Musics{ get;  }

        // Method for created 
        void CreateTransaction();
        void Commit();
        void Rollback();
        int Save();
        Task<int> SaveAsync();
        Task CommitAsync();
        Task RollbackAsync();
        
    }
}