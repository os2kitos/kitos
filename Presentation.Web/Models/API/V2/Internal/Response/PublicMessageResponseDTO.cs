using System;
using System.Diagnostics.CodeAnalysis;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class PublicMessageResponseDTO
    {
        public PublicMessageResponseDTO() {}

        [SetsRequiredMembers]
        public PublicMessageResponseDTO(PublicMessage publicMessageModel)
        {
            Uuid = publicMessageModel.Uuid;
            LongDescription = publicMessageModel.LongDescription;
            ShortDescription = publicMessageModel.ShortDescription;
            Status = publicMessageModel.Status?.ToPublicMessageStatusChoice();
            Link = publicMessageModel.Link;
            Title = publicMessageModel.Title;
            IconType = publicMessageModel.IconType?.ToPublicMessageIconTypeChoice();
            IsMain = publicMessageModel.IsMain;
        }

        public Guid Uuid { get; set; }
        public required string Title { get; set; }
        public required string LongDescription { get; set; }
        public required string ShortDescription { get; set; }
        public PublicMessageStatusChoice? Status { get; set; }
        public required string Link { get; set; }
        public PublicMessageIconTypeChoice? IconType { get; set; }
        public bool IsMain { get; set; }
    }
}