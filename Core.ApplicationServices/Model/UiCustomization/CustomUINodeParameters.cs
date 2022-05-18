namespace Core.ApplicationServices.Model.UiCustomization
{
    public class CustomUINodeParameters
    {
        public string Key { get; }
        public bool Enabled { get; }

        public CustomUINodeParameters(string key, bool enabled)
        {
            Key = key;
            Enabled = enabled;
        }
    }
}
