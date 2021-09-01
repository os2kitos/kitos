namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class NamedLink
    {
        public string Name { get; }
        public string Url { get; }

        public NamedLink(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
