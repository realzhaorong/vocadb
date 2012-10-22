﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocaDb.Model.Domain;

namespace VocaDb.Model.Service.Helpers {

	public class WebLinkCategoryHelper {

		enum StringMatchMode {

			Contains,

			StartsWith

		}

		class StringMatcher {

			private readonly string matcher;
			private readonly StringMatchMode matchMode;

			public StringMatcher(string matcher, StringMatchMode matchMode = StringMatchMode.Contains) {
				this.matchMode = matchMode;
				this.matcher = matcher;
			}

			public bool IsMatch(string val) {

				if (matchMode == StringMatchMode.Contains)
					return val.IndexOf(matcher, StringComparison.InvariantCultureIgnoreCase) != -1;
				else
					return val.StartsWith(matcher, StringComparison.InvariantCultureIgnoreCase);

			}

		}

		class CategoryMatcher {

			private readonly IList<StringMatcher> matchers;

			public CategoryMatcher(WebLinkCategory category, params StringMatcher[] matchers) {
				Category = category;
				this.matchers = matchers;
			}

			public WebLinkCategory Category { get; private set; }

			public bool IsMatch(string val) {
				return matchers.Any(m => m.IsMatch(val));
			}

		}

		private IList<CategoryMatcher> categoryMatchers = new List<CategoryMatcher>{
			new CategoryMatcher(WebLinkCategory.Official, 
				new StringMatcher("ameblo.jp/"),
				new StringMatcher("www.facebook.com/"),
				new StringMatcher("nicovideo.jp/user/"),
				new StringMatcher("com.nicovideo.jp/community/"),
				new StringMatcher("nicovideo.jp/mylist/"),
				new StringMatcher("piapro.jp/"),
				new StringMatcher("www.pixiv.net/member.php"),
				new StringMatcher("soundcloud.com/"),
				new StringMatcher("twitter.com/"),
				new StringMatcher("vimeo.com/"),
				new StringMatcher("youtube.com/user/")
			),
			new CategoryMatcher(WebLinkCategory.Commercial,
				new StringMatcher("www.amazon.co.jp/"),
				new StringMatcher("www.amazon.com/"),
				new StringMatcher("www.animate-onlineshop.jp/"),
				new StringMatcher("www.cdjapan.co.jp/detailview.html"),
				new StringMatcher("itunes.apple.com/"),
				new StringMatcher("karent.jp/"),
				new StringMatcher("listen.jp/store/"),
				new StringMatcher("shop.melonbooks.co.jp/"),
				new StringMatcher("books.rakuten.co.jp/"),
				new StringMatcher("toranoana.jp/mailorder/article/"),
				new StringMatcher("www.yesasia.com/")
			),
			new CategoryMatcher(WebLinkCategory.Reference,
				new StringMatcher("www.discogs.com/"),
				new StringMatcher("mikudb.com/"),
				new StringMatcher("www5.atwiki.jp/hmiku/pages/"),
				new StringMatcher("vgmdb.net/"),
				new StringMatcher("vocaloid.wikia.com/wiki/")
			)
		};

		public WebLinkCategory GetCategory(string url) {

			var matcher = categoryMatchers.FirstOrDefault(m => m.IsMatch(url));
			return (matcher != null ? matcher.Category : WebLinkCategory.Other);

		}

	}

}