﻿using System.Runtime.Serialization;

namespace VocaDb.Model.DataContracts.Security {

	[DataContract(Namespace = Schemas.VocaDb)]
	public class UpdateUserSettingsContract : UserContract {

		public string NewPass { get; set; }

		public string OldPass { get; set; }

	}

}
