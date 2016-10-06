namespace Bymyslf.AuditDbContext.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var dbContext = new TestDbContext(""))
            {
            }
        }
    }
}