﻿using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Users;

namespace VocaDb.Tests.TestSupport {

	public class FakePermissionContext : IUserPermissionContext {

		public FakePermissionContext() {}

		public FakePermissionContext(UserContract loggedUser) {
			LoggedUser = loggedUser;
		}

		public bool HasPermission(PermissionToken token) {

			if (token == PermissionToken.Nothing)
				return true;

			if (!IsLoggedIn || !LoggedUser.Active)
				return false;

			return (LoggedUser.EffectivePermissions.Contains(token));

		}

		public bool IsLoggedIn {
			get { return LoggedUser != null; }
		}

		public ContentLanguagePreference LanguagePreference { get; private set; }

		public UserContract LoggedUser { get; set; }

		public int LoggedUserId {			
			get { return (LoggedUser != null ? LoggedUser.Id : 0); }
		}

		public string Name { get; private set; }
		public UserGroupId UserGroupId { get; private set; }

		public void VerifyLogin() {

			if (!IsLoggedIn)
				throw new NotAllowedException("Must be logged in.");

		}

		public void VerifyPermission(PermissionToken flag) {

			if (!HasPermission(flag)) {
				throw new NotAllowedException();
			}

		}

	}
}
