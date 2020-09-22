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
        public static string ToDanishString(this YesNoIrrelevantOption yesNoIrrelevantOption)
        {
            return yesNoIrrelevantOption switch
            {
                YesNoIrrelevantOption.NO => "Nej",
                YesNoIrrelevantOption.YES => "Ja",
                YesNoIrrelevantOption.IRRELEVANT => "Ikke relevant",
                YesNoIrrelevantOption.UNDECIDED => "",
                _ => "",
            };
        }

        public static string ToDanishString(this YesNoIrrelevantOption? yesNoIrrelevantOption)
        {
            return yesNoIrrelevantOption.HasValue ? yesNoIrrelevantOption.Value.ToDanishString() : "";
        }
    }
}
