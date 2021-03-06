﻿using System.Linq;
using System.Runtime.Serialization;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Songs;

namespace VocaDb.Model.DataContracts.Songs {

	[DataContract(Namespace = Schemas.VocaDb)]
	public class SongWithAdditionalNamesContract : SongContract {

		public SongWithAdditionalNamesContract(Song song, ContentLanguagePreference languagePreference)
			: base(song, languagePreference) {

		}

		public SongWithAdditionalNamesContract() { }

	}
}
