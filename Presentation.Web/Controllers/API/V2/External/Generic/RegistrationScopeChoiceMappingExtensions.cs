using Core.DomainModel;
using Core.Abstractions.Types;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public static class RegistrationScopeChoiceMappingExtensions
    {
        private static readonly EnumMap<RegistrationScopeChoice, AccessModifier> Mapping;

        static RegistrationScopeChoiceMappingExtensions()
        {
            Mapping = new EnumMap<RegistrationScopeChoice, AccessModifier>
            (
                (RegistrationScopeChoice.Global, AccessModifier.Public),
                (RegistrationScopeChoice.Local, AccessModifier.Local)
            );
        }

        public static AccessModifier FromChoice(this RegistrationScopeChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static RegistrationScopeChoice ToChoice(this AccessModifier value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}