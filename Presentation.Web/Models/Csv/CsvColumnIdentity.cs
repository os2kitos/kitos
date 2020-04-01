using System;

namespace Presentation.Web.Models.Csv
{
    public class CsvColumnIdentity
    {
        public string Id { get; }

        public string DisplayName { get; }

        /// <param name="id">A valid c# property name</param>
        /// <param name="displayName">Display name to be used in the CSV output</param>
        public CsvColumnIdentity(string id, string displayName)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }
    }
}