namespace Core.DomainModel
{
    /// <summary>
    /// Entity for storing user editable text
    /// </summary>
    public class Text : Entity
    {
        public static class SectionIds
        {
            public const int About = 1;
            public const int Misc = 2;
            public const int Guides = 3;
            public const int StatusMessages = 4;
            public const int ContactInfo = 5;
        }
        public string Value { get; set; }
    }
}
