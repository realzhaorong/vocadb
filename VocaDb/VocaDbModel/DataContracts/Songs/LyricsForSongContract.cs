﻿using System.Runtime.Serialization;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Songs;

namespace VocaDb.Model.DataContracts.Songs {

	[DataContract]
	public class LyricsForSongContract {

		public LyricsForSongContract(LyricsForSong lyrics) {
			
			ParamIs.NotNull(() => lyrics);

			Id = lyrics.Id;
			Language = lyrics.Language;
			Value = lyrics.Value;

		}

		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public ContentLanguageSelection Language { get; set; }

		[DataMember]
		public string Value { get; set; }

	}
}
