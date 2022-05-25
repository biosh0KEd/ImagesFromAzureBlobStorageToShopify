using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PruebaBlob.Controllers
{
    public class SubirImagenesController : Controller
    {
        // GET: SubirImagenes
        public ActionResult Index()
        {
            if (TempData.ContainsKey("UploadStatus"))
			{
                ViewBag.UploadStatus = TempData["UploadStatus"];

            }
            return View();
        }

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase[] files)
        {

            //Ensure model state is valid  
            if (ModelState.IsValid)
            {   //iterating through multiple file collection   
                foreach (HttpPostedFileBase file in files)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        var InputFileName = Path.GetFileName(file.FileName);
                        Console.WriteLine(InputFileName);
                        TempData["UploadStatus"] = files.Count().ToString() + " files uploaded successfully.";

                    }

                }
            }
            return RedirectToAction("Index");
        }   
        
    }
}