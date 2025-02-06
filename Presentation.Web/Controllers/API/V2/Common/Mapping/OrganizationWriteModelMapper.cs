using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.UiCustomization;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class OrganizationWriteModelMapper : WriteModelMapperBase, IOrganizationWriteModelMapper
{
    private readonly IEntityIdentityResolver _identityResolver;

    public OrganizationWriteModelMapper(ICurrentHttpRequest currentHttpRequest,
        IEntityIdentityResolver identityResolver) : base(currentHttpRequest)
    {
        _identityResolver = identityResolver;
    }
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

    public OrganizationBaseParameters ToOrganizationUpdateParameters(OrganizationUpdateRequestDTO dto)
    {
        return MapParameters(dto, false);
    }

    public OrganizationBaseParameters ToOrganizationCreateParameters(OrganizationCreateRequestDTO dto)
    {
        return MapParameters(dto, true);
    }

    private OrganizationBaseParameters MapParameters(OrganizationBaseRequestDTO dto, bool enforceChanges)
    {
        var rule = CreateChangeRule<OrganizationBaseRequestDTO>(enforceChanges);

        var parameters = new OrganizationBaseParameters()
        {
            Cvr = rule.MustUpdate(x => x.Cvr) ? (dto.Cvr.FromNullable()).AsChangedValue()
                : OptionalValueChange<Maybe<string>>.None,
            Name = rule.MustUpdate(x => x.Name) ? (dto.Name.FromNullable()).AsChangedValue()
                : OptionalValueChange<Maybe<string>>.None,
            TypeId = rule.MustUpdate(x => x.Type) ? ((int)dto.Type).AsChangedValue() : OptionalValueChange<int>.None,
        };

        return WithForeignCountryCodeMapping(rule, dto, parameters);
    }

    private OrganizationBaseParameters WithForeignCountryCodeMapping(IPropertyUpdateRule<OrganizationBaseRequestDTO> rule, OrganizationBaseRequestDTO dto, OrganizationBaseParameters parameters)
    {
        if (dto is OrganizationUpdateRequestDTO updateDto)
        {
            parameters.ForeignCountryCodeUuid = updateDto.UpdateForeignCountryCode
                ? dto.ForeignCountryCodeUuid.AsChangedValue()
                : OptionalValueChange<Guid?>.None;
            return parameters;
        }

        parameters.ForeignCountryCodeUuid = rule.MustUpdate(x => x.ForeignCountryCodeUuid)
            ? dto.ForeignCountryCodeUuid.AsChangedValue()
            : OptionalValueChange<Guid?>.None;
        return parameters;
    }

    public Result<UIModuleCustomizationParameters, OperationError> ToUIModuleCustomizationParameters(Guid organizationUuid, string moduleName,
        UIModuleCustomizationRequestDTO dto)
    { 
        var orgDbIdMaybe = _identityResolver.ResolveDbId<Organization>(organizationUuid);
        if (!orgDbIdMaybe.HasValue) return new OperationError(OperationFailure.NotFound);

        var nodes = dto.Nodes.Select(ToCustomUINodeParameters);
        return new UIModuleCustomizationParameters(orgDbIdMaybe.Value, moduleName, nodes);
    }

    public UIRootConfigUpdateParameters ToUIRootConfigUpdateParameters(UIRootConfigUpdateRequestDTO dto)
    {
        var rule = CreateChangeRule<UIRootConfigUpdateRequestDTO>(false);

        return new()
        {
            ShowItSystemModule = rule.MustUpdate(x => x.ShowItSystemModule)
                ? (dto.ShowItSystemModule.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                : OptionalValueChange<Maybe<bool>>.None,
            ShowItContractModule = rule.MustUpdate(x => x.ShowItContractModule)
                ? (dto.ShowItContractModule.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                : OptionalValueChange<Maybe<bool>>.None,

            ShowDataProcessing = rule.MustUpdate(x => x.ShowDataProcessing)
                ? (dto.ShowDataProcessing.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                : OptionalValueChange<Maybe<bool>>.None,
        };
    }

    private CustomUINodeParameters ToCustomUINodeParameters(CustomizedUINodeRequestDTO node)
    {
        return new CustomUINodeParameters(node.Key, node.Enabled);
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
