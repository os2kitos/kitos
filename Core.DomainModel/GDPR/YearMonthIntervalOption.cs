namespace Core.DomainModel.Shared
{
    public enum YearMonthIntervalOption
    {
        Half_yearly = 0,
        Yearly = 1,
        Every_second_year = 2,
        Other = 3,
        Undecided = 4
    }


    public static class YearMonthIntervalOptionExtension
    {
        public static string TranslateToDanishString(this YearMonthIntervalOption yearMonthIntervalOption)
        {
            return yearMonthIntervalOption switch
                {
                YearMonthIntervalOption.Half_yearly => "Halvårligt",
                YearMonthIntervalOption.Yearly => "Årligt",
                YearMonthIntervalOption.Every_second_year => "Hver andet år",
                YearMonthIntervalOption.Other => "Andet",
                YearMonthIntervalOption.Undecided => "",
                _ => "",
            };
        }
    }
}
