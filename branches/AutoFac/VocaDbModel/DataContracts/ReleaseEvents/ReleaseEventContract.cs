﻿using System;
using VocaDb.Model.Domain.Albums;

namespace VocaDb.Model.DataContracts.ReleaseEvents {

	public class ReleaseEventContract {

		public ReleaseEventContract() {
			Description = string.Empty;
		}

		public ReleaseEventContract(ReleaseEvent ev)
			: this() {

			ParamIs.NotNull(() => ev);

			Date = ev.Date;
			Description = ev.Description;
			Id = ev.Id;
			Name = ev.Name;

		}

		public DateTime? Date { get; set; }

		public string Description { get; set; }

		public int Id { get; set; }

		public string Name { get; set; }


	}

}
