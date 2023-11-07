namespace EventsLogger.DataService.Repositories.Interfaces;

public interface IUnitOfWork
{
    public IUserProjectRepository UsersProjects { get; set; }
    public IAddressRepository Addresses { get; set; }
    public IProjectRepository Projects { get; set; }
    public IEntryRepository Entries { get; set; }
    public IUserRepository Users { get; set; }
    Task<bool> CompleteAsync();

}
