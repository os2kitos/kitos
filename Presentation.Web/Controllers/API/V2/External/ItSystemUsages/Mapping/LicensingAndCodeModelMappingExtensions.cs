using Core.Abstractions.Types;
using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Types.System;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class LicensingAndCodeModelMappingExtensions
    {
        private static readonly EnumMap<LicensingAndCodeModel, LicensingAndCodeModelChoice> Mapping;

        static LicensingAndCodeModelMappingExtensions()
        {
            Mapping = new EnumMap<LicensingAndCodeModel, LicensingAndCodeModelChoice>(
                (LicensingAndCodeModel.Freeware, LicensingAndCodeModelChoice.Freeware),
                (LicensingAndCodeModel.OpenSource, LicensingAndCodeModelChoice.OpenSource),
                (LicensingAndCodeModel.Proprietary, LicensingAndCodeModelChoice.Proprietary)
                );
        }

        public static LicensingAndCodeModel FromChoice(this LicensingAndCodeModelChoice value)
        {
            return Mapping.FromRightToLeft(value);
        }

        public static LicensingAndCodeModelChoice ToChoice(this LicensingAndCodeModel value) {
            return Mapping.FromLeftToRight(value);
        }
    }
}
