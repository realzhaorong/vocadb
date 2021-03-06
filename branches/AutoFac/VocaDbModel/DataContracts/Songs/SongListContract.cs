﻿using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Songs;

namespace VocaDb.Model.DataContracts.Songs {

	[DataContract(Namespace = Schemas.VocaDb)]
	public class SongListContract : SongListBaseContract {

		public SongListContract() {
			Description = string.Empty;
		}

		public SongListContract(SongList list, IUserPermissionContext permissionContext)
			: base(list) {

			ParamIs.NotNull(() => list);

			Author = new UserContract(list.Author);
			CanEdit = EntryPermissionManager.CanEdit(permissionContext, list);
			Description = list.Description;
			FeaturedCategory = list.FeaturedCategory;

		}

		[DataMember]
		public UserContract Author { get; set; }

		[DataMember]
		public bool CanEdit { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		[JsonConverter(typeof(StringEnumConverter))]
		public SongListFeaturedCategory FeaturedCategory { get; set; }

	}
}
