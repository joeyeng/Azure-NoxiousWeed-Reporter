using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using NoxiousWeedsWebApp.Models;

namespace NoxiousWeedsWebApp.Controllers
{
    public class SubmitPhotoController : Controller
    {
        private readonly AppSettings _appSettings;
        private PredictionEndpoint _endpoint;

        public SubmitPhotoController(IOptions<AppSettings> options)
        {
            _appSettings = options.Value;
            _endpoint = new PredictionEndpoint { ApiKey = _appSettings.PredictionEndpointApiKey };
        }

        [HttpPost]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Position = 0;
                        formFile.CopyTo(stream);

                        string blobName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(formFile.FileName)}";
                        var blockBlob = GetBlockBlob("weedphotos", blobName);
                        stream.Position = 0;
                        await blockBlob.UploadFromStreamAsync(stream);

                        var predictionResult = await MakePredictionRequest(stream.ToArray());

                        var topResult = predictionResult
                            .Predictions
                            .Where(p => p.Probability >= _appSettings.ProbabilityThreshold)
                            .FirstOrDefault();

                        if (topResult != null)
                        {
                            return View(new PredictionResponse { Message = "Success", ImageUrl = blockBlob.Uri.ToString(), Probability = topResult.Probability, Tag = topResult.Tag });
                        }
                        else
                        {
                            return View(new PredictionResponse { Message = "Could not find match." });
                        }
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return BadRequest(new { message = "Bad request" });
        }


        private async Task<ImagePredictionResultModel> MakePredictionRequest(byte[] byteData)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid subscription key.
            client.DefaultRequestHeaders.Add("Prediction-Key", _appSettings.PredictionEndpointApiKey);

            // Prediction URL - replace this example URL with your valid prediction URL.
            string url = "";

            HttpResponseMessage response;

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                return JsonConvert.DeserializeObject<ImagePredictionResultModel>(await response.Content.ReadAsStringAsync());
            }
        }

        public CloudBlockBlob GetBlockBlob(string containerName, string blobName)
        {
            CloudBlobContainer container = GetBlobContainer(containerName);

            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            return blockBlob;
        }

        private CloudBlobContainer GetBlobContainer(string containerName)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_appSettings.StorageAccountConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            return blobClient.GetContainerReference(containerName);
        }
    }
}