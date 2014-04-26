using System;
using System.Web.Mvc;
using Chaow.MvcRocks.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Chaow.MvcRocks.Controllers
{
    public class VideoController : Controller
    {
        //
        // GET: /Video/
        public ActionResult Index()
        {
            var context = new MovieStoreContext();
            var blob = context.video.GetBlockBlobReference("mov_bbb.mp4");
            var policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.Now.AddSeconds(30),
            };

            var model = new VideoItem
            {
                Name = blob.Name,
                Url = blob.Uri + blob.GetSharedAccessSignature(policy),
            };
            return View(model);
        }
    }
}