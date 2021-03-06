﻿namespace VocaDb.Model.DataContracts {

	/// <summary>
	/// Entry name (title) with primary display name and additional names.
	/// </summary>
	public class EntryNameContract {

		public static EntryNameContract Empty {
			get {
				return new EntryNameContract(string.Empty);
			}
		}

		public EntryNameContract(string displayName, string additionalNames) {
			DisplayName = displayName;
			AdditionalNames = additionalNames;
		}

		public EntryNameContract(string displayName)
			: this(displayName, string.Empty) {}

		public string AdditionalNames { get; set; }

		public string DisplayName { get; set; }



	}

}
