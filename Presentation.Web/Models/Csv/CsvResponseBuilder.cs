using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Core.ApplicationServices;
using Core.DomainModel.Result;

namespace Presentation.Web.Models.Csv
{
    public class CsvResponseBuilder<T>
    {
        private const string CsvFileNameExtension = ".csv";
        private readonly IList<CsvColumnDefinition<T>> _columns;
        private readonly IList<T> _rowInputs;
        private Maybe<string> _fileName;

        public CsvResponseBuilder()
        {
            _fileName = Maybe<string>.None;
            _columns = new List<CsvColumnDefinition<T>>();
            _rowInputs = new List<T>();
        }

        public CsvResponseBuilder<T> WithFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(@"Missing value", nameof(name));

            _fileName = name;

            return this;
        }

        public CsvResponseBuilder<T> WithColumn(string id, string title, Func<T, string> valueBinding)
        {
            _columns.Add(new CsvColumnDefinition<T>(id, title, valueBinding));
            return this;
        }

        public CsvResponseBuilder<T> WithRow(T rowInput)
        {
            _rowInputs.Add(rowInput);
            return this;
        }

        public HttpResponseMessage Build()
        {
            if (_fileName.IsNone)
                throw new InvalidOperationException("File name must be defined");
            if (_columns.Count == 0)
                throw new InvalidOperationException("No columns defined");

            var documentInput = new List<dynamic> { BuildHeaders() };
            documentInput.AddRange(BuildRows());
            return BuildResponse(documentInput);
        }

        private HttpResponseMessage BuildResponse(IEnumerable<object> documentInput)
        {
            var s = documentInput.ToCsv();
            var bytes = Encoding.UTF8.GetBytes(s);
            var stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream),
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = BuildFileName(),
                DispositionType = "attachment"

            };
            return result;
        }

        private string BuildFileName()
        {
            var name = _fileName.Value;
            return name.EndsWith(CsvFileNameExtension, StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}{CsvFileNameExtension}";
        }

        private ExpandoObject BuildHeaders()
        {
            var headerExpando = new ExpandoObject();
            var header = (IDictionary<string, object>)headerExpando;

            foreach (var column in _columns)
            {
                header.Add(column.Id, column.Title);
            }

            return headerExpando;
        }

        private IEnumerable<ExpandoObject> BuildRows()
        {
            foreach (var rowInput in _rowInputs)
            {
                var rowExpando = new ExpandoObject();
                var row = (IDictionary<string, object>)rowExpando;

                foreach (var column in _columns)
                {
                    row.Add(column.Id, column.BindValueFunc(rowInput));
                }

                yield return rowExpando;
            }
        }
    }
}