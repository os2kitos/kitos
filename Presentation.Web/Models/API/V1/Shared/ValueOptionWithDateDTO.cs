using System;

namespace Presentation.Web.Models.API.V1.Shared
{
    public class ValueOptionWithOptionalDateDTO<T>
    {
        public T Value { get; set; }

        public DateTime? OptionalDateValue { get; set; }
    }
}