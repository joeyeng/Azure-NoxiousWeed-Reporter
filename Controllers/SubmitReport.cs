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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using NoxiousWeedsWebApp.Models;

namespace NoxiousWeedsWebApp.Controllers
{
    public class SubmitReportRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Tag { get; set; }
        public long Probability { get; set; }
        public string ImageUrl { get; set; }
    }

    public class SubmitReport : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post(SubmitReportRequest request)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid subscription key.
            //client.DefaultRequestHeaders.Add("Prediction-Key", "45874fe0badb45b38bd77392e1fbf974");

            // Prediction URL - replace this example URL with your valid prediction URL.
            string url = "";

            HttpResponseMessage response;

            var content = new StringContent(JsonConvert.SerializeObject(request));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(url, content);

            return View();
            //return JsonConvert.DeserializeObject<ImagePredictionResultModel>(await response.Content.ReadAsStringAsync());
        }
    }
}