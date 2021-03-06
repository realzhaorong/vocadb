﻿using System;
using VocaDb.Model.Domain.Activityfeed;
using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Songs;
using VocaDb.Model.Domain.Versioning;
using VocaDb.Model.Service.VideoServices;

namespace VocaDb.Model.DataContracts.Activityfeed {

	public class ActivityEntryContract {

		private string GetArtistString(IEntryBase entry, ContentLanguagePreference languagePreference) {

			if (entry is Album)
				return ((Album)entry).ArtistString[languagePreference];
			else if (entry is Song)
				return ((Song)entry).ArtistString[languagePreference];
			else
				return null;

		}

		private string GetSongThumbUrl(IEntryBase entry) {
			return (entry is Song ? VideoServiceHelper.GetThumbUrl(((Song)entry).PVs.PVs) : string.Empty);
		}

		public ActivityEntryContract(ActivityEntry entry, ContentLanguagePreference languagePreference) {

			ParamIs.NotNull(() => entry);

			ArtistString = GetArtistString(entry.EntryBase, languagePreference);
			Author = new UserContract(entry.Author);
			CreateDate = entry.CreateDate;
			EditEvent = entry.EditEvent;
			EntryRef = new EntryRefWithNameContract(entry.EntryBase, languagePreference);
			SongThumbUrl = GetSongThumbUrl(entry.EntryBase);

		}

		public ActivityEntryContract(ArchivedObjectVersion entry, ContentLanguagePreference languagePreference) {

			ParamIs.NotNull(() => entry);

			ArtistString = GetArtistString(entry.EntryBase, languagePreference);
			Author = new UserContract(entry.Author);
			CreateDate = entry.Created;
			EditEvent = entry.EditEvent;
			EntryRef = new EntryRefWithNameContract(entry.EntryBase, languagePreference);
			SongThumbUrl = GetSongThumbUrl(entry.EntryBase);

		}

		public string ArtistString { get; set; }

		public UserContract Author { get; set; }

		public DateTime CreateDate { get; set; }

		public EntryEditEvent EditEvent { get; set; }

		public EntryRefWithNameContract EntryRef { get; set; }

		public string SongThumbUrl { get; set; }

	}

}
