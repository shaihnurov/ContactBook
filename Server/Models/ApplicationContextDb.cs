using ContactBook.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Models;

public class ApplicationContextDb : DbContext
{
    public DbSet<ContactListModel> ContactLists => Set<ContactListModel>();
    
    public ApplicationContextDb(DbContextOptions<ApplicationContextDb> options) : base(options) { }
}