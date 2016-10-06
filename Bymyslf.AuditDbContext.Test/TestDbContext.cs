namespace Bymyslf.AuditDbContext.Test
{
    public class TestDbContext : AuditDbContext
    {
        public TestDbContext(string nameOrConnectionString)
          : base(nameOrConnectionString)
        {
        }
    }
}