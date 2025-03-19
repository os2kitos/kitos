using Core.Abstractions.Types;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic;

public static class YesNoPartiallyMappingExtensions
{
    private static readonly EnumMap<YesNoPartiallyChoice, YesNoPartiallyOption> Mapping;

    static YesNoPartiallyMappingExtensions()
    {
        Mapping = new EnumMap<YesNoPartiallyChoice, YesNoPartiallyOption>
        (
            (YesNoPartiallyChoice.No, YesNoPartiallyOption.No),
            (YesNoPartiallyChoice.Yes, YesNoPartiallyOption.Yes),
            (YesNoPartiallyChoice.Partially, YesNoPartiallyOption.Partially)
        );
    }

    public static YesNoPartiallyOption ToYesNoPartiallyOption(this YesNoPartiallyChoice value)
    {
        return Mapping.FromLeftToRight(value);
    }

    public static YesNoPartiallyChoice ToYesNoPartiallyChoice(this YesNoPartiallyOption value)
    {
        return Mapping.FromRightToLeft(value);

    }
}