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

        public static bool AuditingEnabled { get; set; } = true;

        protected virtual void Audit()
        {
            if (!AuditingEnabled)
            {
                return;
            }

            var modifiedEntities = ChangeTracker.Entries()
                                                .Where(p => p.State == EntityState.Modified && this.IsAuditableType(p))
                                                .ToList();

            var dateChanged = DateTime.UtcNow;

            foreach (var entity in modifiedEntities)
            {
                var entityName = entity.Entity.GetType().FullName;
                var primaryKeyFieldAndValue = this.GetPrimaryKeyFieldAndValue(entity);

                foreach (var propertyName in entity.OriginalValues.PropertyNames)
                {
                    var prop = entity.Property(propertyName);
                    var originalValue = this.OriginalValue(entity, propertyName);
                    var currentValue = this.CurrentValue(prop);
                    if (originalValue != currentValue)
                    {
                        this.AuditLogs.Add(new AuditLog()
                        {
                            Id = Guid.NewGuid(),
                            EntityName = entityName,
                            PrimaryKeyField = primaryKeyFieldAndValue.Item1,
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
        private Tuple<string, object> GetPrimaryKeyFieldAndValue(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            return Tuple.Create(objectStateEntry.EntityKey.EntityKeyValues[0].Key, objectStateEntry.EntityKey.EntityKeyValues[0].Value);
        }

        private bool IsAuditableType(DbEntityEntry entry)
        {
            var type = ObjectContext.GetObjectType(entry.Entity.GetType());
            return auditableTypes.Contains(type);
        }

        private string CurrentValue(DbPropertyEntry prop)
        {
            return (prop.CurrentValue ?? string.Empty).ToString();
        }

        private string OriginalValue(DbEntityEntry entry, string propertyName)
        {
            return (entry.GetDatabaseValues().GetValue<object>(propertyName) ?? string.Empty).ToString();
        }
    }
}