using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;
using ContactPerson = Core.DomainModel.ContactPerson;
using Organization = Core.DomainModel.Organization.Organization;


namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class OrganizationMapper: IOrganizationMapper
    {
        public OrganizationResponseDTO ToOrganizationDTO(Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr(), MapOrganizationType(organization));
        }

        public OrganizationMasterDataRolesResponseDTO ToRolesDTO(OrganizationMasterDataRoles roles)
        {
            var contactPersonDto = ToContactPersonDTO(roles.ContactPerson);
            var dataResponsibleDto = ToDataResponsibleDTO(roles.DataResponsible);
            var dataProtectionAdvisor = ToDataProtectionAdvisorDTO(roles.DataProtectionAdvisor);
            return new OrganizationMasterDataRolesResponseDTO()
            {
                OrganizationUuid = roles.OrganizationUuid,
                ContactPerson = contactPersonDto,
                DataResponsible = dataResponsibleDto,
                DataProtectionAdvisor = dataProtectionAdvisor
            };
        }

        private static OrganizationType MapOrganizationType(Organization organization)
        {
            return organization.Type.Id switch
            {
                (int)OrganizationTypeKeys.Virksomhed => OrganizationType.Company,
                (int)OrganizationTypeKeys.Kommune => OrganizationType.Municipality,
                (int)OrganizationTypeKeys.AndenOffentligMyndighed => OrganizationType.OtherPublicAuthority,
                (int)OrganizationTypeKeys.Interessefællesskab => OrganizationType.CommunityOfInterest,
                _ => throw new ArgumentOutOfRangeException(nameof(organization.Type.Id), "Unknown organization type key")
            };
        }

        private static ContactPersonResponseDTO ToContactPersonDTO(ContactPerson contactPerson)
        {
            return new()
            {
                Email = contactPerson.Email,
                Id = contactPerson.Id,
                LastName = contactPerson.LastName,
                Name = contactPerson.Name,
                PhoneNumber = contactPerson.PhoneNumber,
            };
        }

        private static DataResponsibleResponseDTO ToDataResponsibleDTO(DataResponsible dataResponsible)
        {
            return new()
            {
                Email = dataResponsible.Email,
                Id = dataResponsible.Id,
                Cvr = dataResponsible.Cvr,
                Name = dataResponsible.Name,
                Address = dataResponsible.Adress,
                Phone = dataResponsible.Phone,
            };
        }

        private static DataProtectionAdvisorResponseDTO ToDataProtectionAdvisorDTO(DataProtectionAdvisor dataProtectionAdvisor)
        {
            return new()
            {
                Email = dataProtectionAdvisor.Email,
                Id = dataProtectionAdvisor.Id,
                Cvr = dataProtectionAdvisor.Cvr,
                Name = dataProtectionAdvisor.Name,
                Address = dataProtectionAdvisor.Adress,
                Phone = dataProtectionAdvisor.Phone,
            };
        }

        public OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto)
        {
            return new()
            {
                Cvr = OptionalValueChange<string>.With(dto.Cvr),
                Email = OptionalValueChange<string>.With(dto.Email),
                Address = OptionalValueChange<string>.With(dto.Address),
                Phone = OptionalValueChange<string>.With(dto.Phone),
            };
        }

        public OrganizationMasterDataRolesUpdateParameters ToMasterDataRolesUpdateParameters(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto)
        {
            var contactPersonDto = dto.ContactPerson;
            var contactPersonParameters = ToContactPersonUpdateParameters(contactPersonDto);

            var dataResponsibleDto = dto.DataResponsible;
            var dataResponsibleParameters = ToDataResponsibleUpdateParameters(dataResponsibleDto);

            var dataProtectionAdvisorDto = dto.DataProtectionAdvisor;
            var dataProtectionAdvisorParameters = ToDataProtectionAdvisorUpdateParameters(dataProtectionAdvisorDto);

            return new()
            {
                OrganizationUuid = organizationUuid,
                ContactPerson = contactPersonParameters,
                DataResponsible = dataResponsibleParameters,
                DataProtectionAdvisor = dataProtectionAdvisorParameters
            };
        }

        private static Maybe<ContactPersonUpdateParameters> ToContactPersonUpdateParameters(ContactPersonRequestDTO dto)
        {
            return dto != null
            ? new ContactPersonUpdateParameters()
            {
                Email = OptionalValueChange<string>.With(dto.Email),
                LastName = OptionalValueChange<string>.With(dto.LastName),
                Name = OptionalValueChange<string>.With(dto.Name),
                PhoneNumber = OptionalValueChange<string>.With(dto.PhoneNumber)
            }
            : Maybe<ContactPersonUpdateParameters>.None;
        }

        private static Maybe<DataResponsibleUpdateParameters> ToDataResponsibleUpdateParameters(DataResponsibleRequestDTO dto)
        {
            return dto != null
                ? new DataResponsibleUpdateParameters()
                {
                    Email = OptionalValueChange<string>.With(dto.Email),
                    Cvr = OptionalValueChange<string>.With(dto.Cvr),
                    Name = OptionalValueChange<string>.With(dto.Name),
                    Phone = OptionalValueChange<string>.With(dto.Phone),
                    Address = OptionalValueChange<string>.With(dto.Address)
                }
                : Maybe<DataResponsibleUpdateParameters>.None;
        }

        private static Maybe<DataProtectionAdvisorUpdateParameters> ToDataProtectionAdvisorUpdateParameters(DataProtectionAdvisorRequestDTO dto)
        {
            return dto != null
                ? new DataProtectionAdvisorUpdateParameters()
                {
                    Email = OptionalValueChange<string>.With(dto.Email),
                    Cvr = OptionalValueChange<string>.With(dto.Cvr),
                    Name = OptionalValueChange<string>.With(dto.Name),
                    Phone = OptionalValueChange<string>.With(dto.Phone),
                    Address = OptionalValueChange<string>.With(dto.Address)
                }
                : Maybe<DataProtectionAdvisorUpdateParameters>.None;
        }
    }
}