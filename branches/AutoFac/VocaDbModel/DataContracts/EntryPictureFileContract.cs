﻿using System.IO;
using VocaDb.Model.Domain;

namespace VocaDb.Model.DataContracts {

	public class EntryPictureFileContract {

		public EntryPictureFileContract() { }

		public EntryPictureFileContract(EntryPictureFile picture) {

			ParamIs.NotNull(() => picture);

			EntryType = picture.EntryType;
			FileName = picture.FileName;
			Id = picture.Id;
			Mime = picture.Mime;
			Name = picture.Name;

		}

		public int ContentLength { get; set; }

		public EntryType EntryType { get; set; }

		public string FileName { get; set; }

		public int Id { get; set; }

		public string Mime { get; set; }

		public string Name { get; set; }

		public Stream UploadedFile { get; set; }

		public EntryPictureFileContract NullToEmpty() {
			Name = Name ?? string.Empty;
			return this;
		}

	}
}
