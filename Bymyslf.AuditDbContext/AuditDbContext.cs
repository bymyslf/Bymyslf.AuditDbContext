namespace Bymyslf.AuditDbContext
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuditDbContext : DbContext
    {
        private static readonly HashSet<Type> auditableTypes = new HashSet<Type>();

        public AuditDbContext(string nameOrConnectionString)
          : base(nameOrConnectionString)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected virtual void Audit()
        {
            var modifiedEntities = ChangeTracker.Entries()
                                                .Where(p => (p.State == EntityState.Deleted || p.State == EntityState.Modified) && this.IsAuditableType(p))
                                                .ToList();

            var dateChanged = DateTime.UtcNow;

            foreach (var entity in modifiedEntities)
            {
                var entityName = entity.Entity.GetType().Name;
                var primaryKeyFieldAndValue = this.GetPrimaryKeyFieldAndValue(entity);

                foreach (var propertyName in entity.OriginalValues.PropertyNames)
                {
                    var prop = entity.Property(propertyName);
                    if (prop.IsModified)
                    {
                        var originalValue = (entity.OriginalValues[propertyName] ?? string.Empty).ToString();
                        var currentValue = (entity.CurrentValues[propertyName] ?? string.Empty).ToString();
                        if (originalValue != currentValue)
                        {
                            this.AuditLogs.Add(new AuditLog()
                            {
                                EntityName = entityName,
                                PrimaryKeyField = primaryKeyFieldAndValue.Item1.ToString(),
                                PrimaryKeyValue = primaryKeyFieldAndValue.Item2.ToString(),
                                PropertyName = propertyName,
                                Action = entity.State.ToString("G"),
                                OldValue = originalValue,
                                NewValue = currentValue,
                                DateChanged = dateChanged
                            });
                        }
                    }
                }
            }
        }

        public static void RegisterAuditableEntity(Type auditableType)
        {
            auditableTypes.Add(auditableType);
        }

        public override int SaveChanges()
        {
            this.Audit();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            this.Audit();
            return base.SaveChangesAsync();
        }

        //http://stackoverflow.com/questions/7255089/get-the-primary-key-value-of-an-arbitrary-entity-in-code-first/8083687#8083687
        private Tuple<EntityKey, object> GetPrimaryKeyFieldAndValue(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            return Tuple.Create(objectStateEntry.EntityKey, objectStateEntry.EntityKey.EntityKeyValues[0].Value);
        }

        private bool IsAuditableType(DbEntityEntry entry)
        {
            var type = ObjectContext.GetObjectType(entry.Entity.GetType());
            return auditableTypes.Contains(type);
        }
    }
}