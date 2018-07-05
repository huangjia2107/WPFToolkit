using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Utils.IO
{
    public static class XmlUtil
    {
        public static void AppendXmlDataToFile<T>(string strFilePath, T parameter)
        {
            using (var fs = new FileStream(strFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true
                };
                using (var xw = XmlWriter.Create(fs, settings))
                {
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);

                    var xs = new XmlSerializer(parameter.GetType());
                    xs.Serialize(xw, parameter, namespaces);
                }

                using (var sw = new StreamWriter(fs))
                {
                    sw.Write("\r\n");
                }
            }
        }

        public static T ConvertXmlNode<T>(XmlNode node) where T : class
        {
            var xs = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(node.OuterXml))
            {
                return (T)xs.Deserialize(stringReader);
            }
        }

        public static T ReadSpecifiedNoteDataFromFile<T>(string strFilePath, Predicate<T> predicate) where T : class
        {
            if (!File.Exists(strFilePath))
                return null;

            var xmldoc = new XmlDocument();
            xmldoc.LoadXml("<?xml version=\"1.0\"?><Root>" + File.ReadAllText(strFilePath) + @"</Root>");

            foreach (XmlNode xmlNode in xmldoc.SelectSingleNode("//Root").ChildNodes)
            {
                try
                {
                    var t = ConvertXmlNode<T>(xmlNode);
                    if (predicate(t))
                        return t;
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message + ex.StackTrace);
                }
            }

            return null;
        }

        public static List<T> ReadLatestSpecifiedCountXmlDataFromFile<T>(string strFilePath, int count) where T : class
        {
            var tList = new List<T>();
            if (!File.Exists(strFilePath))
                return tList;

            var xmldoc = new XmlDocument();
            xmldoc.LoadXml("<?xml version=\"1.0\"?><Root>" + File.ReadAllText(strFilePath) + @"</Root>");

            var currentXmlNode = xmldoc.SelectSingleNode("//Root").LastChild;
            while (tList.Count < count)
            {
                if (currentXmlNode == null)
                    break;

                tList.Add(ConvertXmlNode<T>(currentXmlNode));
                currentXmlNode = currentXmlNode.PreviousSibling;
            }

            return tList;
        }

        public static List<T> ReadAllNoteDataFromFile<T>(string strFilePath) where T : class
        {
            if (!File.Exists(strFilePath))
                return null;

            return ReadAllNoteDataFromFileContent<T>(File.ReadAllText(strFilePath));
        }

        public static List<T> ReadAllNoteDataFromFileContent<T>(string fileContent) where T : class
        {
            if (string.IsNullOrEmpty(fileContent))
                return null;

            try
            {
                var tList = new List<T>();

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(("<?xml version=\"1.0\"?><Root>" + fileContent + @"</Root>").Replace("\0", ""));

                var currentXmlNode = xmldoc.SelectSingleNode("//Root").FirstChild;

                while (true)
                {
                    if (currentXmlNode == null)
                        break;

                    tList.Add(ConvertXmlNode<T>(currentXmlNode));
                    currentXmlNode = currentXmlNode.NextSibling;
                }

                return tList;
            }
            catch (Exception ex)
            {
                Debug.Print("[ ReadAllNoteDataFromFile ] Exception : " + ex.Message + ex.StackTrace);
                return null;
            }
        }
    }
}
