﻿using System.Runtime.Serialization;
using VocaDb.Model.Domain.Tags;

namespace VocaDb.Model.DataContracts.Tags {

	[DataContract(Namespace = Schemas.VocaDb)]
	public class ArchivedTagContract {

		public ArchivedTagContract() { }

		public ArchivedTagContract(Tag tag) {

			ParamIs.NotNull(() => tag);

			AliasedTo = tag.AliasedTo != null ? tag.AliasedTo.Name : null;
			CategoryName = tag.CategoryName;
			Description = tag.Description;
			Name = tag.Name;

		}

		[DataMember]
		public string AliasedTo { get; set; }

		[DataMember]
		public string CategoryName { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public string Name { get; set; }

	}

}
