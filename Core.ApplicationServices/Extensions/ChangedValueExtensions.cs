using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Extensions
{
    public static class ChangedValueExtensions
    {
        public static ChangedValue<T> AsChangedValue<T>(this T sourceValue) => new(sourceValue);
    }
}