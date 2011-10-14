﻿using System;
using VocaDb.Model.DataContracts.Artists;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Globalization;
using System.Runtime.Serialization;

namespace VocaDb.Model.DataContracts.Albums {

	[DataContract(Namespace = Schemas.VocaDb)]
	public class AlbumReleaseContract {

		public AlbumReleaseContract() {}

		public AlbumReleaseContract(AlbumRelease release, ContentLanguagePreference languagePreference) {
			
			ParamIs.NotNull(() => release);

			CatNum = release.CatNum;
			ReleaseDate = (release.ReleaseDate != null ? new OptionalDateTimeContract(release.ReleaseDate) : null);
			EventName = release.EventName;
			//Label = (release.Label != null ? new ArtistContract(release.Label, languagePreference) : null);

		}

		[DataMember]
		public string CatNum { get; set; }

		[DataMember]
		public OptionalDateTimeContract ReleaseDate { get; set; }

		[DataMember]
		public string EventName { get; set; }

	}

}
