﻿using System.Runtime.Serialization;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Globalization;

namespace VocaDb.Model.DataContracts.Albums {

	[DataContract(Namespace = Schemas.VocaDb)]
	public class AlbumWithAdditionalNamesContract : AlbumContract {

		public AlbumWithAdditionalNamesContract(Album album, ContentLanguagePreference languagePreference) 
			: base(album, languagePreference) {

		}

		public AlbumWithAdditionalNamesContract() {}

	}

}
