namespace Bymyslf.AuditDbContext.Test
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            AuditDbContext.RegisterAuditableEntity(typeof(Customer));

            using (var dbContext = new TestDbContext("AuditDbContext"))
            {
                var customer = new Customer()
                {
                    Name = "TestCustomer"
                };

                dbContext.Customers.Add(customer);
                dbContext.SaveChanges();

                var customerInDb = dbContext.Customers.FirstOrDefault(ct => ct.Name == customer.Name);

                customerInDb.Name = "TestCustomerUpdate";

                dbContext.Customers.Attach(customerInDb);
                dbContext.Entry(customerInDb).State = EntityState.Modified;

                dbContext.SaveChanges();
            }

            Console.ReadKey();
        }
    }
}