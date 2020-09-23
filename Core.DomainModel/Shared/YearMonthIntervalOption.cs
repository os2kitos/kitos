using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Shared
{
    public enum YearMonthIntervalOption
    {
        Half_yearly = 0,
        Yearly = 1,
        Every_second_year = 2,
        Other = 3
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
                _ => "",
            };
        }

        public static string TranslateToDanishString(this YearMonthIntervalOption? yearMonthIntervalOption)
        {
            return yearMonthIntervalOption.HasValue ? yearMonthIntervalOption.Value.TranslateToDanishString() : "";
        }

    }


}
