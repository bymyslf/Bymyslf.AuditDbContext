namespace Bymyslf.AuditDbContext.Test
{
    using System.Data.Entity;

    public class TestDbContext : AuditDbContext
    {
        public TestDbContext(string nameOrConnectionString)
          : base(nameOrConnectionString)
        {
            this.Configuration.ProxyCreationEnabled = true;
        }

        public DbSet<Customer> Customers { get; set; }
    }
}