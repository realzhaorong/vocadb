﻿
function initPage(songId, urlMapper) {

	function acceptTargetSong(targetSongId) {

		$.post(urlMapper.mapRelative("/Song/CreateSongLink"), { songId: targetSongId }, function (content) {
			$("#targetSong").html(content);
		});

	}

	var songList = $("#songList");
	var songName = $("input#songName");
	var songIdBox = $("#targetSongId");

	initEntrySearch(songName, songList, "Song", urlMapper.mapRelative("/Song/FindJsonByName"),
		{
			acceptSelection: acceptTargetSong,
			filter: function (item) { return item.Id != songId; },
			idElem: songIdBox,
			createOptionFirstRow: function (item) { return item.Name + " (" + item.SongType + ")"; },
			createOptionSecondRow: function (item) { return item.ArtistString; },
			createTitle: function (item) { return item.AdditionalNames; }
		});

	$("#mergeBtn").click(function () {

		return confirm("Are you sure you want to merge the songs?");

	});

}