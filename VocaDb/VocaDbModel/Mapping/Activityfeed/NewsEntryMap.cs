﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using VocaDb.Model.Domain.Activityfeed;

namespace VocaDb.Model.Mapping.Activityfeed {

	public class NewsEntryMap : ClassMap<NewsEntry> {

		public NewsEntryMap() {

			Table("NewsEntries");
			Id(m => m.Id);
			Cache.ReadWrite();

			Map(m => m.CreateDate).Not.Nullable();
			Map(m => m.Important).Not.Nullable();
			Map(m => m.Text).Not.Nullable();

			References(m => m.Author).Not.Nullable();

		}

	}
}
