using EventsLogger.DataService.Data;
using EventsLogger.DataService.Repositories;
using EventsLogger.DataService.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace FormulaOne.DataService.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly AppDbContext _context;

        public IUserProjectRepository UsersProjects { get; set; }
        public IAddressRepository Addresses { get; set; }
        public IProjectRepository Projects { get; set; }
        public IEntryRepository Entries { get; set; }
        public IUserRepository Users { get; set; }


        public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            var logger = loggerFactory.CreateLogger("logs");

            Users = new UserRepository(_context, logger);
            Projects = new ProjectRepository(_context, logger);
            Entries = new EntryRepository(_context, logger);
            Addresses = new AddressRepository(_context, logger);
            UsersProjects = new UserProjectRepository(_context, logger);

        }

        public async Task<bool> CompleteAsync()
        {
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // public void Dispose()
        // {
        //     _context.Dispose();
        // }

    }





}