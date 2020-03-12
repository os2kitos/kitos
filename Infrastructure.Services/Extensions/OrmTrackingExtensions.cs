namespace Infrastructure.Services.Extensions
{
    public static class OrmTrackingExtensions
    {
        public static void Track<T>(this T src)
        {
            //Does not do anything but gets the reference loaded into memory. Used on child objects' reference properties
        }
    }
}
