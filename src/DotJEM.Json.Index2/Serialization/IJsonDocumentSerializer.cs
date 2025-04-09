using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Index2.Serialization
{
    public interface IJsonDocumentSerializer
    {
        ISet<string> FieldsToLoad { get; }

        JObject Deserialize(byte[] value);
        byte[] Serialize(JObject json);

        void SerializeTo(JObject json, Document document);
        JObject DeserializeFrom(Document document);
    }

    public class DefaultJsonDocumentSerialier : IJsonDocumentSerializer
    {
        private const string FIELD_NAME = "$$RAW$$";

        public ISet<string> FieldsToLoad { get; } = new CharArraySet(LuceneVersion.LUCENE_48, new[] {FIELD_NAME}, false);

        public void SerializeTo(JObject json, Document document)
        {
            document.Add(new StoredField(FIELD_NAME, Serialize(json)));
        }

        public JObject DeserializeFrom(Document document)
        {
            return Deserialize(document.GetField(FIELD_NAME).GetBinaryValue().Bytes);
        }

        public JObject Deserialize(byte[] value)
        {
            using MemoryStream stream = new MemoryStream(value);
            using JsonTextReader reader = new JsonTextReader(new StreamReader(stream));

            var entity = (JObject)JToken.ReadFrom(reader);
            reader.Close();
            return entity;
        }

        public byte[] Serialize(JObject json)
        {
            using MemoryStream stream = new MemoryStream();
            using JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream));
            
            json.WriteTo(jsonWriter);
            jsonWriter.Flush();
            jsonWriter.Close();
            
            return stream.GetBuffer();
        }
    }
}
