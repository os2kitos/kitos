namespace Core.ApplicationServices.Model.Messages
{
    public class PublicMessages
    {
        public string About { get; }
        public string Guides { get; }
        public string StatusMessages { get; }
        public string Misc { get; }
        public string ContactInfo { get; }

        public PublicMessages(string about, string guides, string statusMessages, string misc, string contactInfo)
        {
            About = about;
            Guides = guides;
            StatusMessages = statusMessages;
            Misc = misc;
            ContactInfo = contactInfo;
        }
    }
}
