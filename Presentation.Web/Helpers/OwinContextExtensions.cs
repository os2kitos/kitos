using Microsoft.Owin;

namespace Presentation.Web.Helpers
{
    public static class OwinContextExtensions
    {
        private const string Prefix = nameof(OwinContextExtensions);

        public static IOwinContext WithEnvironmentProperty<T>(this IOwinContext context, T value)
        {
            context.Set(GetPropertyName<T>(), value);
            return context;
        }

        public static T GetEnvironmentProperty<T>(this IOwinContext context)
        {
            return context.Get<T>(GetPropertyName<T>());
        }

        private static string GetPropertyName<T>()
        {
            return $"{Prefix}_{typeof(T).Name}";
        }
    }
}