﻿using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using VocaDb.Model;
using VocaDb.Model.DataContracts;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Service;
using VocaDb.Model.Service.Security;
using VocaDb.Web.Models;

namespace VocaDb.Web.Controllers
{
    public class ArtistController : ControllerBase
    {

    	private LoginManager LoginManager {
			get { return MvcApplication.LoginManager; }
    	}

    	private ArtistService Service {
    		get { return MvcApplication.Services.Artists; }
    	}

        //
        // GET: /Artist/

        public ActionResult Index() {
			ViewBag.Artists = Service.GetArtistsWithAdditionalNames().OrderBy(a => a.Name);
            return View();
        }

		public ActionResult FindJson(string term) {

			var albums = Service.FindArtists(term, 20);

			return Json(albums);

		}

        //
        // GET: /Artist/Details/5

        public ActionResult Details(int id) {
        	var model = Service.GetArtistDetails(id);
            return View(model);
        }

		public ActionResult Picture(int id) {

			var pictureData = Service.GetArtistPicture(id);

			if (pictureData == null)
				return new EmptyResult();

			/*using (var stream = new MemoryStream(pictureData.Bytes)) {
				return new FileStreamResult(stream, pictureData.Mime);
			}*/
			return File(pictureData.Bytes, pictureData.Mime);

		}

        //
        // POST: /Artist/Create

        [HttpPost]
        public ActionResult Create(ObjectCreate model)
        {

			if (ModelState.IsValid) {

				var artist = Service.Create(model.Name, MvcApplication.LoginManager);
				return RedirectToAction("Edit", new { id = artist.Id });

			} else {
			
				return RedirectToAction("Index");

			}

        }
        
        //
        // GET: /Artist/Edit/5
 
        public ActionResult Edit(int id) {
        	var model = new ArtistEdit(Service.GetArtistForEdit(id));
            return View(model);
        }

        //
        // POST: /Artist/Edit/5

        [HttpPost]
		public ActionResult EditBasicDetails(ArtistEdit model)
        {

            PictureDataContract pictureData = null;

			if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0) {

				var file = Request.Files[0];
				var buf = new Byte[file.ContentLength];
				file.InputStream.Read(buf, 0, file.ContentLength);

				pictureData = new PictureDataContract(buf, file.ContentType);

			}

			Service.UpdateBasicProperties(model.ToContract(), pictureData, LoginManager);

			return RedirectToAction("Edit", new { id = model.Id });

        }

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult CreateName(int artistId, string nameVal, ContentLanguageSelection language) {

			var name = Service.CreateName(artistId, nameVal, language, LoginManager);

			return Json(name);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult CreateWebLink(int artistId, string description, string url) {

			var name = Service.CreateWebLink(artistId, description, url, LoginManager);

			return Json(name);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public void DeleteName(int nameId) {

			Service.DeleteName(nameId, LoginManager);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public void DeleteWebLink(int linkId) {

			Service.DeleteWebLink(linkId, LoginManager);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public void EditNameLanguage(int nameId, string nameLanguage) {

			Service.UpdateArtistNameLanguage(nameId, EnumVal<ContentLanguageSelection>.Parse(nameLanguage), LoginManager);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public void EditNameValue(int nameId, string nameVal) {

			Service.UpdateArtistNameValue(nameId, nameVal, LoginManager);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public void EditWebLinkDescription(int linkId, string description) {

			Service.UpdateWebLinkDescription(linkId, description, LoginManager);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public void EditWebLinkUrl(int linkId, string url) {

			Service.UpdateWebLinkUrl(linkId, url, LoginManager);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public PartialViewResult AddNewAlbum(int artistId, string newAlbumName) {

			var link = Service.AddAlbum(artistId, newAlbumName);
			return PartialView("ArtistForAlbumRow", link);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		public PartialViewResult AddExistingAlbum(int artistId, int albumId) {

			var link = Service.AddAlbum(artistId, albumId);
			return PartialView("ArtistForAlbumRow", link);

		}

        //
        // GET: /Artist/Delete/5
 
        public ActionResult Delete(int id)
        {

			Service.Delete(id);

            return RedirectToAction("Index");

        }

    }
}
