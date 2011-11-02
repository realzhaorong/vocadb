﻿using System;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Users;

namespace VocaDb.Model.Domain {

	public class Comment {

		private string authorName;
		private string message;

		public Comment() {
			Created = DateTime.Now;
		}

		public Comment(string message, AgentLoginData loginData)
			: this() {

			ParamIs.NotNull(() => loginData);

			Message = message;
			Author = loginData.User;
			AuthorName = loginData.Name;

		}

		public virtual User Author { get; set; }

		public virtual string AuthorName {
			get { return authorName; }
			set {
				ParamIs.NotNullOrEmpty(() => value);
				authorName = value;
			}
		}

		public virtual DateTime Created { get; set; }

		public virtual int Id { get; set; }

		public virtual string Message {
			get { return message; }
			set {
				ParamIs.NotNullOrEmpty(() => value);
				message = value;
			}
		}

		public override string ToString() {
			return string.Format("comment [{0}]", Id);
		}

	}

}
