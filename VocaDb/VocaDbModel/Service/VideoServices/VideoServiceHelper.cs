﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VocaDb.Model.Domain.Songs;

namespace VocaDb.Model.Service.VideoServices {

	public static class VideoServiceHelper {

		private static readonly VideoService[] services = new[] { 
			new VideoService(PVService.Youtube, new[] {
				new RegexLinkMatcher("www.youtube.com/watch?v={0}", @"youtube.com/watch?\S*v=(\S{11})"),
				new RegexLinkMatcher("youtu.be/{0}", @"youtu.be/(\S{11})")
			}),
			new VideoService(PVService.NicoNicoDouga, new[] {
				new RegexLinkMatcher("www.nicovideo.jp/watch/{0}", @"nicovideo.jp/watch/\d{6,12}")
			})
		};

		public static VideoUrlParseResult ParseByUrl(string url) {

			var service = services.FirstOrDefault(s => s.IsValidFor(url));

			if (service == null)
				throw new VideoParseException("No video service defined for URL '" + url + "'");

			return service.ParseByUrl(url);

		}

	}
}
