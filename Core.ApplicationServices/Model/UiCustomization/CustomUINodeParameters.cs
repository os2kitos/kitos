namespace Core.ApplicationServices.Model.UiCustomization
{
    public class CustomUINodeParameters
    {
        public string Key { get; set; }
        public bool Enabled { get; set; }

        public CustomUINodeParameters(string key, bool enabled)
        {
            Key = key;
            Enabled = enabled;
        }
    }
}
