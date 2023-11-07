using EventsLogger.DataService.Data;
using EventsLogger.DataService.Repositories.Interfaces;
using EventsLogger.Entities.DbSet;
using Microsoft.Extensions.Logging;

namespace EventsLogger.DataService.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context, ILogger logger) : base(context, logger)
    { }
}
