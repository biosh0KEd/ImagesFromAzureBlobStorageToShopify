using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PruebaBlob.Controllers
{
    public class SubirImagenesController : AsyncController
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
        public async Task<ActionResult> UploadFiles(HttpPostedFileBase[] files)
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
                        System.Diagnostics.Debug.WriteLine(InputFileName);
                        Task.Run(() => SendImage("images", file));                        
                    }

                }
                TempData["UploadStatus"] = files.Count().ToString() + " files uploaded successfully.";
            }
            return RedirectToAction("Index");
        }   

        public async Task SendImage(string containerName, HttpPostedFileBase file)
		{            
            var blobClient = new BlobServiceClient(System.Web.Configuration.WebConfigurationManager.AppSettings["BlobConnString"]);
            var blobContainer = blobClient.GetBlobContainerClient(containerName);
            var blobResponse = await blobContainer.UploadBlobAsync(file.FileName, file.InputStream);            
            var blob = blobContainer.GetBlobClient(file.FileName);            
            SubirAShopify(blob.Uri.AbsoluteUri);         
		}

        public Task<string> generateSasToken(string connectionString, string container, string accountKey, string accountName, string url)
        {                        
            var sharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);
            var expiryDate = DateTime.Now.AddHours(2);

            BlobSasBuilder builder = new BlobSasBuilder()
			{
                BlobContainerName = container,
                ExpiresOn = expiryDate,                   
			};
            builder.SetPermissions(BlobSasPermissions.All);
            BlobSasQueryParameters sasKey = builder.ToSasQueryParameters(sharedKeyCredential);
            return Task.FromResult(sasKey.ToString());
        }

        public void SubirAShopify(string url)
		{
            

        }
    }
}