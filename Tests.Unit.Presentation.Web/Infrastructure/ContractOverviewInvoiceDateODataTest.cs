using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract.ReadModels;
using Core.DomainServices.Generic;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Moq;
using Presentation.Web.Controllers.API.V1.OData;
using Core.DomainModel.ItContract.Read;
using Microsoft.OData.Edm;
using Presentation.Web.Infrastructure.OData;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    public class ContractOverviewInvoiceDateODataTest
    {
        [Fact]
        public void InvoiceDates_Collection_Is_Not_Exposed_On_Overview_Read_Model()
        {
            var model = ODataModelConfig.GetEdmModel();
            var overview = (IEdmEntityType)model.FindDeclaredType(typeof(ItContractOverviewReadModel).FullName);
            Assert.Null(overview.FindProperty("InvoiceDates"));
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Period_Fields_Are_Nullable_DateTimes_And_OData_Queries_Execute_In_Database_Before_Paging(int period)
        {
            var model = ODataModelConfig.GetEdmModel();
            var overview = (IEdmEntityType)model.FindDeclaredType(typeof(ItContractOverviewReadModel).FullName);
            var property = overview.FindProperty($"InvoiceDate{period}");
            Assert.Equal(EdmPropertyKind.Structural, property.PropertyKind);
            Assert.True(property.Type.IsNullable);
            Assert.Equal(EdmPrimitiveTypeKind.DateTimeOffset, property.Type.PrimitiveKind());

            var request = new DefaultHttpContext().Request;
            request.QueryString = new QueryString($"?$filter=InvoiceDate{period} ne null&$orderby=InvoiceDate{period} asc&$skip=10&$top=5");
            var options = new ODataQueryOptions<ItContractOverviewReadModel>(
                new ODataQueryContext(model, typeof(ItContractOverviewReadModel), null), request);
            using var context = new KitosContext(new DbContextOptionsBuilder<KitosContext>()
                .UseNpgsql("Host=localhost;Database=kitos;Username=postgres").Options);
            var query = (IQueryable<ItContractOverviewReadModel>)options.ApplyTo(context.ItContractOverviewReadModels,
                new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False, EnableConstantParameterization = false });
            var sql = query.ToQueryString();

            Assert.Contains($"\"InvoiceDate{period}\"", sql);
            Assert.Contains($"\"InvoiceDate{period}\" IS NOT NULL", sql);
            Assert.Matches($"ORDER BY [^\\r\\n]*\\\"InvoiceDate{period}\\\"", sql);
            Assert.True(sql.IndexOf("WHERE", System.StringComparison.Ordinal) < sql.IndexOf("ORDER BY", System.StringComparison.Ordinal));
            Assert.True(sql.IndexOf("ORDER BY", System.StringComparison.Ordinal) < sql.IndexOf("LIMIT", System.StringComparison.Ordinal));
            Assert.Contains("OFFSET", sql);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Endpoint_Preserves_Queryable_For_OData(bool filterByUnit)
        {
            var query = new[] { new ItContractOverviewReadModel() }.AsQueryable();
            var service = new Mock<IItContractOverviewReadModelsService>();
            var result = Result<IQueryable<ItContractOverviewReadModel>, OperationError>.Success(query);
            service.Setup(x => x.GetByOrganizationId(1)).Returns(result);
            service.Setup(x => x.GetByOrganizationIdOrUnitIdInSubTree(1, 2)).Returns(result);
            var controller = new ItContractOverviewReadModelsController(service.Object, Mock.Of<IEntityIdentityResolver>());

            var response = Assert.IsType<OkObjectResult>(controller.GetByOrganizationId(1, filterByUnit ? 2 : null));

            Assert.Same(query, response.Value);
        }
    }
}
