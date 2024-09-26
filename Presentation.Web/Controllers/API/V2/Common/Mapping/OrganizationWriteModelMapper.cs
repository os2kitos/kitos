using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class OrganizationWriteModelMapper : WriteModelMapperBase, IOrganizationWriteModelMapper
{
    public OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto)
    {
        var rule = CreateChangeRule<OrganizationMasterDataRequestDTO>(false);

        return new()
        {
            Cvr = rule.MustUpdate(x => x.Cvr)
                ? (dto.Cvr.FromNullable() ?? Maybe<string>.None).AsChangedValue()
                : OptionalValueChange<Maybe<string>>.None,
            Email = rule.MustUpdate(x => x.Email)
                ? (dto.Email.FromNullable() ?? Maybe<string>.None).AsChangedValue()
                : OptionalValueChange<Maybe<string>>.None,
            Address = rule.MustUpdate(x => x.Address)
                ? (dto.Address.FromNullable() ?? Maybe<string>.None).AsChangedValue() 
                : OptionalValueChange<Maybe<string>>.None,
            Phone = rule.MustUpdate(x => x.Phone)
                ? (dto.Phone.FromNullable() ?? Maybe<string>.None).AsChangedValue() 
                : OptionalValueChange<Maybe<string>>.None,
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
            if (dto == null) return Maybe<ContactPersonUpdateParameters>.None;

            return new ContactPersonUpdateParameters()
            {
                Email = dto.Email != null 
                    ? OptionalValueChange<Maybe<string>>.With(dto.Email) 
                    : Maybe<string>.None.AsChangedValue(), 
                LastName = dto.LastName != null 
                    ? OptionalValueChange<Maybe<string>>.With(dto.LastName) 
                    : Maybe<string>.None.AsChangedValue(),
                Name = dto.Name != null 
                    ? OptionalValueChange<Maybe<string>>.With(dto.Name) 
                    : Maybe<string>.None.AsChangedValue(),
                PhoneNumber = dto.PhoneNumber != null 
                    ? OptionalValueChange<Maybe<string>>.With(dto.PhoneNumber) 
                    : Maybe<string>.None.AsChangedValue()
            };
        }

        private static Maybe<DataResponsibleUpdateParameters> ToDataResponsibleUpdateParameters(DataResponsibleRequestDTO dto)
        {
            if (dto == null) return Maybe<DataResponsibleUpdateParameters>.None;

            return new DataResponsibleUpdateParameters()
                {
                    Email = dto.Email != null
                        ? OptionalValueChange<Maybe<string>>.With(dto.Email)
                        : Maybe<string>.None.AsChangedValue(),
                    Cvr = dto.Cvr != null
                        ? OptionalValueChange<Maybe<string>>.With(dto.Cvr)
                        : Maybe<string>.None.AsChangedValue(),
                    Name = dto.Name != null
                        ? OptionalValueChange<Maybe<string>>.With(dto.Name)
                        : Maybe<string>.None.AsChangedValue(),
                    Phone = dto.Phone != null
                        ? OptionalValueChange<Maybe<string>>.With(dto.Phone)
                        : Maybe<string>.None.AsChangedValue(),
                    Address = dto.Address != null
                        ? OptionalValueChange<Maybe<string>>.With(dto.Address)
                        : Maybe<string>.None.AsChangedValue()
                };
        }

        private static Maybe<DataProtectionAdvisorUpdateParameters> ToDataProtectionAdvisorUpdateParameters(DataProtectionAdvisorRequestDTO dto)
        {
        if (dto == null) return Maybe<DataProtectionAdvisorUpdateParameters>.None;

        return new DataProtectionAdvisorUpdateParameters()
        {
            Email = dto.Email != null
                ? OptionalValueChange<Maybe<string>>.With(dto.Email)
                : Maybe<string>.None.AsChangedValue(),
            Cvr = dto.Cvr != null
                ? OptionalValueChange<Maybe<string>>.With(dto.Cvr)
                : Maybe<string>.None.AsChangedValue(),
            Name = dto.Name != null
                ? OptionalValueChange<Maybe<string>>.With(dto.Name)
                : Maybe<string>.None.AsChangedValue(),
            Phone = dto.Phone != null
                ? OptionalValueChange<Maybe<string>>.With(dto.Phone)
                : Maybe<string>.None.AsChangedValue(),
            Address = dto.Address != null
                ? OptionalValueChange<Maybe<string>>.With(dto.Address)
                : Maybe<string>.None.AsChangedValue()
        };
    }

    public OrganizationWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }
}
