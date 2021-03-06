﻿using VocaDb.Model.Domain.Songs;
using VocaDb.Model.Service.Paging;

namespace VocaDb.Model.Service.Search.User {

	/// <summary>
	/// Query parameters for rated (favorited/liked) songs by user.
	/// </summary>
	public class RatedSongQueryParams {

		public RatedSongQueryParams(int userId, PagingProperties paging) {

			ParamIs.NotNull(() => paging);

			Paging = paging;
			UserId = userId;

			FilterByRating = SongVoteRating.Nothing;
			GroupByRating = true;
			SortRule = SongSortRule.Name;

		}

		public SongVoteRating FilterByRating { get; set; }

		/// <summary>
		/// Group by rating.
		/// </summary>
		public bool GroupByRating { get; set; }

		/// <summary>
		/// Paging properties. Cannot be null.
		/// </summary>
		public PagingProperties Paging { get; set; }

		/// <summary>
		/// Song sort rule.
		/// </summary>
		public SongSortRule SortRule { get; set; }

		/// <summary>
		/// Id of the user whose songs to get.
		/// </summary>
		public int UserId { get; set; }

	}
}
