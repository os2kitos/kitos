using System;

namespace Presentation.Web.Models.API.V1
{
    public class ValueWithOptionalDateAndRemark<T>
    {
        public T Value { get; set; }

        public DateTime? OptionalDateValue { get; set; }

        public string Remark { get; set; }
    }
}