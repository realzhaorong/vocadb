﻿using System;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using VocaDb.Model.DataContracts.Songs;
using VocaDb.Model.DataContracts.UseCases;
using VocaDb.Model.Domain.Artists;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Ranking;
using VocaDb.Model.Domain.Songs;

namespace VocaDb.Model.Service {

	public class SongService : ServiceBase {

		public SongService(ISessionFactory sessionFactory)
			: base(sessionFactory) {

		}

		public SongContract CreateSong(CreateSongContract contract) {

			return HandleTransaction(session => {

				if (!string.IsNullOrEmpty(contract.BasicData.NicoId)) {

					var existing = session.Query<Song>().FirstOrDefault(s => s.NicoId == contract.BasicData.NicoId);

					if (existing != null) {
						throw new ServiceException("Song with NicoId '" + contract.BasicData.NicoId + "' has already been added");
					}			
		
				}

				var song = new Song(new TranslatedString(contract.BasicData.Name), contract.BasicData.NicoId);

				if (contract.AlbumId != null)
					song.AddAlbum(session.Load<Album>(contract.AlbumId.Value), 0);

				if (contract.PerformerId != null)
					song.AddArtist(session.Load<Artist>(contract.PerformerId.Value));

				if (contract.ProducerId != null)
					song.AddArtist(session.Load<Artist>(contract.ProducerId.Value));

				song.UpdateArtistString();
				session.Save(song);

				return new SongContract(song);

			});

		}

		public SongWithAdditionalNamesContract[] Find(string query, int start, int maxResults) {

			return HandleQuery(session => {


				var direct = session.Query<Song>()
					.Where(s => (string.IsNullOrEmpty(query)
						|| s.TranslatedName.English.Contains(query)
							|| s.TranslatedName.Romaji.Contains(query)
								|| s.TranslatedName.Japanese.Contains(query))
						|| (s.ArtistString.Contains(query))
						|| (s.NicoId != null && s.NicoId == query))
					.OrderBy(s => s.TranslatedName.Japanese)
					.Take(maxResults)
					.ToArray();

				var additionalNames = session.Query<SongName>()
					.Where(m => m.Value.Contains(query))
					.Select(m => m.Song)
					.Distinct()
					.Take(maxResults)
					.ToArray()
					.Where(a => !direct.Contains(a));

				return direct.Concat(additionalNames)
					.Skip(start)
					.Take(maxResults)
					.Select(a => new SongWithAdditionalNamesContract(a))
					.ToArray();

			});

		}

		public LyricsForSongContract GetRandomSongWithLyricsDetails() {

			return HandleQuery(session => {

				var ids = session.Query<LyricsForSong>().Select(s => s.Id).ToArray();
				var id = ids[new Random().Next(ids.Length)];

				return new LyricsForSongContract(session.Load<LyricsForSong>(id));

			});

		}

		public int GetSongCount(string query) {

			return HandleQuery(session => {

				//var artistForSong = (artistId != null ? session.Query<ArtistForSong>()
				//	.Where(a => a.Artist.Id == artistId).Select(a => a.Song).ToArray() : null);

				var direct = session.Query<Song>()
					.Where(s => string.IsNullOrEmpty(query)
						|| s.TranslatedName.English.Contains(query)
							|| s.TranslatedName.Romaji.Contains(query)
								|| s.TranslatedName.Japanese.Contains(query)
						|| (s.ArtistString.Contains(query))
						|| (s.NicoId == null || s.NicoId == query))
					.OrderBy(s => s.TranslatedName.Japanese)
					.ToArray();

				var additionalNames = session.Query<SongName>()
					.Where(m => m.Value.Contains(query))
					.Select(m => m.Song)
					.Distinct()
					.ToArray()
					.Where(a => !direct.Contains(a))
					.ToArray();

				return direct.Count() + additionalNames.Count();

			});

		}

		public SongDetailsContract GetSongDetails(int songId) {

			return HandleQuery(session => new SongDetailsContract(session.Load<Song>(songId)));

		}

		public SongContract[] GetSongs(string filter, int start, int count) {

			return HandleQuery(session => session.Query<Song>()
				.Where(s => string.IsNullOrEmpty(filter) 
					|| s.TranslatedName.Japanese.Contains(filter) 
					|| s.NicoId == filter)
				.OrderBy(s => s.TranslatedName.Japanese)
				.Skip(start)
				.Take(count)
				.ToArray()
				.Select(s => new SongContract(s))
				.ToArray());

		}

	}

}
