using System;
using System.Linq;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Migrations.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DataAccess
{
    public class StsOrganizationIdentityMappingTest : WithAutoFixture
    {
        [Fact]
        public void GetByExternalUuid_WithPostgreSql_QueriesOrganizationId()
        {
            using var context = CreateContext();
            var externalUuid = A<Guid>();
            var query = context.SsoOrganizationIdentities
                .Where(identity => identity.ExternalUuid == externalUuid)
                .Take(1)
                .ToQueryString();

            Assert.Contains("\"OrganizationId\"", query);
            Assert.DoesNotContain("Organization_Id", query);

            var entityType = context.Model.FindEntityType(typeof(StsOrganizationIdentity));
            Assert.NotNull(entityType);
            var foreignKey = Assert.Single(entityType.GetForeignKeys());
            var property = Assert.Single(foreignKey.Properties);
            var tableName = entityType.GetTableName();
            Assert.NotNull(tableName);
            var table = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());

            Assert.Equal("OrganizationId", property.GetColumnName(table));
            Assert.False(property.IsNullable);
            Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
            Assert.Equal(typeof(Organization), foreignKey.PrincipalEntityType.ClrType);
        }

        [Fact]
        public void InitialBaseline_WithPostgreSql_CreatesOrganizationIdForeignKey()
        {
            var migration = new InitialBaseline { ActiveProvider = "Npgsql.EntityFrameworkCore.PostgreSQL" };
            var table = Assert.Single(migration.UpOperations.OfType<CreateTableOperation>(),
                operation => operation.Name == "StsOrganizationIdentities");
            var column = Assert.Single(table.Columns, operation => operation.Name == "OrganizationId");
            var foreignKey = Assert.Single(table.ForeignKeys);
            var index = Assert.Single(migration.UpOperations.OfType<CreateIndexOperation>(),
                operation => operation.Table == table.Name && operation.Columns.Contains("OrganizationId"));

            Assert.False(column.IsNullable);
            Assert.DoesNotContain(table.Columns, operation => operation.Name == "Organization_Id");
            Assert.Equal("OrganizationId", Assert.Single(foreignKey.Columns));
            Assert.Equal("Organization", foreignKey.PrincipalTable);
            Assert.Equal(Microsoft.EntityFrameworkCore.Migrations.ReferentialAction.Cascade, foreignKey.OnDelete);
            Assert.Equal("OrganizationId", Assert.Single(index.Columns));
        }

        private static KitosContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<KitosContext>();
            options.UseNpgsql("Host=localhost;Database=kitos;Username=postgres");

            return new KitosContext(options.Options);
        }
    }
}
