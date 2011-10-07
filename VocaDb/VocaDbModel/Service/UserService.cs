﻿using System;
using System.Linq;
using System.Runtime.Serialization;
using log4net;
using NHibernate;
using NHibernate.Linq;
using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Users;
using VocaDb.Model.Service.Security;

namespace VocaDb.Model.Service {

	public class UserService : ServiceBase {

		private static readonly ILog log = LogManager.GetLogger(typeof(UserService));

		public UserService(ISessionFactory sessionFactory, IUserPermissionContext permissionContext)
			: base(sessionFactory, permissionContext) {

		}

		public AlbumForUserContract AddAlbum(int userId, int albumId) {

			PermissionContext.VerifyPermission(PermissionFlags.EditProfile);

			AuditLog("adding album '" + albumId + "' to user '" + userId + "'");

			return HandleTransaction(session => {

				var user = session.Load<User>(userId);
				var album = session.Load<Album>(albumId);

				var albumForUser = user.AddAlbum(album);
				session.Save(albumForUser);

				return new AlbumForUserContract(albumForUser, PermissionContext.LanguagePreference);

			});

		}

		public AlbumForUserContract AddAlbum(int userId, string newAlbumName) {

			PermissionContext.VerifyPermission(PermissionFlags.EditProfile);

			AuditLog("creating album '" + newAlbumName + "' to user '" + userId + "'");

			return HandleTransaction(session => {

				var user = session.Load<User>(userId);
				var album = new Album(new TranslatedString(newAlbumName));

				session.Save(album);
				var albumForUser = user.AddAlbum(album);
				session.Update(user);

				return new AlbumForUserContract(albumForUser, PermissionContext.LanguagePreference);

			});

		}

		public UserContract CheckAuthentication(string name, string pass) {

			return HandleTransaction(session => {

				var lc = name.ToLowerInvariant();
				var user = session.Query<User>().FirstOrDefault(u => u.Active && u.Name == lc);

				if (user == null)
					return null;

				var hashed = LoginManager.GetHashedPass(lc, pass, user.Salt);

				if (user.Password != hashed)
					return null;

				log.Info("'" + user + "' logged in");

				user.UpdateLastLogin();
				session.Update(user);

				return new UserContract(user);

			});

		}

		public UserContract Create(string name, string pass) {

			ParamIs.NotNullOrEmpty(() => name);
			ParamIs.NotNullOrEmpty(() => pass);

			log.Info("Creating user '" + name + "'");

			return HandleTransaction(session => {

				var lc = name.ToLowerInvariant();
				var existing = session.Query<User>().FirstOrDefault(u => u.NameLC == lc);

				if (existing != null)
					return null;

				var salt = new Random().Next();
				var hashed = LoginManager.GetHashedPass(lc, pass, salt);
				var user = new User(name, hashed, salt);
				session.Save(user);

				return new UserContract(user);

			});

		}

		public void DeleteAlbumForUser(int albumForUserId) {

			PermissionContext.VerifyPermission(PermissionFlags.EditProfile);

			DeleteEntity<AlbumForUser>(albumForUserId);

		}

		public UserContract[] GetUsers() {

			return HandleQuery(session => session.Query<User>().Select(u => new UserContract(u)).ToArray());

		}

		public UserContract GetUser(int id) {

			return HandleQuery(session => new UserContract(session.Load<User>(id)));

		}

		public UserDetailsContract GetUserDetails(int id) {

			return HandleQuery(session => new UserDetailsContract(session.Load<User>(id), PermissionContext.LanguagePreference));

		}

		public UserContract GetUserByName(string name) {

			return HandleQuery(session => new UserContract(session.Query<User>().First(u => u.Name == name)));

		}

		public void UpdateAlbumForUserMediaType(int albumForUserId, MediaType mediaType) {

			PermissionContext.VerifyPermission(PermissionFlags.EditProfile);

			UpdateEntity<AlbumForUser>(albumForUserId, albumForUser => albumForUser.MediaType = mediaType);

		}

		public void UpdateUser(UserContract contract) {

			ParamIs.NotNull(() => contract);

			PermissionContext.VerifyPermission(PermissionFlags.ManageUsers);

			UpdateEntity<User>(contract.Id, user => user.PermissionFlags = contract.PermissionFlags);

		}

		public UserContract UpdateUserSettings(UpdateUserSettingsContract contract) {

			ParamIs.NotNull(() => contract);

			PermissionContext.VerifyPermission(PermissionFlags.EditProfile);

			return HandleTransaction(session => {

				var user = session.Load<User>(contract.Id);

				if (!string.IsNullOrEmpty(contract.NewPass)) {

					var oldHashed = LoginManager.GetHashedPass(user.NameLC, contract.OldPass, user.Salt);

					if (user.Password != oldHashed)
						throw new InvalidPasswordException();

					var newHashed = LoginManager.GetHashedPass(user.NameLC, contract.NewPass, user.Salt);
					user.Password = newHashed;

				}

				user.DefaultLanguageSelection = contract.DefaultLanguageSelection;
				user.SetEmail(contract.Email);
				session.Update(user);

				return new UserContract(user);

			});

		}

	}

	public class InvalidPasswordException : Exception {
		
		public InvalidPasswordException()
			: base("Invalid password") {}

		protected InvalidPasswordException(SerializationInfo info, StreamingContext context) 
			: base(info, context) {}

	}

}
