using System;

namespace Core.DomainModel.PublicMessage
{
    public class PublicMessage : Entity, IHasUuid
    {
        public const int DefaultShortDescriptionMaxLength = 200;
        public const int DefaultTitleMaxLength = 50;

        public PublicMessage()
        {
            Uuid = Guid.NewGuid();
        }

        public Guid Uuid { get; set; }

        public string Title { get; set; }
        public string LongDescription { get; set; }
        public PublicMessageStatus? Status { get; set; }
        public string ShortDescription { get; set; }
        public string Link { get; set; }

        public void UpdateTitle(string title)
        {
            Title = title;
        }
        public void UpdateLongDescription(string value)
        {
            LongDescription = value;
        }

        public void UpdateStatus(PublicMessageStatus? status)
        {
            Status = status;
        }

        public void UpdateShortDescription(string shortDescription)
        {
            ShortDescription = shortDescription;
        }

        public void UpdateLink(string link)
        {
            Link = link;
        }
    }
}
