﻿using System.Web.Http;
using System.Web.Http.Cors;
using VocaDb.Model.DataContracts.Songs;
using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Songs;
using VocaDb.Model.Service;
using VocaDb.Model.Service.Paging;
using VocaDb.Model.Service.Search.User;
using VocaDb.Web.Controllers.DataAccess;

namespace VocaDb.Web.Controllers.Api {

	/// <summary>
	/// API queries for users.
	/// </summary>
	[RoutePrefix("api/users")]
	public class UserApiController : ApiController {

		private const int absoluteMax = 50;
		private const int defaultMax = 10;
		private readonly IUserPermissionContext permissionContext;
		private readonly UserQueries queries;
		private readonly UserService service;

		public UserApiController(UserQueries queries, UserService service, IUserPermissionContext permissionContext) {
			this.queries = queries;
			this.service = service;
			this.permissionContext = permissionContext;
		}

		/// <summary>
		/// Gets a list of songs rated by a user.
		/// </summary>
		/// <param name="userId">ID of the user whose songs are to be browsed.</param>
		/// <param name="query">Song name query (optional).</param>
		/// <param name="rating">Filter songs by given rating (optional).</param>
		/// <param name="groupByRating">Group results by rating so that highest rated are first.</param>
		/// <param name="start">First item to be retrieved (optional, defaults to 0).</param>
		/// <param name="maxResults">Maximum number of results to be loaded (optional, defaults to 10, maximum of 50).</param>
		/// <param name="getTotalCount">Whether to load total number of items (optional, default to false).</param>
		/// <param name="sort">Sort rule (optional, defaults to Name). Possible values are None, Name, AdditionDate, FavoritedTimes, RatingScore.</param>
		/// <param name="nameMatchMode">Match mode for song name (optional, defaults to Auto).</param>
		/// <param name="fields">
		/// List of optional fields (optional). Possible values are Albums, Artists, Names, PVs, Tags, ThumbUrl, WebLinks.
		/// </param>
		/// <param name="lang">Content language preference (optional).</param>
		/// <returns>Page of songs with ratings.</returns>
		[Route("{userId:int}/ratedSongs")]
		public PartialFindResult<RatedSongForUserForApiContract> GetRatedSongs(
			int userId,
			string query = "", 
			SongVoteRating rating = SongVoteRating.Nothing,
			bool groupByRating = true,
			int start = 0, int maxResults = defaultMax, bool getTotalCount = false,
			SongSortRule? sort = null,
			NameMatchMode nameMatchMode = NameMatchMode.Auto, 
			SongOptionalFields fields = SongOptionalFields.None, 
			ContentLanguagePreference lang = ContentLanguagePreference.Default) {
			
			var queryParams = new RatedSongQueryParams(userId, new PagingProperties(start, maxResults, getTotalCount)) {
				Query = query,
				NameMatchMode = nameMatchMode,
				SortRule = sort ?? SongSortRule.Name,
				FilterByRating = rating,
				GroupByRating = groupByRating
			};

			var songs = queries.GetRatedSongs(queryParams, ratedSong => new RatedSongForUserForApiContract(ratedSong, lang, fields));
			return songs;

		}

		/// <summary>
		/// Add or update rating for a specific song, for the currently logged in user.
		/// If the user has already rated the song, any previous rating is replaced.
		/// Authorization cookie must be included.
		/// This API supports CORS.
		/// </summary>
		/// <param name="songId">ID of the song to be rated.</param>
		/// <param name="rating">Rating to be given. Possible values are Nothing, Like, Favorite.</param>
		/// <returns>The string "OK" if successful.</returns>
		[Route("current/ratedSongs/{songId:int}")]
		[Authorize]
		[EnableCors(origins: "*", headers: "*", methods: "post", SupportsCredentials = true)]
		public string PostSongRating(int songId, SongVoteRating rating) {
			
			service.UpdateSongRating(permissionContext.LoggedUserId, songId, rating);
			return "OK";

		}

	}
}