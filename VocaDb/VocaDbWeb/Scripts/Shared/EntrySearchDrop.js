﻿function initEntrySearch(nameBoxElem, findListElem, entityName, searchUrl, params) {

	var w = 400;
	var h = 350;
	var idElem = null;
	var createNewItem = null;
	var acceptSelection = null;
	var extraQueryParams = null;
	var createOptionFirstRow = null;
	var createOptionSecondRow = null;
	var createOptionHtml = null;
	var createTitle = null;

	if (params != null) {

		if (params.width != null)
			w = params.width;

		if (params.height != null)
			h = params.height;

		if (params.createNewItem)
			createNewItem = params.createNewItem;

		if (params.acceptSelection != null)
			acceptSelection = params.acceptSelection;

		if (params.idElem != null)
			idElem = params.idElem;

		if (params.extraQueryParams != null)
			extraQueryParams = params.extraQueryParams;

		createOptionFirstRow = params.createOptionFirstRow;
		createOptionSecondRow = params.createOptionSecondRow;
		createOptionHtml = params.createOptionHtml;
		createTitle = params.createTitle;

	}

	function createHtml(item) {

		var data = item.data;

		if (!data) {
			return "<a><div>" + item.label + "</div></a>";
		}

		var html = null;

		if (createOptionHtml) {
			html = createOptionHtml(data);
		} else if (createOptionFirstRow && createOptionSecondRow) {
			var firstRow = createOptionFirstRow(data);
			var secondRow = createOptionSecondRow(data);
			if (firstRow)
				html = "<a><div>" + firstRow + "</div><div><small class='extraInfo'>" + secondRow + "</small></div></a>";
		} else if (createOptionFirstRow) {
			var firstRow = createOptionFirstRow(data);
			if (firstRow)
				html = "<a><div>" + firstRow + "</div></a>";
		}

		return html;

	}

	function getItems(par, response) {

		var queryParams = { term: par.term };

		if (extraQueryParams != null)
			jQuery.extend(queryParams, extraQueryParams);

		$.post(searchUrl, queryParams, function (result) {

			var mapped = $.map(result.Items, function (item) {
				return { label: item.Name, value: item.Id, data: item };
			});

			if (createNewItem)
				mapped.push({ label: createNewItem.replace("{0}", par.term), value: "" });

			response(mapped);

		});

	}

	function selectItem(event, ui) {

		if (idElem)
			$(idElem).val(ui.item.value);

		acceptSelection(ui.item.value, $(nameBoxElem).val());
		$(nameBoxElem).val("");

		return false;

	}

	var auto = $(nameBoxElem).autocomplete({
		source: getItems,
		select: selectItem
	})
	.data("autocomplete");

	if (auto) {
		auto._renderItem = function (ul, item) {
			return $("<li>")
				.data("item.autocomplete", item)
				.append(createHtml(item))
				.appendTo(ul);
		};
	}

}