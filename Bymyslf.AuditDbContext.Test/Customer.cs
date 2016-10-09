namespace Bymyslf.AuditDbContext.Test
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}