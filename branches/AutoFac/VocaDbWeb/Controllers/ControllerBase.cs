﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Newtonsoft.Json;
using VocaDb.Model.DataContracts;
using VocaDb.Model.Domain;
using VocaDb.Model.Helpers;
using VocaDb.Model.Service.Security;
using VocaDb.Model.DataContracts.UseCases;
using VocaDb.Model;
using VocaDb.Model.Service;
using VocaDb.Web.Helpers;

namespace VocaDb.Web.Controllers {

	public class ControllerBase : Controller {

		protected static readonly TimeSpan imageExpirationTime = TimeSpan.FromMinutes(5);
		protected const int entriesPerPage = 30;
		protected const int invalidId = 0;

		protected string Hostname {
			get {
				return CfHelper.GetRealIp(Request);
			}
		}

		protected int LoggedUserId {
			get {

				LoginManager.VerifyLogin();

				return LoginManager.LoggedUser.Id;

			}
		}

		protected LoginManager LoginManager {
			get { return MvcApplication.LoginManager; }
		}

		protected ServiceModel Services {
			get { return MvcApplication.Services; }
		}

		protected ActionResult NoId() {
			return HttpNotFound("No ID specified");
		}

		protected ActionResult Picture(EntryForPictureDisplayContract contract) {

			ParamIs.NotNull(() => contract);

			Response.Cache.SetETag(string.Format("{0}{1}v{2}", contract.EntryType, contract.EntryId, contract.Version));

			return Picture(contract.Picture, contract.Name);

		}

		protected void CheckConcurrentEdit(EntryType entryType, int id) {

			var conflictingEditor = ConcurrentEntryEditManager.CheckConcurrentEdits(new EntryRef(entryType, id), Login.User);

			if (conflictingEditor.UserId != ConcurrentEntryEditManager.Nothing.UserId) {

				var ago = DateTime.Now - conflictingEditor.Time;

				if (ago.TotalMinutes < 1) {

					TempData.SetStatusMessage(string.Format(ViewRes.EntryEditStrings.ConcurrentEditWarningNow, conflictingEditor.UserName));

				} else {

					TempData.SetStatusMessage(string.Format(ViewRes.EntryEditStrings.ConcurrentEditWarning, conflictingEditor.UserName, (int)ago.TotalMinutes));

				}

			}

		}

		protected ActionResult HttpStatusCodeResult(HttpStatusCode code, string message) {

			Response.StatusCode = (int)code;
			Response.StatusDescription = message;

			return Content((int)code + ": " + message);

		}

		protected void ParseAdditionalPictures(HttpPostedFileBase mainPic, IList<EntryPictureFileContract> pictures) {

			var additionalPics = Enumerable.Range(0, Request.Files.Count)
				.Select(i => Request.Files.Get(i))
				.Where(f => f.FileName != mainPic.FileName)
				.ToArray();
			var newPics = pictures.Where(p => p.Id == 0).ToArray();

			for (int i = 0; i < additionalPics.Length; ++i) {

				if (i >= newPics.Length)
					break;

				var file = additionalPics[i];

				if (file.ContentLength > ImageHelper.MaxImageSizeBytes) {
					ModelState.AddModelError("Pictures", "Picture file is too large.");
				}

				if (!ImageHelper.IsValidImageExtension(file.FileName)) {
					ModelState.AddModelError("Pictures", "Picture format is not valid.");
				}

				newPics[i].FileName = file.FileName;
				newPics[i].UploadedFile = file.InputStream;
				newPics[i].Mime = file.ContentType ?? string.Empty;
				newPics[i].ContentLength = file.ContentLength;

			}

			CollectionHelper.RemoveAll(pictures, p => p.Id == 0 && p.UploadedFile == null);

		}

		protected PictureDataContract ParseMainPicture(HttpPostedFileBase pictureUpload, string fieldName) {

			PictureDataContract pictureData = null;

			if (Request.Files.Count > 0 && pictureUpload != null && pictureUpload.ContentLength > 0) {

				if (pictureUpload.ContentLength > ImageHelper.MaxImageSizeBytes) {
					ModelState.AddModelError(fieldName, "Picture file is too large.");
				}

				if (!ImageHelper.IsValidImageExtension(pictureUpload.FileName)) {
					ModelState.AddModelError(fieldName, "Picture format is not valid.");
				}

				if (ModelState.IsValid) {

					pictureData = ImageHelper.GetOriginalAndResizedImages(
						pictureUpload.InputStream, pictureUpload.ContentLength, pictureUpload.ContentType ?? string.Empty);

				}

			}

			return pictureData;

		}

		protected ActionResult Picture(PictureContract pictureData, string title) {

			if (pictureData == null)
				return File(Server.MapPath("~/Content/unknown.png"), "image/png");

			var ext = ImageHelper.GetExtensionFromMime(pictureData.Mime);

			if (ext != null) {
				//var encoded = Url.Encode(title);
				Response.AddHeader("content-disposition", "inline;filename=\"" + title + ext + "\"");
				return File(pictureData.Bytes, pictureData.Mime);
			} else {
				return File(pictureData.Bytes, pictureData.Mime);
			}

		}

		protected new ActionResult Json(object obj) {

			return Content(JsonConvert.SerializeObject(obj), "application/json");
	
		}

		protected new ActionResult Json(object obj, string jsonPCallback) {

			if (string.IsNullOrEmpty(jsonPCallback))
				return Json(obj);

			return Content(string.Format("{0}({1})", jsonPCallback, JsonConvert.SerializeObject(obj)), "application/json");

		}

		protected ActionResult Object<T>(T obj, DataFormat format) where T : class {

			if (format == DataFormat.Xml)
				return Xml(obj);
			else
				return Json(obj);

		}

		protected ActionResult Object<T>(T obj, DataFormat format, string jsonPCallback) where T : class {

			if (format == DataFormat.Xml)
				return Xml(obj);
			else
				return Json(obj, jsonPCallback);

		}

		protected string RenderPartialViewToString(string viewName, object model) {

			if (string.IsNullOrEmpty(viewName))
				viewName = ControllerContext.RouteData.GetRequiredString("action");

			ViewData.Model = model;

			using (var sw = new StringWriter()) {
				var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
				var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
				viewResult.View.Render(viewContext, sw);

				return sw.GetStringBuilder().ToString();
			}

		}

		protected void RestoreErrorsFromTempData() {

			var list = TempData["ModelErrors"] as ModelStateList;

			if (list == null)
				return;

			foreach (var state in list.ModelStates) {
				if (ModelState[state.Key] == null || !ModelState[state.Key].Errors.Any()) {
					foreach (var err in state.Errors) {
						if (err.Exception != null)
							ModelState.AddModelError(state.Key, err.Exception);
						else
							ModelState.AddModelError(state.Key, err.ErrorMessage);
					}
				}
			}

		}

		protected void SaveErrorsToTempData() {

			var list = new ModelStateList { ModelStates 
				= ViewData.ModelState.Select(m => new ModelStateErrors(m.Key, m.Value)).ToArray() };

			TempData["ModelErrors"] = list;

		}

		protected void SetSearchEntryType(EntryType entryType) {

			ViewData["GlobalSearchObjectType"] = entryType;

		}

		protected ActionResult Xml<T>(T obj) where T : class {

			if (obj == null)
				return new EmptyResult();

			var doc = XmlHelper.SerializeToXml(obj);
			doc.Declaration = new XDeclaration("1.0", "utf-8", "yes");
			var wr = new StringWriter();
			doc.Save(wr);

			return new ContentResult {
				ContentType = "text/xml",
				Content = wr.ToString(),
				ContentEncoding = Encoding.UTF8
			};

		}

	}

	class ModelStateList {

		public ModelStateErrors[] ModelStates;

	}

	class ModelStateErrors {

		public ModelStateErrors() { }

		public ModelStateErrors(string key, ModelState state) {

			Key = key;
			Errors = state.Errors;

		}

		public string Key { get; set; }

		public ModelErrorCollection Errors { get; set; }

	}

	public enum DataFormat {

		Auto,

		Json,

		Xml

	}

}