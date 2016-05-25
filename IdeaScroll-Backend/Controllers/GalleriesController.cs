using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using IdeaScroll_Backend.Models;
//Blob
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Net.Http.Headers;
using System.Web;

namespace IdeaScroll_Backend.Controllers
{
    //[Authorize]
    public class GalleriesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // Interface in place so you can resolve with IoC container of your choice
        private readonly IBlobService _service = new BlobService();

        /// <summary>
        /// Uploads one or more blob files.
        /// </summary>
        /// <returns></returns>
        [Route("PostIdeaPic")]
        public async Task<IHttpActionResult> PostIdeaPic(int IdeaId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            try
            {
                // This endpoint only supports multipart form data
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                {
                    return StatusCode(HttpStatusCode.UnsupportedMediaType);
                }

                // Call service to perform upload, then check result to return as content
                var result = await _service.UploadBlobs(Request.Content);
                if (result != null && result.Count > 0)
                {


                    string picture = "";
                    var takeURI = result.Select(i =>

                        picture = i.FileUrl

                    ).SingleOrDefault();
                    var addPhoto = new Gallery() { Visibility = true, ImgUri = takeURI, IdeaId = IdeaId };

                    db.Gallery.Add(addPhoto);

                    await db.SaveChangesAsync();


                    return Ok(result);
                }

                // Otherwise
                return BadRequest();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Downloads a blob file.
        /// </summary>
        /// <param name="blobId">The ID of the blob.</param>
        /// <returns></returns>
        [Route("GetPicture")]
        public async Task<HttpResponseMessage> GetBlobDownload(int blobId)
        {
            // IMPORTANT: This must return HttpResponseMessage instead of IHttpActionResult

            try
            {
                var result = await _service.DownloadBlob(blobId);
                if (result == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                // Reset the stream position; otherwise, download will not work
                result.BlobStream.Position = 0;

                // Create response message with blob stream as its content
                var message = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(result.BlobStream)
                };

                // Set content headers
                message.Content.Headers.ContentLength = result.BlobLength;
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(result.BlobContentType);
                message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = HttpUtility.UrlDecode(result.BlobFileName),
                    Size = result.BlobLength
                };

                return message;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        //Query
        [Route("GetPictureList")]
        public async Task<IHttpActionResult> GetPicList(int IdeaId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            var query = await db.Gallery.Select(i =>
            new
            {
                ImgUri = i.ImgUri,
                Id = i.Id,
                IdeaId = i.IdeaId,
                Visibility = i.Visibility,
                Name=i.Name
            }).Where(x => x.IdeaId == IdeaId && x.Visibility == true).OrderByDescending(y=>y.Id).ToListAsync();
            return Ok(query);
            
        }
        [Route("DeletePicture")]
        public async Task<IHttpActionResult> PutDeletePic(int PictureId)
        {

            try
            {
                //check end date passed
                var getIdea = await db.Gallery.Where(x => x.Id == PictureId).SingleAsync();

                getIdea.Visibility = false;

                await db.SaveChangesAsync();
                return Ok("delete");
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

    }
    public interface IBlobService
    {
        Task<List<Pictures>> UploadBlobs(HttpContent httpContent);
        Task<PicDownloadModel> DownloadBlob(int blobId);
    }

    public class BlobService : IBlobService
    {
        public async Task<List<Pictures>> UploadBlobs(HttpContent httpContent)
        {
            var blobUploadProvider = new BlobStorageUploadProvider();

            var list = await httpContent.ReadAsMultipartAsync(blobUploadProvider)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        throw task.Exception;
                    }

                    var provider = task.Result;
                    return provider.Uploads.ToList();
                });

            // TODO: Use data in the list to store blob info in your
            // database so that you can always retrieve it later.

            return list;
        }

        public async Task<PicDownloadModel> DownloadBlob(int blobId)
        {
            // TODO: You must implement this helper method. It should retrieve blob info
            // from your database, based on the blobId. The record should contain the
            // blobName, which you should return as the result of this helper method.

            string blobName = GetBlobName(blobId);

            if (!String.IsNullOrEmpty(blobName))
            {
                var container = BlobHelper.GetBlobContainer();
                var blob = container.GetBlockBlobReference(blobName);

                // Download the blob into a memory stream. Notice that we're not putting the memory
                // stream in a using statement. This is because we need the stream to be open for the
                // API controller in order for the file to actually be downloadable. The closing and
                // disposing of the stream is handled by the Web API framework.
                var ms = new MemoryStream();
                await blob.DownloadToStreamAsync(ms);

                // Strip off any folder structure so the file name is just the file name
                var lastPos = blob.Name.LastIndexOf('/');
                var fileName = blob.Name.Substring(lastPos + 1, blob.Name.Length - lastPos - 1);

                // Build and return the download model with the blob stream and its relevant info
                var download = new PicDownloadModel
                {
                    BlobStream = ms,
                    BlobFileName = fileName,
                    BlobLength = blob.Properties.Length,
                    BlobContentType = blob.Properties.ContentType
                };

                return download;
            }

            // Otherwise
            return null;
        }
        private ApplicationDbContext db = new ApplicationDbContext();
        private string GetBlobName(int blobId)
        {

            var Accept = db.Pictures.Select(i =>
            new
            {
                Id = i.Id,
                BlobName = i.FileName

            })
              .Where(x => (x.Id == blobId)).SingleOrDefault();
            return Accept.BlobName;
        }
    }
    public static class BlobHelper
    {
        public static CloudBlobContainer GetBlobContainer()
        {
            // Pull these from config
            var blobStorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            var blobStorageContainerName = "ideabox";

            // Create blob client and return reference to the container
            var blobStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(blobStorageConnectionString);
            var blobClient = blobStorageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(blobStorageContainerName);
        }
    }
    public class BlobStorageUploadProvider : MultipartFileStreamProvider
    {
        public List<Pictures> Uploads { get; set; }

        public BlobStorageUploadProvider()
            : base(Path.GetTempPath())
        {
            Uploads = new List<Pictures>();
        }

        public override Task ExecutePostProcessingAsync()
        {
            // NOTE: FileData is a property of MultipartFileStreamProvider and is a list of multipart
            // files that have been uploaded and saved to disk in the Path.GetTempPath() location.

            foreach (var fileData in FileData)
            {
                // Sometimes the filename has a leading and trailing double-quote character
                // when uploaded, so we trim it; otherwise, we get an illegal character exception
                var fileName = Path.GetFileName(fileData.Headers.ContentDisposition.FileName.Trim('"'));

                // Retrieve reference to a blob
                var blobContainer = BlobHelper.GetBlobContainer();
                var blob = blobContainer.GetBlockBlobReference(fileName);

                // Set the blob content type
                blob.Properties.ContentType = fileData.Headers.ContentType.MediaType;

                // Upload file into blob storage, basically copying it from local disk into Azure
                using (var fs = File.OpenRead(fileData.LocalFileName))
                {
                    blob.UploadFromStream(fs);
                }

                // Delete local file from disk
                File.Delete(fileData.LocalFileName);

                // Create blob upload model with properties from blob info
                var blobUpload = new Pictures
                {
                    FileName = blob.Name,
                    FileUrl = blob.Uri.AbsoluteUri,
                    FileSizeInBytes = blob.Properties.Length
                };

                // Add uploaded blob to the list
                Uploads.Add(blobUpload);
            }

            return base.ExecutePostProcessingAsync();
        }
    }
}