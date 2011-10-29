﻿using System.Linq;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Globalization;

namespace VocaDb.Model.DataContracts.Albums {

	public class AlbumWithArchivedVersionsContract : AlbumContract {

		public AlbumWithArchivedVersionsContract(Album album, ContentLanguagePreference languagePreference)
			: base(album, languagePreference) {

			ParamIs.NotNull(() => album);

			ArchivedVersions = album.ArchivedVersions.Select(a => new ArchivedObjectVersionContract(a)).ToArray();

		}

		public ArchivedObjectVersionContract[] ArchivedVersions { get; set; }

	}

}
