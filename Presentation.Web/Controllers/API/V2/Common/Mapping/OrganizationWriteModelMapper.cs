using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class OrganizationWriteModelMapper : IOrganizationWriteModelMapper
{
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
