﻿namespace VocaDb.Model.Domain.Artists {

	/// <summary>
	/// Artist type (category).
	/// </summary>
	public enum ArtistType {

		Unknown,

		/// <summary>
		/// Doujin circle. A group of doujin producers that also releases music (acts as a label).
		/// </summary>
		Circle,

		/// <summary>
		/// Commercial music label. Does not produce music by itself.
		/// </summary>
		Label,

		/// <summary>
		/// Producer is the maker or the song (usually an individual, for example doriko)
		/// </summary>
		Producer,

		Animator,

		Illustrator,

		Lyricist,

		Vocaloid,

		UTAU,

		OtherVocalist,

		OtherGroup,

		OtherIndividual

	}
}
