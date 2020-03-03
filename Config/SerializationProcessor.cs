using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Yoloanno.Config
{
    public static class SerializationProcessor
    {
        public static void Serialize<T>(string filename, T serializationObject)
        {
            var xs = new XmlSerializer(typeof(T), new[] { typeof(T) });
            TextWriter writer = new StreamWriter(filename);            
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);
            xs.Serialize(writer, serializationObject, xns);
            writer.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">File system path or URL that points to XML file.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string filePath)
        {
            var result = default(T);
            if (!string.IsNullOrEmpty(filePath))
                return Deserialize<T>(new NamespaceIgnorantXmlTextReader(filePath) { Namespaces = false });
            return result;
        }

        public static T Deserialize<T>(XmlTextReader configReader)
        {
            var result = default(T);
            if (configReader == null)
            {                
                return result;
            }
            try
            {
                var xs = new XmlSerializer(typeof(T));
                result = (T)xs.Deserialize(configReader);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                configReader.Close();
            }
            return result;
        }

        private class NamespaceIgnorantXmlTextReader : XmlTextReader
        {
            public NamespaceIgnorantXmlTextReader(string path) : base(path) { }

            public override string NamespaceURI
            {
                get { return ""; }
            }
        }

        public static string ObjectToXml<T>(T obj)
        {
            var xsSubmit = new XmlSerializer(typeof(T));
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    xsSubmit.Serialize(writer, obj, xns);
                    xml = sww.ToString(); // Your XML
                }
            }
            return xml;
        }

        public static T XmlToObject<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            T result;

            using (TextReader reader = new StringReader(xml))
            {
                result = (T)serializer.Deserialize(reader);
            }
            return result;
        }
    }
}
