using System;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel;
using APIWithIdentity.Persistence.Repository;
using APIWithIdentity.Persistence.Repository.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace APIWithIdentity.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDbContext _dbContext;
        private IDbContextTransaction _dbTransaction;

        private ArtistRepository _artistRepository;
        private MusicRepository _musicRepository;
        private RefreshTokenRepository _refreshTokens;

        public UnitOfWork(AppDbContext dbContext)
        {
         
            _dbContext = dbContext;
        }
        
        public void Commit()
        {
            _dbTransaction.Commit();
        }

        public IArtistRepository Artists => _artistRepository ??= new ArtistRepository(_dbContext) ;

        public IMusicRepository Musics =>  _musicRepository ??= new MusicRepository(_dbContext);

        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_dbContext) ;
        public void CreateTransaction()
        {
            _dbTransaction = _dbContext.Database.BeginTransaction();
        }

        public void Rollback()
        {
            _dbTransaction.Rollback();
        }

        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public void DisposeAsync()
        {
            _dbContext.DisposeAsync();
        }

        public async Task CommitAsync()
        {
            await _dbTransaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _dbTransaction.RollbackAsync();
        }
    }
}