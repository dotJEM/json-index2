using System;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Documents.Fields
{
    public interface IFieldResolver
    {
        string IndentityField { get; }
        string ContentTypeField { get; }

        string ContentType(JObject entity);
        Term Identity(JObject entity);
    }

    public class FieldResolver : IFieldResolver
    {
        public string IndentityField { get; }
        public string ContentTypeField { get; }

        private readonly Func<JObject, Term> indentityFieldLookup;
        private readonly Func<JObject, string> contentTypeFieldLookup;

        public FieldResolver(string indentityField = "$id", string contentTypeField = "$contentType")
        {
            IndentityField = indentityField;
            ContentTypeField = contentTypeField;
            indentityFieldLookup = UseSelect(indentityField)
                ? obj => new (indentityField, (string)obj.SelectToken(indentityField))
                : obj => new (indentityField, (string)obj[indentityField]);

            contentTypeFieldLookup = UseSelect(contentTypeField)
                ? obj => (string)obj.SelectToken(contentTypeField)
                : obj => (string)obj[contentTypeField];
        }

        private static bool UseSelect(string indentityField)
        {
            return indentityField.Contains(".") || indentityField.Contains("[");
        }

        public string ContentType(JObject entity) => contentTypeFieldLookup(entity);

        public Term Identity(JObject entity) => indentityFieldLookup(entity);
    }
}
