namespace Core.ApplicationServices.Model.UiCustomization
{
    public class CustomUINodeParameters
    {
        public string Key { get; }
        public bool Enabled { get; }
        public bool Recommended { get; set; }

        public CustomUINodeParameters(string key, bool enabled, bool recommended)
        {
            Key = key;
            Enabled = enabled;
            Recommended = recommended;
        }
    }
}
