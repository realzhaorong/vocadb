﻿using System;
using System.Linq;
using log4net;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using VocaDb.Model.DataContracts;
using VocaDb.Model.DataContracts.Artists;
using VocaDb.Model.DataContracts.Songs;
using VocaDb.Model.DataContracts.Tags;
using VocaDb.Model.DataContracts.UseCases;
using VocaDb.Model.Domain;
using VocaDb.Model.Domain.Artists;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Songs;
using VocaDb.Model.Domain.Tags;
using VocaDb.Model.Domain.Users;
using VocaDb.Model.Service.Helpers;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.DataContracts.Albums;
using System.Drawing;
using VocaDb.Model.Helpers;

namespace VocaDb.Model.Service {

	public class ArtistService : ServiceBase {

		private static readonly ILog log = LogManager.GetLogger(typeof(ArtistService));

		private PartialFindResult<ArtistWithAdditionalNamesContract> FindArtists(
			ISession session, string query, ArtistType[] artistTypes, int start, int maxResults,
			bool draftsOnly, bool getTotalCount, 
			NameMatchMode nameMatchMode = NameMatchMode.Auto) {

			if (string.IsNullOrWhiteSpace(query)) {

				bool filterByArtistType = artistTypes.Any();
				Artist art = null;
				//IList<ArtistName> names = null;

				var q = session.QueryOver(() => art)
					//.Left.JoinAlias(a => a.Names, () => names)
					.Where(s => !s.Deleted);

				if (draftsOnly)
					q = q.Where(a => a.Status == EntryStatus.Draft);

				if (filterByArtistType)
					q = q.WhereRestrictionOn(s => s.ArtistType).IsIn(artistTypes);

				var artists = q
					.OrderBy(s => s.Names.SortNames.Romaji).Asc
					.TransformUsing(new DistinctRootEntityResultTransformer())
					.Skip(start)
					.Take(maxResults)
					.List();

				var contracts = artists.Select(s => new ArtistWithAdditionalNamesContract(s, PermissionContext.LanguagePreference))
					.ToArray();

				var count = (getTotalCount ? GetArtistCount(session, query, artistTypes, draftsOnly, nameMatchMode) : 0);

				return new PartialFindResult<ArtistWithAdditionalNamesContract>(contracts, count);

			} else {

				query = query.Trim();

				var directQ = session.Query<Artist>()
					.Where(s => !s.Deleted);

				if (draftsOnly)
					directQ = directQ.Where(a => a.Status == EntryStatus.Draft);

				if (artistTypes.Any())
					directQ = directQ.Where(s => artistTypes.Contains(s.ArtistType));

				directQ = AddNameMatchFilter(directQ, query, nameMatchMode);
					
				var direct = directQ
					.OrderBy(s => s.Names.SortNames.Romaji)
					.Take(maxResults)
					//.FetchMany(s => s.Names)
					.ToArray();

				var additionalNamesQ = session.Query<ArtistName>()
					.Where(m => !m.Artist.Deleted);

				if (draftsOnly)
					additionalNamesQ = additionalNamesQ.Where(a => a.Artist.Status == EntryStatus.Draft);

				if (nameMatchMode == NameMatchMode.Exact || (nameMatchMode == NameMatchMode.Auto && query.Length < 3)) {

					additionalNamesQ = additionalNamesQ.Where(m => m.Value == query);

				} else {

					additionalNamesQ = additionalNamesQ.Where(m => m.Value.Contains(query));

				}

				if (artistTypes.Any())
					additionalNamesQ = additionalNamesQ.Where(m => artistTypes.Contains(m.Artist.ArtistType));

				var additionalNames = additionalNamesQ
					.Select(m => m.Artist)
					.OrderBy(s => s.Names.SortNames.Romaji)
					.Distinct()
					.Take(maxResults)
					//.FetchMany(s => s.Names)
					.ToArray()
					.Where(a => !direct.Contains(a));

				var contracts = direct.Concat(additionalNames)
					.Skip(start)
					.Take(maxResults)
					.Select(a => new ArtistWithAdditionalNamesContract(a, PermissionContext.LanguagePreference))
					.ToArray();

				var count = (getTotalCount ? GetArtistCount(session, query, artistTypes, draftsOnly, nameMatchMode) : 0);

				return new PartialFindResult<ArtistWithAdditionalNamesContract>(contracts, count);

			}

		}

		private int GetArtistCount(ISession session, string query, ArtistType[] artistTypes, bool draftsOnly, 
			NameMatchMode nameMatchMode) {

			if (string.IsNullOrWhiteSpace(query)) {

				var q = session.Query<Artist>()
					.Where(s => 
						!s.Deleted
						&& (!artistTypes.Any() || artistTypes.Contains(s.ArtistType)));

				if (draftsOnly)
					q = q.Where(a => a.Status == EntryStatus.Draft);

				return q.Count();

			}

			var directQ = session.Query<Artist>()
				.Where(s => !s.Deleted);

			if (artistTypes.Any())
				directQ = directQ.Where(s => artistTypes.Contains(s.ArtistType));

			if (draftsOnly)
				directQ = directQ.Where(a => a.Status == EntryStatus.Draft);

			directQ = AddNameMatchFilter(directQ, query, nameMatchMode);

			var direct = directQ.ToArray();

			var additionalNamesQ = session.Query<ArtistName>()
				.Where(m => !m.Artist.Deleted);

			if (draftsOnly)
				additionalNamesQ = additionalNamesQ.Where(a => a.Artist.Status == EntryStatus.Draft);

			if (query.Length < 3) {

				additionalNamesQ = additionalNamesQ.Where(m => m.Value == query);

			} else {

				additionalNamesQ = additionalNamesQ.Where(m => m.Value.Contains(query));

			}

			if (artistTypes.Any())
				additionalNamesQ = additionalNamesQ.Where(m => artistTypes.Contains(m.Artist.ArtistType));

			var additionalNames = additionalNamesQ
				.Select(m => m.Artist)
				.Distinct()
				.ToArray()
				.Where(a => !direct.Contains(a))
				.ToArray();

			return direct.Count() + additionalNames.Count();

		}

		[Obsolete]
		private T[] GetArtists<T>(Func<Artist, T> func) {

			return HandleQuery(session => session
				.Query<Artist>()
				.Where(a => !a.Deleted)
				.ToArray()
				.OrderBy(a => a.DefaultName)
				.Select(func)
				.ToArray());

		}

		public ArtistService(ISessionFactory sessionFactory, IUserPermissionContext permissionContext, IEntryLinkFactory entryLinkFactory)
			: base(sessionFactory, permissionContext, entryLinkFactory) {}

		public ArtistForAlbumContract AddAlbum(int artistId, int albumId) {

			PermissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			return HandleTransaction(session => {
				
				var artist = session.Load<Artist>(artistId);
				var album = session.Load<Album>(albumId);

				AuditLog(string.Format("adding {0} for {1}", 
					EntryLinkFactory.CreateEntryLink(album), EntryLinkFactory.CreateEntryLink(artist)), session);

				var artistForAlbum = artist.AddAlbum(album);
				session.Save(artistForAlbum);

				album.UpdateArtistString();
				session.Update(album);

				return new ArtistForAlbumContract(artistForAlbum, PermissionContext.LanguagePreference);

			});

		}

		public void Archive(ISession session, Artist artist, ArtistDiff diff, ArtistArchiveReason reason, string notes = "") {

			AuditLog("Archiving " + artist);

			var agentLoginData = SessionHelper.CreateAgentLoginData(session, PermissionContext);
			var archived = ArchivedArtistVersion.Create(artist, diff, agentLoginData, reason, notes);
			session.Save(archived);

		}

		public void Archive(ISession session, Artist artist, ArtistArchiveReason reason, string notes = "") {

			Archive(session, artist, new ArtistDiff(), reason, notes);

		}

		public ArtistContract Create(CreateArtistContract contract) {

			ParamIs.NotNull(() => contract);

			if (contract.Names == null || !contract.Names.Any())
				throw new ArgumentException("Artist needs at least one name", "contract");

			PermissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			return HandleTransaction(session => {

				AuditLog(string.Format("creating a new artist with name '{0}'", contract.Names.First().Value));

				var artist = new Artist { 
					ArtistType = contract.ArtistType, 
					Description = contract.Description.Trim(), 
					Status = contract.Draft ? EntryStatus.Draft : EntryStatus.Finished 
				};

				artist.Names.Init(contract.Names, artist);

				if (contract.WebLink != null) {
					artist.CreateWebLink(contract.WebLink.Description, contract.WebLink.Url);
				}

				session.Save(artist);

				Archive(session, artist, ArtistArchiveReason.Created);
				session.Update(artist);

				AuditLog(string.Format("created {0}", EntryLinkFactory.CreateEntryLink(artist)), session);

				return new ArtistContract(artist, PermissionContext.LanguagePreference);

			});

		}

		public ArtistContract Create(string name, IUserPermissionContext permissionContext) {

			ParamIs.NotNullOrWhiteSpace(() => name);
			ParamIs.NotNull(() => permissionContext);

			PermissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			name = name.Trim();

			return HandleTransaction(session => {

				AuditLog(string.Format("creating a new artist with name '{0}'", name), session);

				var artist = new Artist(name);

				session.Save(artist);

				Archive(session, artist, ArtistArchiveReason.Created);
				session.Update(artist);

				return new ArtistContract(artist, PermissionContext.LanguagePreference);

			});

		}

		public CommentContract CreateComment(int artistId, string message) {

			ParamIs.NotNullOrEmpty(() => message);

			PermissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			message = message.Trim();

			return HandleTransaction(session => {

				var artist = session.Load<Artist>(artistId);
				var author = GetLoggedUser(session);

				AuditLog(string.Format("creating comment for {0}: '{1}'", 
					EntryLinkFactory.CreateEntryLink(artist), 
					message.Truncate(60)), session, author);

				var comment = artist.CreateComment(message, author);
				session.Save(comment);

				return new CommentContract(comment);

			});

		}

		public void Delete(int id) {

			PermissionContext.VerifyPermission(PermissionFlags.DeleteEntries);

			UpdateEntity<Artist>(id, (session, a) => {

				AuditLog(string.Format("deleting {0}", EntryLinkFactory.CreateEntryLink(a)), session);

				//ArchiveArtist(session, permissionContext, a);
				a.Delete();
			                         
			}, skipLog: true);

		}

		public void DeleteComment(int commentId) {

			HandleTransaction(session => {

				var comment = session.Load<ArtistComment>(commentId);
				var user = GetLoggedUser(session);

				AuditLog("deleting " + comment, session, user);

				if (!user.Equals(comment.Author))
					PermissionContext.VerifyPermission(PermissionFlags.ManageUserBlocks);

				comment.Artist.Comments.Remove(comment);
				session.Delete(comment);

			});

		}

		public PartialFindResult<ArtistWithAdditionalNamesContract> FindArtists(string query, ArtistType[] artistTypes, int start, int maxResults, bool draftsOnly, bool getTotalCount) {

			return HandleQuery(session => FindArtists(session, query, artistTypes, start, maxResults, draftsOnly, getTotalCount));

		}

		public ArtistWithAdditionalNamesContract FindByNames(string[] query) {

			return HandleQuery(session => {

				foreach (var q in query.Where(q => !string.IsNullOrWhiteSpace(q))) {

					var result = FindArtists(session, q, new ArtistType[] {}, 0, 1, false, false, NameMatchMode.Exact);

					if (result.Items.Any())
						return result.Items.First();

				}

				return null;

			});

		}

		public AlbumWithAdditionalNamesContract[] GetAlbums(int artistId) {

			// TODO: sorting could be done in DB

			return HandleQuery(session =>
				session.Load<Artist>(artistId).Albums.Select(a => 
					new AlbumWithAdditionalNamesContract(a.Album, PermissionContext.LanguagePreference)).OrderBy(a => a.Name).ToArray());

		}

		public EntryForPictureDisplayContract GetArchivedArtistPicture(int archivedVersionId) {

			return HandleQuery(session =>
				EntryForPictureDisplayContract.Create(session.Load<ArchivedArtistVersion>(archivedVersionId)));

		}

		public ArtistContract GetArtist(int id) {

			return HandleQuery(session => new ArtistContract(session.Load<Artist>(id), PermissionContext.LanguagePreference));

		}

		public ArtistDetailsContract GetArtistDetails(int id) {

			return HandleQuery(session => {
			                   	
				var contract = new ArtistDetailsContract(session.Load<Artist>(id), PermissionContext.LanguagePreference);

				contract.CommentCount = session.Query<ArtistComment>().Where(c => c.Artist.Id == id).Count();

				contract.LatestAlbums = session.Query<ArtistForAlbum>()
					.Where(s => !s.Album.Deleted && s.Artist.Id == id)
					.Select(s => s.Album)
					.OrderByDescending(s => s.CreateDate)
					.Take(15).ToArray()
					.Select(s => new AlbumWithAdditionalNamesContract(s, PermissionContext.LanguagePreference))
					.ToArray();

				contract.LatestSongs = session.Query<ArtistForSong>()
					.Where(s => !s.Song.Deleted && s.Artist.Id == id)
					.Select(s => s.Song)
					.OrderByDescending(s => s.CreateDate)
					.Take(15).ToArray()
					.Select(s => new SongWithAdditionalNamesContract(s, PermissionContext.LanguagePreference))
					.ToArray();

				return contract;

			});

		}

		public ArtistForEditContract GetArtistForEdit(int id) {

			return
				HandleQuery(session =>
					new ArtistForEditContract(session.Load<Artist>(id), PermissionContext.LanguagePreference,
						session.Query<Artist>().Where(a => !a.Deleted 
							&& (a.ArtistType == ArtistType.Circle 
								|| a.ArtistType == ArtistType.Label
								|| a.ArtistType == ArtistType.OtherGroup))));

		}

		public ArtistWithAdditionalNamesContract GetArtistWithAdditionalNames(int id) {

			return HandleQuery(session => new ArtistWithAdditionalNamesContract(session.Load<Artist>(id), PermissionContext.LanguagePreference));

		}

		/// <summary>
		/// Gets the picture for a <see cref="Artist"/>.
		/// </summary>
		/// <param name="id">Artist Id.</param>
		/// <param name="requestedSize">Requested size. If Empty, original size will be returned.</param>
		/// <returns>Data contract for the picture. Can be null if there is no picture.</returns>
		public EntryForPictureDisplayContract GetArtistPicture(int id, Size requestedSize) {

			return HandleQuery(session => 
				EntryForPictureDisplayContract.Create(session.Load<Artist>(id), PermissionContext.LanguagePreference, requestedSize));

		}

		public ArtistWithArchivedVersionsContract GetArtistWithArchivedVersions(int artistId) {

			return HandleQuery(session => new ArtistWithArchivedVersionsContract(
				session.Load<Artist>(artistId), PermissionContext.LanguagePreference));

		}

		[Obsolete]
		public ArtistContract[] GetArtists() {

			return GetArtists(a => new ArtistContract(a, PermissionContext.LanguagePreference));

		}

		[Obsolete]
		public ArtistWithAdditionalNamesContract[] GetArtistsWithAdditionalNames() {

			return GetArtists(a => new ArtistWithAdditionalNamesContract(a, PermissionContext.LanguagePreference));

		}

		public ArtistContract[] GetCircles() {

			return HandleQuery(session => session.Query<Artist>()
				.Where(a => !a.Deleted && (a.ArtistType == ArtistType.Circle || a.ArtistType == ArtistType.Label))
				.ToArray()
				.Select(a => new ArtistContract(a, PermissionContext.LanguagePreference))
				.ToArray());

		}

		public CommentContract[] GetComments(int artistId) {

			return HandleQuery(session => {

				return session.Query<ArtistComment>()
					.Where(c => c.Artist.Id == artistId)
					.OrderByDescending(c => c.Created)
					.Select(c => new CommentContract(c)).ToArray();

			});

		}

		public SongWithAdditionalNamesContract[] GetSongs(int artistId) {

			// TODO: sorting could be done in DB

			return HandleQuery(session =>
				session.Load<Artist>(artistId).Songs.Select(a => 
					new SongWithAdditionalNamesContract(a.Song, PermissionContext.LanguagePreference)).OrderBy(s => s.Name).ToArray());

		}

		public TagSelectionContract[] GetTagSelections(int artistId, int userId) {

			return HandleQuery(session => {

				var tagsInUse = session.Query<ArtistTagUsage>().Where(a => a.Artist.Id == artistId).ToArray();
				var tagVotes = session.Query<ArtistTagVote>().Where(a => a.User.Id == userId && a.Usage.Artist.Id == artistId).ToArray();

				var tagSelections = tagsInUse.Select(t =>
					new TagSelectionContract(t.Tag.Name, t.Votes.Any(v => tagVotes.Any(v.Equals))));

				return tagSelections.ToArray();

			});

		}

		public ArchivedArtistVersionDetailsContract GetVersionDetails(int id) {

			return HandleQuery(session =>
				new ArchivedArtistVersionDetailsContract(session.Load<ArchivedArtistVersion>(id), PermissionContext.LanguagePreference));

		}

		public void Merge(int sourceId, int targetId) {

			PermissionContext.VerifyPermission(PermissionFlags.MergeEntries);

			if (sourceId == targetId)
				throw new ArgumentException("Source and target artists can't be the same", "targetId");

			HandleTransaction(session => {

				var source = session.Load<Artist>(sourceId);
				var target = session.Load<Artist>(targetId);

				AuditLog(string.Format("Merging {0} to {1}", 
					EntryLinkFactory.CreateEntryLink(source), EntryLinkFactory.CreateEntryLink(target)), session);

				foreach (var n in source.Names.Names.Where(n => !target.HasName(n))) {
					var name = target.CreateName(n.Value, n.Language);
					session.Save(name);
				}

				foreach (var w in source.WebLinks.Where(w => !target.HasWebLink(w.Url))) {
					var link = target.CreateWebLink(w.Description, w.Url);
					session.Save(link);
				}

				var groups = source.Groups.Where(g => !target.HasGroup(g.Group)).ToArray();
				foreach (var g in groups) {
					g.MoveToMember(target);
					session.Update(g);
				}

				var members = source.Members.Where(m => !m.Member.HasGroup(target)).ToArray();
				foreach (var m in members) {
					m.MoveToGroup(target);
					session.Update(m);
				}

				var albums = source.Albums.Where(a => !target.HasAlbum(a.Album)).ToArray();
				foreach (var a in albums) {
					a.Move(target);
					session.Update(a);
				}

				var songs = source.Songs.Where(s => !target.HasSong(s.Song)).ToArray();
				foreach (var s in songs) {
					s.Move(target);
					session.Update(s);
				}

				if (target.Description == string.Empty)
					target.Description = source.Description;

				source.Deleted = true;

				//Archive(session, source, "Merged to '" + target.DefaultName + "'");
				Archive(session, target, ArtistArchiveReason.Merged, "Merged from '" + source.DefaultName + "'");

				session.Update(source);
				session.Update(target);

			});

		}

		public void Restore(int artistId) {

			PermissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			HandleTransaction(session => {

				var artist = session.Load<Artist>(artistId);

				artist.Deleted = false;

				AuditLog("restored " + EntryLinkFactory.CreateEntryLink(artist), session);

			});

		}

		public TagUsageContract[] SaveTags(int artistId, string[] tags) {

			ParamIs.NotNull(() => tags);

			PermissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			return HandleTransaction(session => {

				tags = tags.Distinct(new CaseInsensitiveStringComparer()).ToArray();

				var user = session.Load<User>(PermissionContext.LoggedUser.Id);
				var artist = session.Load<Artist>(artistId);

				AuditLog(string.Format("tagging {0} with {1}", 
					EntryLinkFactory.CreateEntryLink(artist), string.Join(", ", tags)), session, user);

				var existingTags = session.Query<Tag>().ToDictionary(t => t.Name, new CaseInsensitiveStringComparer());

				artist.Tags.SyncVotes(user, tags, existingTags, new TagFactory(session), new ArtistTagUsageFactory(session, artist));

				return artist.Tags.Usages.OrderByDescending(u => u.Count).Take(3).Select(t => new TagUsageContract(t)).ToArray();

			});

		}

		public void UpdateBasicProperties(ArtistForEditContract properties, PictureDataContract pictureData, IUserPermissionContext permissionContext) {
			
			ParamIs.NotNull(() => properties);
			ParamIs.NotNull(() => permissionContext);

			permissionContext.VerifyPermission(PermissionFlags.ManageDatabase);

			HandleTransaction(session => {

				var artist = session.Load<Artist>(properties.Id);
				var diff = new ArtistDiff(DoSnapshot(artist.GetLatestVersion()));

				AuditLog(string.Format("updating properties for {0}", artist));

				if (artist.ArtistType != properties.ArtistType) {
					artist.ArtistType = properties.ArtistType;
					diff.ArtistType = true;
				}

				if (artist.Description != properties.Description) {
					artist.Description = properties.Description;
					diff.Description = true;
				}

				if (artist.TranslatedName.DefaultLanguage != properties.TranslatedName.DefaultLanguage) {
					artist.TranslatedName.DefaultLanguage = properties.TranslatedName.DefaultLanguage;
					diff.OriginalName = true;
				}

				if (pictureData != null) {
					artist.Picture = new PictureData(pictureData);
					diff.Picture = true;
				}

				if (artist.Status != properties.Status) {
					artist.Status = properties.Status;
					diff.Status = true;
				}

				var nameDiff = artist.Names.Sync(properties.Names, artist);
				SessionHelper.Sync(session, nameDiff);

				if (nameDiff.Changed)
					diff.Names = true;

				var webLinkDiff = WebLink.Sync(artist.WebLinks, properties.WebLinks, artist);
				SessionHelper.Sync(session, webLinkDiff);

				if (webLinkDiff.Changed)
					diff.WebLinks = true;

				if (diff.ArtistType || diff.Names) {

					foreach (var song in artist.Songs) {
						song.Song.UpdateArtistString();
						session.Update(song);
					}

				}

				var groupsDiff = CollectionHelper.Diff(artist.Groups, properties.Groups, (i, i2) => (i.Id == i2.Id));

				foreach (var grp in groupsDiff.Removed) {
					grp.Delete();
					session.Delete(grp);
				}

				foreach (var grp in groupsDiff.Added) {
					var link = artist.AddGroup(session.Load<Artist>(grp.Group.Id));
					session.Save(link);
				}

				if (groupsDiff.Changed)
					diff.Groups = true;

				var albumGetter = new Func<AlbumForArtistEditContract, Album>(contract => {

					Album album;

					if (contract.AlbumId != 0) {

						album = session.Load<Album>(contract.AlbumId);

					} else {

						AuditLog(string.Format("creating a new album '{0}' to {1}", contract.AlbumName, artist));

						album = new Album(contract.AlbumName);
						session.Save(album);

						Services.Albums.Archive(session, album, AlbumArchiveReason.Created,
							string.Format("Created for artist '{0}'", artist.DefaultName));

						AuditLog(string.Format("created {0} for {1}", 
							EntryLinkFactory.CreateEntryLink(album), EntryLinkFactory.CreateEntryLink(artist)), session);

					}

					return album;

				});

				var albumDiff = artist.SyncAlbums(properties.AlbumLinks, albumGetter);

				SessionHelper.Sync(session, albumDiff);

				if (albumDiff.Changed) {

					diff.Albums = true;

					var add = string.Join(", ", albumDiff.Added.Select(i => i.Album.ToString()));
					var rem = string.Join(", ", albumDiff.Removed.Select(i => i.Album.ToString()));

					var str = string.Format("edited albums (added: {0}, removed: {1})", add, rem)
						.Truncate(300);

					AuditLog(str, session);

				}

				AuditLog(string.Format("updated properties for {0} ({1})", 
					EntryLinkFactory.CreateEntryLink(artist), diff.ChangedFieldsString), session);

				Archive(session, artist, diff, ArtistArchiveReason.PropertiesUpdated);
				session.Update(artist);

				return true;

			});

		}

	}

	public class ArtistTagUsageFactory : ITagUsageFactory<ArtistTagUsage> {

		private readonly Artist artist;
		private readonly ISession session;

		public ArtistTagUsageFactory(ISession session, Artist artist) {
			this.session = session;
			this.artist = artist;
		}

		public ArtistTagUsage CreateTagUsage(Tag tag) {

			var usage = new ArtistTagUsage(artist, tag);
			session.Save(usage);

			return usage;

		}

	}


}
