﻿using System.Linq;
using System.Web.Mvc;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Service;
using VocaDb.Web.Helpers;
using VocaDb.Web.Models.Admin;
using VocaDb.Model.DataContracts;

namespace VocaDb.Web.Controllers
{
    public class AdminController : ControllerBase
    {

		private CommentViewModel CreateCommentView(UnifiedCommentContract contract) {

			string name;
			string url;

			if (contract.Album != null) {
				name = contract.Album.Name;
				url = Url.Action("Details", "Album", new { id = contract.Album.Id });
			} else {
				name = contract.Artist.Name;
				url = Url.Action("Details", "Artist", new { id = contract.Artist.Id });
			}

			return new CommentViewModel(contract, name, url);
				
		}

    	private AdminService Service {
			get { return MvcApplication.Services.Admin; }
    	}

        //
        // GET: /Admin/

        public ActionResult Index()
        {

			LoginManager.VerifyPermission(PermissionFlags.ManageDatabase);

            return View();

        }

		public ActionResult GeneratePictureThumbs() {
			
			Service.GeneratePictureThumbs();

			return RedirectToAction("Index");

		}

		public ActionResult RecentComments() {

			var comments = Service.GetRecentComments();
			var models = comments.Select(c => CreateCommentView(c)).ToArray();

			return View(models);

		}

		public ActionResult RefreshDbCache() {

			var sessionFactory = MvcApplication.SessionFactory;

			var classMetadata = sessionFactory.GetAllClassMetadata();
			foreach (var ep in classMetadata.Values) {
				sessionFactory.EvictEntity(ep.EntityName);
			}
 
			var collMetadata = sessionFactory.GetAllCollectionMetadata();
			foreach (var acp in collMetadata.Values) {
				sessionFactory.EvictCollection(acp.Role);
			}

			return RedirectToAction("Index");

		}

		public ActionResult UpdateArtistStrings() {
			
			Service.UpdateArtistStrings();

			return RedirectToAction("Index");

		}

		public ActionResult UpdateEntryStatuses() {

			var count = Service.UpdateEntryStatuses();

			TempData.SetStatusMessage(count + " entries updated");

			return RedirectToAction("Index");

		}

		public ActionResult UpdateNicoIds() {
			
			Service.UpdateNicoIds();

			return RedirectToAction("Index");

		}

		public ActionResult UpdatePVIcons() {

			Service.UpdatePVIcons();

			return RedirectToAction("Index");

		}

		[Authorize]
		public ActionResult ViewAuditLog() {

			LoginManager.VerifyPermission(PermissionFlags.ViewAuditLog);

			var entries = Service.GetAuditLog();

			return View(entries);

		}

    }
}
