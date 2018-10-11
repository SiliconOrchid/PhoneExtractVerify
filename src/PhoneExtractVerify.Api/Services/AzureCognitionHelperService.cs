using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

using PhoneExtractVerify.Api.Models;
using PhoneExtractVerify.Api.Services.Interface;

namespace PhoneExtractVerify.Api.Services
{
    public class AzureCognitionHelperService : IAzureCognitionHelperService
    {
        private AzureComputerVisionCredentials _azureComputerVisionCredentials;

        public AzureCognitionHelperService(IOptions<AzureComputerVisionCredentials> azureComputerVisionCredentialsConfiguration)
        {
            _azureComputerVisionCredentials = azureComputerVisionCredentialsConfiguration.Value ?? throw new ArgumentException(nameof(azureComputerVisionCredentialsConfiguration));
        }


        public async Task<string> ExtractPrintedText(Byte[] imageBytes)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _azureComputerVisionCredentials.SubscriptionKey);

                HttpResponseMessage response;

                using (ByteArrayContent content = new ByteArrayContent(imageBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(_azureComputerVisionCredentials.UriBase, content);
                }

                string contentString = await response.Content.ReadAsStringAsync();

                return contentString;
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return "";
            }
        }



        public async Task<string> ReadHandwrittenText(Byte[] imageBytes)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _azureComputerVisionCredentials.SubscriptionKey);
                string requestParameters = "mode=Handwritten";
                string uri = _azureComputerVisionCredentials.UriBase + "?" + requestParameters;

                HttpResponseMessage response;
                string operationLocation;


                using (ByteArrayContent content = new ByteArrayContent(imageBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                }

                if (response.IsSuccessStatusCode)
                {
                    operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                }
                else
                {
                    string errorString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n\nResponse:\n{0}\n",JToken.Parse(errorString).ToString());
                    return string.Empty;
                }


                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);

                if (i == 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
                {
                    Console.WriteLine("\nTimeout error.\n");
                    return string.Empty;
                }

                return contentString;

            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return string.Empty;
            }

        }




        public List<string> ExtractWords(string jsonResponse)
        {
            List<string> listDistinctWords = new List<string>();

            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);

            foreach (var region in jsonObj.regions)
            {
                foreach (var line in region.lines)
                {
                    foreach (var word in line.words)
                    {
                        listDistinctWords.Add(Convert.ToString(word.text));
                    }
                }
            }

            return listDistinctWords;
        }

    }
}
