﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VocaDb.Model;
using VocaDb.Model.DataContracts;
using VocaDb.Model.DataContracts.PVs;
using VocaDb.Model.DataContracts.Songs;
using VocaDb.Model.DataContracts.UseCases;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.DataContracts.Albums;
using VocaDb.Model.DataContracts.Artists;
using VocaDb.Model.Domain.Artists;
using VocaDb.Model.Helpers;
using VocaDb.Web.Helpers;
using VocaDb.Model.DataContracts.Tags;
using VocaDb.Model.Domain;
using VocaDb.Web.Models.Shared;

namespace VocaDb.Web.Models {

	public class AlbumEdit {

		public AlbumEdit() {
			
			Names = new List<LocalizedStringEdit>();
			Tracks = new List<SongInAlbumEditContract>();
			WebLinks = new List<WebLinkDisplay>();

			AllDiscTypes = EnumVal<DiscType>.Values;

		}

		public AlbumEdit(AlbumForEditContract album)
			: this() {

			ParamIs.NotNull(() => album);

			DefaultLanguageSelection = album.TranslatedName.DefaultLanguage;
			Description = album.Description;
			DiscType = album.DiscType;
			Draft = album.Status == EntryStatus.Draft;
			Id = album.Id;
			Name = album.Name;
			Names = album.Names.Select(n => new LocalizedStringEdit(n)).ToArray();
			Tracks = album.Songs;
			WebLinks = album.WebLinks.Select(w => new WebLinkDisplay(w)).ToArray();

			if (album.OriginalRelease != null) {
				CatNum = album.OriginalRelease.CatNum;
				ReleaseEvent = album.OriginalRelease.EventName;
				var d = album.OriginalRelease.ReleaseDate;
				if (d != null) {
					ReleaseDay = d.Day;
					ReleaseMonth = d.Month;
					ReleaseYear = d.Year;
				}
			}

			CopyNonEditableFields(album);

		}

		public DiscType[] AllDiscTypes { get; set; }

		public Dictionary<int, string> AllLabels { get; set; }

		public ArtistForAlbumContract[] ArtistLinks { get; set; }

		[Display(Name = "Catalog number")]
		[StringLength(50)]
		public string CatNum { get; set; }

		[Display(Name = "Original language")]
		public ContentLanguageSelection DefaultLanguageSelection { get; set; }

		[Display(Name = "Description")]
		public string Description { get; set; }

		[Display(Name = "Record type")]
		public DiscType DiscType { get; set; }

		[Display(Name = "This entry is a draft")]
		public bool Draft { get; set; }

		public int Id { get; set; }

		public string Name { get; set; }

		[Display(Name = "Names")]
		public IList<LocalizedStringEdit> Names { get; set; }

		[Display(Name = "Name in English")]
		public string NameEnglish { get; set; }

		[Display(Name = "Name in Japanese")]
		public string NameJapanese { get; set; }

		[Display(Name = "Name in Romaji")]
		public string NameRomaji { get; set; }

		[Display(Name = "PVs")]
		public IList<PVContract> PVs { get; set; }

		[Display(Name = "Day")]
		[Range(1, 31)]
		public int? ReleaseDay { get; set; }

		[Display(Name = "Month")]
		[Range(1, 12)]
		public int? ReleaseMonth { get; set; }

		[Display(Name = "Year")]
		public int? ReleaseYear { get; set; }

		[Display(Name = "Release event")]
		[StringLength(50)]
		public string ReleaseEvent { get; set; }

		[Display(Name = "Tracks")]
		public IList<SongInAlbumEditContract> Tracks { get; set; }

		[Display(Name = "External links")]
		public IList<WebLinkDisplay> WebLinks { get; set; }

		public void CopyNonEditableFields(AlbumForEditContract album) {

			ParamIs.NotNull(() => album);

			ArtistLinks = album.ArtistLinks;
			NameEnglish = album.TranslatedName.English;
			NameJapanese = album.TranslatedName.Japanese;
			NameRomaji = album.TranslatedName.Romaji;
			PVs = album.PVs;

		}

		public AlbumForEditContract ToContract() {

			return new AlbumForEditContract {
				Description = this.Description ?? string.Empty,
				DiscType = this.DiscType,
				Id = this.Id,
				Name = this.Name,
				Names = this.Names.Select(n => n.ToContract()).ToArray(),
				OriginalRelease = new AlbumReleaseContract {
					CatNum = this.CatNum,
					EventName = this.ReleaseEvent,
					ReleaseDate = new OptionalDateTimeContract {
						Day = this.ReleaseDay,
						Month = this.ReleaseMonth,
						Year = this.ReleaseYear
					}
				},
				Songs = Tracks.ToArray(),
				Status = (this.Draft ? EntryStatus.Draft : EntryStatus.Finished),
				TranslatedName = new TranslatedStringContract(
					NameEnglish, NameJapanese, NameRomaji, DefaultLanguageSelection),
				WebLinks = this.WebLinks.Select(w => w.ToContract()).ToArray()
			};

		}

	}

	public class AlbumDetails {

		public AlbumDetails() { }

		public AlbumDetails(AlbumDetailsContract contract) {

			ParamIs.NotNull(() => contract);

			AdditionalNames = contract.AdditionalNames;
			CommentCount = contract.CommentCount;
			Description = contract.Description;
			Deleted = contract.Deleted;
			DiscType = contract.DiscType;
			Draft = contract.Status == EntryStatus.Draft;
			Id = contract.Id;
			Locked = contract.Status == EntryStatus.Locked;
			Name = contract.Name;
			PVs = contract.PVs;
			Songs = contract.Songs;
			Tags = contract.Tags;
			UserHasAlbum = contract.UserHasAlbum;
			WebLinks = contract.WebLinks;

			if (contract.OriginalRelease != null) {
				CatNum = contract.OriginalRelease.CatNum;
				ReleaseEvent = contract.OriginalRelease.EventName;
				ReleaseDate = contract.OriginalRelease.ReleaseDate;
			}

			var artists = contract.ArtistLinks.Select(a => a.Artist);

			Circles = artists.Where(a => a.ArtistType == ArtistType.Circle).ToArray();
			Labels = artists.Where(a => a.ArtistType == ArtistType.Label).ToArray();
			OtherArtists = artists.Where(a => a.ArtistType == ArtistType.Unknown || a.ArtistType == ArtistType.OtherGroup).ToArray();
			Producers = artists.Where(a => a.ArtistType == ArtistType.Producer).ToArray();
			Vocalists = artists.Where(a => ArtistHelper.VocalistTypes.Contains(a.ArtistType)).ToArray();

			PrimaryPV = PVHelper.PrimaryPV(PVs);

		}

		public string AdditionalNames { get; set; }

		public string CatNum { get; set; }

		public ArtistWithAdditionalNamesContract[] Circles { get; set; }

		public int CommentCount { get; set; }

		public bool Deleted { get; set; }

		public string Description { get; set; }

		public DiscType DiscType { get; set; }

		public bool Draft { get; set; }

		public int Id { get; set; }

		public ArtistWithAdditionalNamesContract[] Labels { get; set; }

		public bool Locked { get; set; }

		public string Name { get; set; }

		public ArtistWithAdditionalNamesContract[] OtherArtists { get; set; }

		public PVContract PrimaryPV { get; set; }

		public ArtistWithAdditionalNamesContract[] Producers { get; set; }

		public PVContract[] PVs { get; set; }

		public string ReleaseEvent { get; set; }

		public OptionalDateTimeContract ReleaseDate { get; set; }

		public SongInAlbumContract[] Songs { get; set; }

		public TagUsageContract[] Tags { get; set; }

		public bool UserHasAlbum { get; set; }

		public ArtistWithAdditionalNamesContract[] Vocalists { get; set; }

		public WebLinkContract[] WebLinks { get; set; }

	}

}