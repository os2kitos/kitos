using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Migrations.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DataAccess
{
    public class ItContractInvoiceDateMappingTest : WithAutoFixture
    {
        [Fact]
        public void InvoiceDates_PostgreSql_Model_Uses_Full_Date_And_Owned_Unique_Periods()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(ItContractInvoiceDate));
            Assert.NotNull(entity);
            Assert.Equal(new[] { nameof(ItContractInvoiceDate.ItContractId), nameof(ItContractInvoiceDate.Period) },
                entity.FindPrimaryKey()!.Properties.Select(x => x.Name));
            Assert.Equal("timestamp without time zone", entity.FindProperty(nameof(ItContractInvoiceDate.Date))!.GetColumnType());
            var foreignKey = Assert.Single(entity.GetForeignKeys());
            Assert.Equal(typeof(ItContract), foreignKey.PrincipalEntityType.ClrType);
            Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
            Assert.True(foreignKey.IsRequired);
            var overview = context.Model.FindEntityType(typeof(ItContractOverviewReadModel));
            Assert.NotNull(overview);
            Assert.Null(overview.FindNavigation("InvoiceDates"));
        }

        [Fact]
        public void InvoiceSchedule_Migration_Adds_Frequency_Columns_And_Date_Table()
        {
            var migration = new AddContractInvoiceSchedule { ActiveProvider = "Npgsql.EntityFrameworkCore.PostgreSQL" };
            var columns = migration.UpOperations.OfType<AddColumnOperation>().ToList();
            Assert.Equal(6, columns.Count);
            Assert.All(columns.Where(x => x.Name == "InvoiceFrequency"), x => Assert.True(x.IsNullable));
            Assert.Equal(4, columns.Count(x => x.Name.StartsWith("InvoiceDate", StringComparison.Ordinal)));
            Assert.All(columns.Where(x => x.Name.StartsWith("InvoiceDate", StringComparison.Ordinal)), x => Assert.True(x.IsNullable));
            var table = Assert.Single(migration.UpOperations.OfType<CreateTableOperation>());
            Assert.Equal("ItContractInvoiceDates", table.Name);
            Assert.Equal(new[] { "ItContractId", "Period" }, table.PrimaryKey!.Columns);
            Assert.Equal("timestamp without time zone", Assert.Single(table.Columns, x => x.Name == "Date").ColumnType);
            Assert.Equal(ReferentialAction.Cascade, Assert.Single(table.ForeignKeys).OnDelete);
            Assert.Equal(7, migration.UpOperations.Count);
            using var context = CreateContext();
            var commands = context.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations);
            var sql = string.Join(Environment.NewLine, commands.Select(x => x.CommandText));
            Assert.Contains("CREATE TABLE dbo.\"ItContractInvoiceDates\"", sql);
            Assert.Contains("\"Date\" timestamp without time zone NOT NULL", sql);
            Assert.Contains("ON DELETE CASCADE", sql);
            Assert.DoesNotContain("ALTER COLUMN", sql);
        }

        [Fact]
        public void Overview_Invoice_Periods_Are_Nullable_Timestamp_Columns_In_Invoice_Schedule_Migration()
        {
            using var context = CreateContext();
            var overview = context.Model.FindEntityType(typeof(ItContractOverviewReadModel))!;
            var migration = new AddContractInvoiceSchedule();
            var columns = migration.UpOperations.OfType<AddColumnOperation>().ToList();
            Assert.Equal(6, columns.Count);
            foreach (var period in Enumerable.Range(1, 4))
            {
                var name = $"InvoiceDate{period}";
                var property = overview.FindProperty(name)!;
                Assert.True(property.IsNullable);
                Assert.Equal("timestamp without time zone", property.GetColumnType());
                var column = Assert.Single(columns, x => x.Name == name);
                Assert.True(column.IsNullable);
                Assert.Equal("timestamp without time zone", column.ColumnType);
            }
            Assert.Equal(6, migration.DownOperations.OfType<DropColumnOperation>().Count());
            Assert.Single(migration.DownOperations.OfType<DropTableOperation>());
            Assert.DoesNotContain(migration.UpOperations, x => x is AlterColumnOperation || x is DropColumnOperation);
            Assert.DoesNotContain(migration.UpOperations, x => x is SqlOperation);

            var generator = context.GetService<IMigrationsSqlGenerator>();
            Assert.NotEmpty(generator.Generate(migration.UpOperations));
            Assert.NotEmpty(generator.Generate(migration.DownOperations));
        }

        private static KitosContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<KitosContext>()
                .UseNpgsql("Host=localhost;Database=kitos;Username=postgres");
            return new KitosContext(options.Options);
        }
    }
}
