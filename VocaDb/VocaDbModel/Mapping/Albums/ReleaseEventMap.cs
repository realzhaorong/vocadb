﻿using FluentNHibernate.Mapping;
using VocaDb.Model.Domain.Albums;

namespace VocaDb.Model.Mapping.Albums {

	public class ReleaseEventMap : ClassMap<ReleaseEvent> {

		public ReleaseEventMap() {

			Table("AlbumReleaseEvents");
			Cache.ReadWrite();
			Id(m => m.Id);

			Map(m => m.Date).Not.Nullable();
			Map(m => m.Description).Length(200).Not.Nullable();
			Map(m => m.Name).Length(50).Not.Nullable();
			References(m => m.Series).Nullable();

		}

	}

	public class ReleaseEventSeriesMap : ClassMap<ReleaseEventSeries> {

		public ReleaseEventSeriesMap() {

			Table("AlbumReleaseEventSeries");
			Cache.ReadWrite();
			Id(m => m.Id);

			Map(m => m.Description).Length(200).Not.Nullable();
			Map(m => m.Name).Length(50).Not.Nullable();

			HasMany(m => m.Aliases).Inverse().Cascade.All().Cache.ReadWrite();
			HasMany(m => m.Events).Inverse();

		}

	}

	public class ReleaseEventSeriesAliasMap : ClassMap<ReleaseEventSeriesAlias> {

		public ReleaseEventSeriesAliasMap() {

			Table("AlbumReleaseEventSeriesAliases");
			Cache.ReadWrite();
			Id(m => m.Id);

			Map(m => m.Name).Length(50).Not.Nullable();
			References(m => m.Series).Not.Nullable();

		}

	}

}
