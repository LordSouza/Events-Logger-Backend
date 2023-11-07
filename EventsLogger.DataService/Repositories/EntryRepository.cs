using EventsLogger.DataService.Data;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using Microsoft.Extensions.Logging;

namespace EventsLogger.DataService.Repositories;

public class EntryRepository : Repository<Entry>, IEntryRepository
{
    public EntryRepository(AppDbContext context, ILogger logger) : base(context, logger)
    { }
}
