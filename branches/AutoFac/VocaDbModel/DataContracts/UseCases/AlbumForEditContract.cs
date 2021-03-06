﻿using System.Linq;
using System.Runtime.Serialization;
using VocaDb.Model.DataContracts.Albums;
using VocaDb.Model.DataContracts.PVs;
using VocaDb.Model.DataContracts.Songs;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Service.EntryValidators;

namespace VocaDb.Model.DataContracts.UseCases {

	public class AlbumForEditContract : AlbumWithAdditionalNamesContract {

		public AlbumForEditContract() {}

		public AlbumForEditContract(Album album, ContentLanguagePreference languagePreference)
			: base(album, languagePreference) {

			ArtistLinks = album.Artists.Select(a => new ArtistForAlbumContract(a, languagePreference)).OrderBy(a => a.Name).ToArray();
			Deleted = album.Deleted;
			Description = album.Description;
			Names = new NameManagerEditContract(album.Names);
			OriginalRelease = (album.OriginalRelease != null ? new AlbumReleaseContract(album.OriginalRelease) : null);
			Pictures = album.Pictures.Select(p => new EntryPictureFileContract(p)).ToArray();
			PVs = album.PVs.Select(p => new PVContract(p)).ToArray();
			Songs = album.Songs
				.OrderBy(s => s.TrackNumber).OrderBy(s => s.DiscNumber)
				.Select(s => new SongInAlbumEditContract(s, languagePreference)).ToArray();
			TranslatedName = new TranslatedStringContract(album.TranslatedName);
			UpdateNotes = string.Empty;
			ValidationResult = AlbumValidator.Validate(album);
			WebLinks = album.WebLinks.Select(w => new WebLinkContract(w)).OrderBy(w => w.DescriptionOrUrl).ToArray();

		}

		//public ArtistContract[] AllLabels { get; set; }

		[DataMember]
		public ArtistForAlbumContract[] ArtistLinks { get; set; }

		[DataMember]
		public bool Deleted { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public AlbumReleaseContract OriginalRelease { get; set; }

		public NameManagerEditContract Names { get; set; }

		[DataMember]
		public EntryPictureFileContract[] Pictures { get; set; }

		[DataMember]
		public PVContract[] PVs { get; set; }

		[DataMember]
		public SongInAlbumEditContract[] Songs { get; set; }

		public TranslatedStringContract TranslatedName { get; set; }

		public string UpdateNotes { get; set; }

		public ValidationResult ValidationResult { get; set; }

		[DataMember]
		public WebLinkContract[] WebLinks { get; set; }

	}

}
