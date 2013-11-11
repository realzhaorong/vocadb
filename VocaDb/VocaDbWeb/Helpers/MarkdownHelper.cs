﻿using System.Web;
using MarkdownSharp;

namespace VocaDb.Web.Helpers {

	public static class MarkdownHelper {

		/// <summary>
		/// Transforms a block of text with Markdown.
		/// </summary>
		/// <param name="text">Text to be transformed. All HTML will be encoded!</param>
		/// <returns>Markdown-transformed text. This will include HTML.</returns>
		public static string TranformMarkdown(string text) {

			if (string.IsNullOrEmpty(text))
				return text;

			return new Markdown(new MarkdownOptions { AutoHyperlink = true, AutoNewLines = true }).Transform(HttpUtility.HtmlEncode(text));

		}

	}

}