using System;
using System.Linq;
using Core.ApplicationServices.Model.Users.Write;
using Core.DomainModel;
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
                SendMailOnCreation = request.SendMailOnCreation,
                Roles = request.Roles.Select(x => x.ToOrganizationRole()).ToList()
            };
        }
    }
}