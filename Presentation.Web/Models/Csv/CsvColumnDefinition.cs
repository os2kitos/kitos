using System;

namespace Presentation.Web.Models.Csv
{
    public class CsvColumnDefinition<TModel>
    {
        public string Id { get; }
        public string Title { get; }
        public Func<TModel, string> BindValueFunc { get; }

        public CsvColumnDefinition(string id, string title, Func<TModel, string> bindValueFunc)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            BindValueFunc = bindValueFunc ?? throw new ArgumentNullException(nameof(bindValueFunc));
        }
    }
}