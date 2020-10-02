namespace Core.DomainModel.Shared
{
    public enum YesNoIrrelevantOption
    {
        NO = 0,
        YES = 1,
        IRRELEVANT = 2,
        UNDECIDED = 3
    }

    public static class YesNoIrrelevantMapping
    {
        public static string ToDanishString(this YesNoIrrelevantOption src)
        {
            return src switch
            {
                YesNoIrrelevantOption.NO => "Nej",
                YesNoIrrelevantOption.YES => "Ja",
                YesNoIrrelevantOption.IRRELEVANT => "Ikke relevant",
                _ => ""
            };
        }
    }

}
