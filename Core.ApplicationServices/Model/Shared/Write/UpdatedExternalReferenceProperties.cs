using System;

namespace Core.ApplicationServices.Model.Shared.Write
{
    public class UpdatedExternalReferenceProperties : ExternalReferenceProperties
    {
        public Guid? Uuid { get; set; }

        public UpdatedExternalReferenceProperties(string title, string documentId, string url, bool masterReference) : base(title, documentId, url, masterReference)
        {
        }
    }
}
