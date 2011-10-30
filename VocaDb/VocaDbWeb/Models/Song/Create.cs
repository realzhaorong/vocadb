﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VocaDb.Model.DataContracts.Artists;
using VocaDb.Model.DataContracts.UseCases;

namespace VocaDb.Web.Models.Song {

	public class Create {

		public Create() {
			Artists = new List<ArtistContract>();
		}

		[Display(Name = "Artists")]
		public IList<ArtistContract> Artists { get; set; }

		[Display(Name = "Name in English")]
		[StringLength(255)]
		public string NameEnglish { get; set; }

		[Display(Name = "Original name")]
		[StringLength(255)]
		public string NameOriginal { get; set; }

		[Display(Name = "URL to the original PV (NicoNicoDouga or Youtube)")]
		[StringLength(255)]
		public string PVUrl { get; set; }

		[Display(Name = "Romanized name")]
		[StringLength(255)]
		public string NameRomaji { get; set; }

		[Display(Name = "URL to the reprint of the PV (NicoNicoDouga or Youtube)")]
		[StringLength(255)]
		public string ReprintPVUrl { get; set; }

		public CreateSongContract ToContract() {

			return new CreateSongContract {
				Artists = this.Artists.ToArray(),
				NameEnglish = this.NameEnglish,
				NameRomaji = this.NameRomaji,
				NameOriginal = this.NameOriginal,
				PVUrl = this.PVUrl,
				ReprintPVUrl = this.ReprintPVUrl
			};

		}

	}

}