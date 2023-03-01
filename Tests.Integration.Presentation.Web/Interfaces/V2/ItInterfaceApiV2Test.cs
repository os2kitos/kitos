﻿using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;

namespace Tests.Integration.Presentation.Web.Interfaces.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItInterfaceApiV2Test : BaseItInterfaceApiV2Test
    {
        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_If_AccessModifier_Is_Public()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public, A<string>()));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id).DisposeAsync();

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            CheckBaseDTOValues(system, itInterface, itInterfaceDTO);
        }

        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_If_Interface_In_Org_Where_User_Has_Any_Role()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), org.Id, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id).DisposeAsync();

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            CheckBaseDTOValues(system, itInterface, itInterfaceDTO);
        }

        [Fact]
        public async Task Cannot_Get_Interface_As_Stakeholder_If_AccessModifier_Is_Local()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id).DisposeAsync();

            //Act
            using var result = await InterfaceV2Helper.SendGetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_With_Correct_Data()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id).DisposeAsync();

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ById(itInterface.Id);

                dbInterface.Description = A<string>();
                dbInterface.ItInterfaceId = A<string>();
                dbInterface.Name = A<string>();
                dbInterface.Url = A<string>();
                dbInterface.Version = A<string>().Substring(0, ItInterface.MaxVersionLength); // Version has maxLength of 20
                dbInterface.Disabled = A<bool>();

                db.SaveChanges();
            });

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetStakeholderInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.NotNull(itInterfaceDTO);
            DatabaseAccess.MapFromEntitySet<ItInterface, bool>(x =>
            {
                var dbInterface = x.AsQueryable().ById(itInterface.Id);
                BaseItInterfaceResponseDTODBCheck(dbInterface, itInterfaceDTO);

                Assert.Equal(dbInterface.LastChanged, itInterfaceDTO.LastModified);
                Assert.Equal(dbInterface.LastChanged, itInterfaceDTO.LastModified);

                Assert.Equal(dbInterface.LastChangedByUser.Uuid, itInterfaceDTO.LastModifiedBy.Uuid);
                Assert.Equal(dbInterface.LastChangedByUser.GetFullName(), itInterfaceDTO.LastModifiedBy.Name);

                return true;
            });
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_Stakeholder()
        {
            //Arrange - Making sure there are at least 2 public interfaces
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface1.Id).DisposeAsync();
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface2.Id).DisposeAsync();

            //Act
            var result = await InterfaceV2Helper.GetStakeholderInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_ChangesSince_Filter()
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrg();
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface3 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));

            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, organization.Id, TestEnvironment.DefaultOrganizationId).DisposeAsync();

            //Change all in a specific order
            foreach (var interfaceDto in new[] { itInterface2, itInterface3, itInterface1 })
            {
                await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, interfaceDto.Id).DisposeAsync();
            }

            var interface3LastModified = DatabaseAccess.MapFromEntitySet<ItInterface, DateTime>(x => x.AsQueryable().ByUuid(itInterface3.Uuid).LastChanged.Transform(dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc)));

            //Act
            var dtos = (await InterfaceV2Helper.GetStakeholderInterfacesAsync(token, changedSinceGtEq: interface3LastModified, exposedBySystemUuid: system.Uuid, pageNumber: 0, pageSize: 10)).ToList();

            //Assert that the right interfaces are returned in the correct order
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { itInterface3.Uuid, itInterface1.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task Can_Get_Active_Interfaces_As_Stakeholder_By_Exposing_System()
        {
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system1.Id, itInterface1.Id).DisposeAsync();
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system2.Id, itInterface2.Id).DisposeAsync();

            //Act
            var result = await InterfaceV2Helper.GetStakeholderInterfacesAsync(token, pageSize, pageNumber, system1.Uuid);

            //Assert
            Assert.Single(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Get_Interfaces_As_Stakeholder_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            var (token, org) = await CreateStakeHolderUserInNewOrg();

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Local);
            var itInterface1 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            var itInterface2 = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface1.Id).DisposeAsync();
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface2.Id).DisposeAsync();

            // Disable second interface
            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ById(itInterface2.Id);
                dbInterface.Disabled = true;
                db.SaveChanges();
            });

            //Act
            var result = await InterfaceV2Helper.GetStakeholderInterfacesAsync(token, pageSize, pageNumber, system.Uuid, includeDeactivated: shouldIncludeDeactivated); // Limit by exposing system

            //Assert
            if (shouldIncludeDeactivated)
            {
                Assert.Equal(pageSize, result.Count());
                var interface1DTO = result.First(x => x.Name.Equals(itInterface1.Name));
                CheckBaseDTOValues(system, itInterface1, interface1DTO);
                Assert.False(interface1DTO.Deactivated);
                var interface2DTO = result.First(x => x.Name.Equals(itInterface2.Name));
                CheckBaseDTOValues(system, itInterface2, interface2DTO);
                Assert.True(interface2DTO.Deactivated);
            }
            else
            {
                var interfaceDTO = Assert.Single(result);
                Assert.False(interfaceDTO.Deactivated);
                CheckBaseDTOValues(system, itInterface1, interfaceDTO);
            }
        }
        protected async Task<(string token, OrganizationDTO createdOrganization)> CreateStakeHolderUserInNewOrg()
        {
            var org = await CreateOrganization();
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, org.Id, true, true);
            return (token, org);
        }
    }
}
