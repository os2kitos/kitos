using System;

namespace Presentation.Web.Models.Application.Csv
{
    public class CsvColumnDefinition<TModel>
    {
        public CsvColumnIdentity Identity { get; }
        public Func<TModel, string> BindValueFunc { get; }

        public CsvColumnDefinition(CsvColumnIdentity identity, Func<TModel, string> bindValueFunc)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            BindValueFunc = bindValueFunc ?? throw new ArgumentNullException(nameof(bindValueFunc));
        }
    }
}