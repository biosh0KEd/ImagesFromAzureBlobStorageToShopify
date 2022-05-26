using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Newtonsoft.Json.Linq;
using OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
                        Task.Run(() => SendImage("imagenes", file));                        
                    }

                }
                TempData["UploadStatus"] = files.Count().ToString() + " files uploaded successfully.";
            }
            return RedirectToAction("Index");
        }   

        public async Task SendImage(string containerName, HttpPostedFileBase file)
		{
            try
            {
                var blobClient = new BlobServiceClient(System.Web.Configuration.WebConfigurationManager.AppSettings["BlobConnString"]);                                
                var blobContainer = blobClient.GetBlobContainerClient(containerName);
                var blob = blobContainer.GetBlobClient(file.FileName);              
                var responseUpload = await blob.UploadAsync(file.InputStream, new BlobHttpHeaders { ContentType = file.ContentType});
                System.Diagnostics.Debug.WriteLine("_________________________Result metadata_________________________________");
                System.Diagnostics.Debug.WriteLine(responseUpload.Value);
                System.Diagnostics.Debug.WriteLine("_________________________________________________________________________");
                bool respuetaPasoAEcommerce = await SubirAShopify(blob.Uri.AbsoluteUri);
                if (respuetaPasoAEcommerce)
				{
                    blob.Delete();
				}

            }
            catch (Exception ex)
			{
                System.Diagnostics.Debug.WriteLine("______________________Exception__________________");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Source);
            }
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

        public async Task<bool> SubirAShopify(string url)
		{
            try
			{
                System.Diagnostics.Debug.WriteLine(url);
                JObject imagen = new JObject();
                imagen["image"] = new JObject(new JProperty("src", url));
                HttpClient client = new HttpClient();
                System.Diagnostics.Debug.WriteLine(imagen.ToString());
                var content = new StringContent(imagen.ToString(), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", System.Web.Configuration.WebConfigurationManager.AppSettings["ShopifyPassword"]);
                var response = await client.PostAsync("https://pruebasefraryc.myshopify.com/admin/api/2022-01/products/7390649450664/images.json", content);
                System.Diagnostics.Debug.WriteLine("______________________Respuesta__________________");
                System.Diagnostics.Debug.WriteLine(response.StatusCode);
                System.Diagnostics.Debug.WriteLine(response.ReasonPhrase);
                System.Diagnostics.Debug.WriteLine(response.Content);
                System.Diagnostics.Debug.WriteLine(response.Headers);
                System.Diagnostics.Debug.WriteLine("_____________________RequestMessage______________");
                System.Diagnostics.Debug.WriteLine(response.RequestMessage);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
                    return true;
				}
                else
				{
                    return false;
				}
            }            
            catch(HttpException ex)
			{
                System.Diagnostics.Debug.WriteLine("______________________Exception__________________");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Source);
                return false;
            }
        }

        public string Base64Encode(string textToEncode)
        {
            byte[] textAsBytes = Encoding.UTF8.GetBytes(textToEncode);
            return Convert.ToBase64String(textAsBytes);
        }

        public async Task<bool> SubirAWoocommerce(string url)
		{
            try
			{
                System.Diagnostics.Debug.WriteLine(url);
                JObject imagen = new JObject();
                imagen["id"] = "1022";
                JArray jArray = new JArray();
                jArray.Add(new JObject(new JProperty("src", url)));
                imagen["images"] = jArray;
                HttpClient client = new HttpClient();               
                OAuthRequest oAuthRequest = new OAuthRequest
                {
                    Method = "POST",
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ConsumerKey = System.Web.Configuration.WebConfigurationManager.AppSettings["WoocommerceConsumerKey"],
                    ConsumerSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["WoocommerceConsumerSecret"],
                    Version = "1.0",
                    RequestUrl = "http://localhost:8080/tiendawoo/wp-json/wc/v3/products/batch",
                };
                string oAuth1 = oAuthRequest.GetAuthorizationHeader();
                client.DefaultRequestHeaders.Add("Authorization", oAuth1);
                JArray json = new JArray();
                json.Add(imagen);
                JObject jsonf = new JObject(new JProperty("update", json));
                var content = new StringContent(jsonf.ToString(), Encoding.UTF8, "application/json");
                System.Diagnostics.Debug.WriteLine(jsonf.ToString());
                var response = await client.PostAsync("http://localhost:8080/tiendawoo/wp-json/wc/v3/products/batch", content);
                System.Diagnostics.Debug.WriteLine("______________________Respuesta__________________");
                System.Diagnostics.Debug.WriteLine(response.StatusCode);
                System.Diagnostics.Debug.WriteLine(response.ReasonPhrase);
                System.Diagnostics.Debug.WriteLine(response.Content);
                System.Diagnostics.Debug.WriteLine(response.Headers);
                System.Diagnostics.Debug.WriteLine("_____________________RequestMessage______________");
                System.Diagnostics.Debug.WriteLine(response.RequestMessage);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
			{
                System.Diagnostics.Debug.WriteLine("______________________Exception__________________");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Source);
                return false;
            }
		}
    }
}