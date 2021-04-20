using System;

namespace Core.DomainModel.Shared
{
    public enum YesNoIrrelevantOption
    {
        NO = 0,
        YES = 1,
        IRRELEVANT = 2,
        UNDECIDED = 3
    }

    public static class YesNoIrrelevantOptionExtensions
    {
        public static string GetReadableName(this YesNoIrrelevantOption yesNoIrrelevantOption)
        {
            return yesNoIrrelevantOption switch
            {
                YesNoIrrelevantOption.NO => "Nej",
                YesNoIrrelevantOption.YES => "Ja",
                YesNoIrrelevantOption.IRRELEVANT => "Ikke relevant",
                YesNoIrrelevantOption.UNDECIDED => "",
                _ => throw new InvalidOperationException($"Invalid yesNoIrrelevantOption value: {yesNoIrrelevantOption}"),
            };
        }
    }
}
