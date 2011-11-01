﻿using VocaDb.Model.Domain.Artists;

namespace VocaDb.Model.DataContracts.Artists {

	public class CreateArtistContract {

		public ArtistType ArtistType { get; set; }

		public string Description { get; set; }

		public LocalizedStringContract[] Names { get; set; }

	}

}
