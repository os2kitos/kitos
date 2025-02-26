using System;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class PublicMessageResponseDTO
    {
        public PublicMessageResponseDTO() {}

        public PublicMessageResponseDTO(PublicMessage publicMessageModel)
        {
            Uuid = publicMessageModel.Uuid;
            LongDescription = publicMessageModel.LongDescription;
            ShortDescription = publicMessageModel.ShortDescription;
            Status = publicMessageModel.Status?.ToPublicMessageStatusChoice();
            Link = publicMessageModel.Link;
            Title = publicMessageModel.Title;
        }

        public Guid Uuid { get; set; }
        public string Title { get; set; }
        public string LongDescription { get; set; }
        public string ShortDescription { get; set; }
        public PublicMessageStatusChoice? Status { get; set; }
        public string Link { get; set; }
    }
}