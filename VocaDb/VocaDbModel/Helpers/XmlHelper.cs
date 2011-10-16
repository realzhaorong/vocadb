﻿using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace VocaDb.Model.Helpers {

	public static class XmlHelper {

		public static XDocument SerializeToXml<T>(T obj) {

			var serializer = new XmlSerializer(typeof(T));
			XDocument doc;

			using (var writer = new StringWriter()) {
				serializer.Serialize(writer, obj);
				doc = XDocument.Parse(writer.ToString());
			}

			/*using (var stream = new MemoryStream()) {
				serializer.Serialize(stream, contract);
				var reader = XmlReader.Create(stream);
				doc = XDocument.Load(reader);
			}*/

			/*var doc = new XDocument();
			serializer.Serialize(doc.CreateWriter(), contract);*/

			return doc;

		}

		public static T DeserializeFromXml<T>(XDocument doc) {

			var serializer = new XmlSerializer(typeof(T));
			T obj;

			using (var reader = doc.CreateReader()) {
				obj = (T)serializer.Deserialize(reader);
			}

			return obj;

		}

	}

}
