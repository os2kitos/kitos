using System;

namespace Presentation.Web.Models.Shared
{
    public class ValueOptionWithOptionalDateDTO<T>
    {
        public T Value { get; set; }

        public DateTime? OptionalDateValue { get; set; }
    }
}