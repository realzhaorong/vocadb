﻿using System.Linq;
using System.Collections.Generic;
using VocaDb.Model.Domain.Users;

namespace VocaDb.Model.Domain.Tags {

	public abstract class TagUsage {

		private Tag tag;

		protected TagUsage() { }

		protected TagUsage(Tag tag) {
			Tag = tag;
		}

		public virtual int Count { get; set; }

		public abstract IEntryBase Entry { get; }

		public virtual long Id { get; set; }

		public virtual Tag Tag {
			get { return tag; }
			set {
				ParamIs.NotNull(() => value);
				tag = value;
			}
		}

		public abstract IEnumerable<TagVote> VotesBase { get; }

		public abstract TagVote CreateVote(User user);

		public virtual bool Equals(TagUsage another) {

			if (another == null)
				return false;

			if (ReferenceEquals(this, another))
				return true;

			if (Id == 0)
				return false;

			return this.Id == another.Id;

		}

		public override bool Equals(object obj) {
			return Equals(obj as TagUsage);
		}

		public override int GetHashCode() {
			return (Tag.Name + "_" + Entry.EntryType + Entry.Id).GetHashCode();
		}

		public virtual bool HasVoteByUser(User user) {

			ParamIs.NotNull(() => user);

			return VotesBase.Any(v => v.User.Equals(user));

		}

		public abstract TagVote RemoveVote(User user);

	}

}
