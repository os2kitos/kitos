namespace Core.ApplicationServices.Model.Shared.Write
{
    public class ExternalReferenceProperties
    {
        public ExternalReferenceProperties(string title, string documentId, string url, bool masterReference)
        {
            Title = title;
            DocumentId = documentId;
            Url = url;
            MasterReference = masterReference;
        }

        public string Title { get; set; }
        public string DocumentId { get; set; }
        public string Url { get; set; }
        public bool MasterReference { get; set; }
    }
}
