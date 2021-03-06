﻿using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Web;
using NLog;
using VocaDb.Model.DataContracts;
using VocaDb.Model.Domain;
using VocaDb.Model.Utils;

namespace VocaDb.Model.Helpers {

	public static class ImageHelper {

		private static readonly string[] allowedExt = new[] { ".bmp", ".gif", ".jpg", ".jpeg", ".png" };
		public const int DefaultSmallThumbSize = 70;
		public const int DefaultThumbSize = 250;
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static string GetImagePath(EntryType entryType, string fileName) {

			return HttpContext.Current.Server.MapPath(string.Format("~\\EntryImg\\{0}\\{1}", entryType, fileName));

		}

		private static string GetImageUrl(EntryType entryType, string fileName) {

			return string.Format("{0}/EntryImg/{1}/{2}", AppConfig.HostAddress, entryType, fileName);

		}

		private static Image OpenImage(Stream stream) {
			try {
				return Image.FromStream(stream);
			} catch (ArgumentException x) {
				log.Error("Unable to open image", x);
				throw new InvalidPictureException("Unable to open image", x);
			}
		}

		public const int MaxImageSizeMB = 8;
		public const int MaxImageSizeBytes = MaxImageSizeMB * 1024 * 1024;

		public static string[] AllowedExtensions {
			get { return allowedExt; }
		}

		public static void GenerateThumbsAndMoveImages(IEnumerable<EntryPictureFile> newPictures) {

			foreach (var pic in newPictures) {

				if (pic.UploadedFile == null)
					continue;

				var path = GetImagePath(pic);
				var thumbPath = GetImagePathThumb(pic);
				//var smallThumbPath = GetImagePathSmallThumb(pic);

				using (var f = File.Create(path)) {
					pic.UploadedFile.CopyTo(f);
				}
				pic.UploadedFile.Seek(0, SeekOrigin.Begin);

				using (var original = OpenImage(pic.UploadedFile)) {

					if (original.Width > DefaultThumbSize || original.Height > DefaultThumbSize) {
						var thumb = ResizeToFixedSize(original, DefaultThumbSize, DefaultThumbSize);
						thumb.Save(thumbPath);
					} else {
						File.Copy(path, thumbPath);
					}

					/*if (original.Width > DefaultSmallThumbSize || original.Height > DefaultSmallThumbSize) {
						var thumb = ResizeToFixedSize(original, DefaultSmallThumbSize, DefaultSmallThumbSize);
						thumb.Save(smallThumbPath);
					} else {
						File.Copy(path, smallThumbPath);
					}*/

				}

			}

		}

		public static PictureThumbContract[] GenerateThumbs(Stream input, int[] sizes) {

			var thumbs = new List<PictureThumbContract>(sizes.Length);

			using (var original = OpenImage(input)) {

				foreach (var size in sizes) {

					if (size < original.Size.Width || size < original.Size.Height) {

						using (var scaled = ResizeToFixedSize(original, size, size))
						using (var memStream = new MemoryStream()) {

							//scaled.Save("C:\\Temp\\out", original.RawFormat);
							scaled.Save(memStream, original.RawFormat);
							var thumbBuf = StreamHelper.ReadStream(memStream, memStream.Length);
							thumbs.Add(new PictureThumbContract(thumbBuf, size));

						}

					}


				}
			}

			return thumbs.ToArray();

		}

		public static string GetExtensionFromMime(string mime) {

			switch (mime) {
				case MediaTypeNames.Image.Jpeg:
					return ".jpg";
				case "image/png":
					return ".png";
				case MediaTypeNames.Image.Gif:
					return ".gif";
				case "image/bmp":
					return ".bmp";
				default:
					return null;
			}

		}

		public static string GetImagePath(EntryPictureFileContract picture) {
			return GetImagePath(picture.EntryType, EntryPictureFile.GetFileName(picture.Id, picture.Mime));
		}

		public static string GetImagePathThumb(EntryPictureFileContract picture) {
			return GetImagePath(picture.EntryType, EntryPictureFile.GetFileNameThumb(picture.Id, picture.Mime));
		}

		public static string GetImagePath(EntryPictureFile picture) {
			return GetImagePath(picture.EntryType, EntryPictureFile.GetFileName(picture.Id, picture.Mime));
		}

		public static string GetImagePathSmallThumb(EntryPictureFile picture) {
			return GetImagePath(picture.EntryType, EntryPictureFile.GetFileNameSmallThumb(picture.Id, picture.Mime));
		}

		public static string GetImagePathThumb(EntryPictureFile picture) {
			return GetImagePath(picture.EntryType, EntryPictureFile.GetFileNameThumb(picture.Id, picture.Mime));
		}

		public static string GetImageUrl(EntryPictureFileContract picture) {
			return GetImageUrl(picture.EntryType, EntryPictureFile.GetFileName(picture.Id, picture.Mime));
		}

		public static string GetImageUrlThumb(EntryPictureFileContract picture) {
			return GetImageUrl(picture.EntryType, EntryPictureFile.GetFileNameThumb(picture.Id, picture.Mime));
		}

		public static PictureDataContract GetOriginalAndResizedImages(Stream input, int length, string contentType) {

			var buf = new Byte[length];
			input.Read(buf, 0, length);

			var contract = new PictureDataContract(buf, contentType);
			var thumbs = GenerateThumbs(input, new[] { 250 });
			var thumb250 = thumbs.FirstOrDefault(t => t.Size == 250);

			contract.Thumb250 = thumb250;

			return contract;

		}

		public static bool IsValidImageExtension(string fileName) {

			var ext = Path.GetExtension(fileName);

			return (allowedExt.Any(e => string.Equals(e, ext, StringComparison.InvariantCultureIgnoreCase)));

		}

		public static Image ResizeToFixedSize(Image imgPhoto, int width, int height) {

			int sourceWidth = imgPhoto.Width;
			int sourceHeight = imgPhoto.Height;

			double nPercent;
			var nPercentW = ((double)width / (double)sourceWidth);
			var nPercentH = ((double)height / (double)sourceHeight);

			if (nPercentH < nPercentW) {
				nPercent = nPercentH;
			} else {
				nPercent = nPercentW;
			}

			int destWidth = width = (int)(sourceWidth * nPercent);
			int destHeight = height = (int)(sourceHeight * nPercent);

			var bmPhoto = new Bitmap(width, height,
							  PixelFormat.Format32bppArgb);
			bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
							 imgPhoto.VerticalResolution);

			using (var grPhoto = Graphics.FromImage(bmPhoto)) {

				grPhoto.Clear(Color.Transparent);
				grPhoto.InterpolationMode =
						InterpolationMode.HighQualityBicubic;

				grPhoto.DrawImage(imgPhoto,
					new Rectangle(0, 0, destWidth, destHeight),
					new Rectangle(0, 0, sourceWidth, sourceHeight),
					GraphicsUnit.Pixel);

			}

			return bmPhoto;

		}
	}

	public class InvalidPictureException : Exception {
		public InvalidPictureException() {}
		public InvalidPictureException(string message) : base(message) {}
		public InvalidPictureException(string message, Exception innerException) : base(message, innerException) {}
		protected InvalidPictureException(SerializationInfo info, StreamingContext context) : base(info, context) {}
	}

}
