﻿
function CreateSongViewModel() {

	var self = this;

	this.DupeEntries = ko.observableArray([]);
	this.NameOriginal = ko.observable();

	$("input.dupeField").focusout(function () {

		var term1 = $("#nameOriginal").val();
		var term2 = $("#nameRomaji").val();
		var term3 = $("#nameEnglish").val();
		var pv1 = $("#pv1").val();
		var pv2 = $("#pv2").val();

		$.post("../../Song/FindDuplicate", { term1: term1, term2: term2, term3: term3, pv1: pv1, pv2: pv2, getPVInfo: true }, function (result) {

			self.DupeEntries(result.Matches);
			
			if (result.Title && !self.NameOriginal()) {
				self.NameOriginal(result.Title);
			}
			
			if (result.Artists && !$("#artistsTableBody tr").length) {

				$(result.Artists).each(function() {
					$.post("../../Artist/CreateArtistContractRow", { artistId: this.Id }, function (row) {
						var artistsTable = $("#artistsTableBody");
						artistsTable.append(row);
						$("#artistsTableBody a.artistLink:last").vdbArtistToolTip();
					});
				});

			}

		});

	});

}

function initPage() {

	function acceptArtistSelection(artistId, term) {

		if (!isNullOrWhiteSpace(artistId)) {
			$.post("../../Artist/CreateArtistContractRow", { artistId: artistId }, function (row) {
				var artistsTable = $("#artistsTableBody");
				artistsTable.append(row);
				$("#artistsTableBody a.artistLink:last").vdbArtistToolTip();
			});
		}

	}

	var artistAddList = $("#artistAddList");
	var artistAddName = $("input#artistAddName");
	var artistAddBtn = $("#artistAddAcceptBtn");

	initEntrySearch(artistAddName, artistAddList, "Artist", "../../Artist/FindJson",
		{
			allowCreateNew: false,
			acceptBtnElem: artistAddBtn,
			acceptSelection: acceptArtistSelection,
			createOptionFirstRow: function (item) { return item.Name + " (" + item.ArtistType + ")"; },
			createOptionSecondRow: function (item) { return item.AdditionalNames; },
			extraQueryParams: { artistTypes: "Vocaloid,UTAU,OtherVocalist,Producer,Circle,OtherGroup,Unknown,Animator,Illustrator,Lyricist,OtherIndividual" },
			height: 300
		});

	$("a.artistRemove").live("click", function () {

		$(this).parent().parent().remove();
		return false;

	});

	$("#artistsTableBody a.artistLink").vdbArtistToolTip();

	ko.applyBindings(new CreateSongViewModel());

}