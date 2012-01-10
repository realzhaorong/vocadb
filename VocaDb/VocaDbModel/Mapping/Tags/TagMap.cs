﻿using FluentNHibernate.Mapping;
using VocaDb.Model.Domain.Tags;

namespace VocaDb.Model.Mapping.Tags {

	public class TagMap : ClassMap<Tag> {

		public TagMap() {

			Cache.NonStrictReadWrite();
			Id(m => m.Name).Column("[Name]").Length(30).GeneratedBy.Assigned();

			Map(m => m.CategoryName).Length(30).Not.Nullable();

			HasMany(m => m.AllAlbumTagUsages).Inverse();
			HasMany(m => m.AllArtistTagUsages).Inverse();
			HasMany(m => m.AllSongTagUsages).Inverse();

		}

	}
}
