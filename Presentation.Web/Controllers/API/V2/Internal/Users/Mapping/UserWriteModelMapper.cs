using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Users.Write;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.User;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public class UserWriteModelMapper : WriteModelMapperBase, IUserWriteModelMapper
    {
        public UserWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }
        
        public CreateUserParameters FromPOST(CreateUserRequestDTO request)
        {
            var user = new User
            {
                Email = request.Email,
                Name = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DefaultUserStartPreference = DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceString(request.DefaultUserStartPreference),
                HasApiAccess = request.HasApiAccess,
                HasStakeHolderAccess = request.HasStakeHolderAccess,
            };

            return new CreateUserParameters
            {
                User = user,
                SendMailOnCreation = request.SendMail,
                Roles = request.Roles.Select(x => x.ToOrganizationRole()).ToList()
            };
        }

        public UpdateUserParameters FromPATCH(UpdateUserRequestDTO request)
        {
            var rule = CreateChangeRule<UpdateUserRequestDTO>(false);
            var parameters = new UpdateUserParameters
            {
                Email = rule.MustUpdate(x => x.Email)
                    ? request.Email.AsChangedValue()
                    : OptionalValueChange<string>.None,
                FirstName = rule.MustUpdate(x => x.FirstName)
                    ? request.FirstName.AsChangedValue()
                    : OptionalValueChange<string>.None,
                LastName = rule.MustUpdate(x => x.LastName)
                    ? request.LastName.AsChangedValue()
                    : OptionalValueChange<string>.None,
                PhoneNumber = rule.MustUpdate(x => x.PhoneNumber)
                    ? request.PhoneNumber.AsChangedValue()
                    : OptionalValueChange<string>.None,
                DefaultUserStartPreference = rule.MustUpdate(x => x.DefaultUserStartPreference)
                    ? DefaultUserStartPreferenceChoiceMapper
                        .GetDefaultUserStartPreferenceString(request.DefaultUserStartPreference).AsChangedValue()
                    : OptionalValueChange<string>.None,
                HasApiAccess = rule.MustUpdate(x => x.HasApiAccess)
                    ? request.HasApiAccess.AsChangedValue()
                    : OptionalValueChange<bool>.None,
                HasStakeHolderAccess = rule.MustUpdate(x => x.HasStakeHolderAccess)
                    ? request.HasStakeHolderAccess.AsChangedValue()
                    : OptionalValueChange<bool>.None,
                Roles = rule.MustUpdate(x => x.Roles)
                    ? request.Roles.Select(x => x.ToOrganizationRole()).AsChangedValue()
                    : OptionalValueChange<IEnumerable<OrganizationRole>>.None,
                SendMailOnUpdate = request.SendMail,
                DefaultOrganizationUnitUuid = rule.MustUpdate(x => x.DefaultOrganizationUnitUuid)
                    ? request.DefaultOrganizationUnitUuid.AsChangedValue()
                    : OptionalValueChange<Guid>.None
            };
            return parameters;
        }
    }
}