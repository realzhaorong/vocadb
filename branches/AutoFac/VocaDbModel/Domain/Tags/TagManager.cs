﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VocaDb.Model.Domain.Users;
using VocaDb.Model.Helpers;

namespace VocaDb.Model.Domain.Tags {

	public class TagManager<T> where T : TagUsage {

		private Iesi.Collections.Generic.ISet<T> tags = new Iesi.Collections.Generic.HashedSet<T>();

		public virtual IEnumerable<Tag> Tags {
			get {
				return Usages.Select(t => t.Tag);
			}
		}

		public virtual Iesi.Collections.Generic.ISet<T> Usages {
			get { return tags; }
			set {
				ParamIs.NotNull(() => value);
				tags = value;
			}
		}

		public virtual bool HasTag(Tag tag) {

			ParamIs.NotNull(() => tag);

			return Usages.Any(u => u.Tag.Equals(tag));

		}

		public virtual bool HasTag(string tagName) {

			ParamIs.NotNull(() => tagName);

			return Usages.Any(u => u.Tag.Name.Equals(tagName));

		}

		public virtual void SyncVotes(User user, string[] tagNames, Dictionary<string, Tag> allTags, ITagFactory tagFactory, ITagUsageFactory<T> tagUsageFactory) {

			var newTags = tagNames.Where(t => !allTags.ContainsKey(t));

			foreach (var tag in newTags) {
				var newTag = tagFactory.CreateTag(tag);
				allTags.Add(newTag.Name, newTag);
			}

			tagNames = tagNames.Select(t => allTags[t].ActualTag.Name).Distinct().ToArray();
			var tagUsagesDiff = CollectionHelper.Diff(Usages, tagNames, (t1, t2) => t1.Tag.Name.Equals(t2, StringComparison.InvariantCultureIgnoreCase));

			foreach (var newUsageName in tagUsagesDiff.Added) {
				var tag = allTags[newUsageName];
				var newUsage = tagUsageFactory.CreateTagUsage(tag);
				Usages.Add(newUsage);
				newUsage.CreateVote(user);
			}

			foreach (var removedTag in tagUsagesDiff.Removed) {

				removedTag.RemoveVote(user);

				if (removedTag.Count <= 0)
					Usages.Remove(removedTag);

			}

			foreach (var updated in tagUsagesDiff.Unchanged) {

				updated.CreateVote(user);

			}

		}

	}

}
