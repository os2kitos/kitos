/* MySqlHistoryContext.cs
 * 
 * From http://www.asp.net/mvc/tutorials/security/aspnet-identity-using-mysql-storage-with-an-entityframework-mysql-provider
 * 
 * "Entity Framework Code First uses a MigrationHistory table to keep track of model changes 
 * and to ensure the consistency between the database schema and conceptual schema. 
 * However, this table does not work for MySQL by default because the primary key is too large. 
 * To remedy this situation, you will need to shrink the key size for that table."
 * 
 * E.g if we don't do this, we won't be able to create the migrations table in MySQL, because
 * the key is too large.
 */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess
{
    public class MySqlHistoryContext : HistoryContext
    {
        public MySqlHistoryContext(
          DbConnection existingConnection,
          string defaultSchema)
            : base(existingConnection, defaultSchema)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasMaxLength(200).IsRequired();
        }
    }
}
