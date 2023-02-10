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

        public string Title { get; }
        public string DocumentId { get; }
        public string Url { get; }
        public bool MasterReference { get; }
    }
}
