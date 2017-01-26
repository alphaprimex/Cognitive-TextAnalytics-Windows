﻿using Microsoft.ProjectOxford.Text.Core;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ProjectOxford.Text.Topic
{
    /// <summary>
    /// Client for interacting with the Text Analytics topic detection API.
    /// </summary>
    /// <seealso cref="Microsoft.ProjectOxford.Text.Core.TextClient" />
    public class TopicClient : TextClient
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicClient"/> class.
        /// </summary>
        /// <param name="apiKey">The Text Analytics API key.</param>
        public TopicClient(string apiKey) : base(apiKey)
        {
            this.Url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/topics";
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Starts processing a collection for topics.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The URL to check processing status.</returns>
        public string StartTopicProcessing(TopicRequest request)
        {
            return StartTopicProcessingAsync(request).Result;
        }

        /// <summary>
        /// Starts processing a collection for topics asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The URL to check processing status.</returns>
        public async Task<string> StartTopicProcessingAsync(TopicRequest request)
        {
            var reqJson = JsonConvert.SerializeObject(request);
            byte[] reqData = Encoding.UTF8.GetBytes(reqJson);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.Url);
            req.Headers.Add("Ocp-Apim-Subscription-Key", this.ApiKey);
            req.ContentType = "application/json";
            req.Accept = "application/json";
            req.ContentLength = reqData.Length;
            req.Method = "POST";

            var reqStream = await req.GetRequestStreamAsync();
            reqStream.Write(reqData, 0, reqData.Length);
            reqStream.Close();

            //// analyze the text
            var response = await req.GetResponseAsync();

            var operationUrl = response.Headers["Operation-Location"];

            return operationUrl;
        }

        /// <summary>
        /// Gets the topics for a collection.
        /// </summary>
        /// <param name="operationUrl">The operation URL.</param>
        /// <returns></returns>
        public TopicResponse GetTopicResponse(string operationUrl)
        {
            return GetTopicResponseAsync(operationUrl).Result;
        }

        /// <summary>
        /// Gets the topics for a collection asynchronously.
        /// </summary>
        /// <param name="operationUrl">The operation URL.</param>
        /// <returns></returns>
        public async Task<TopicResponse> GetTopicResponseAsync(string operationUrl)
        {
            var doneProcessing = false;

            TopicResponse result = null;

            while (!doneProcessing)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(operationUrl);
                request.Headers.Add("Ocp-Apim-Subscription-Key", this.ApiKey);
                request.Method = "GET";

                var response = await request.GetResponseAsync();
                var responseStream =  response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                var responseData = await reader.ReadToEndAsync();
                reader.Close();
                response.Close();

                var topicResponse = JsonConvert.DeserializeObject<TopicResponse>(responseData);

                if (topicResponse.Status == "Succeeded")
                {
                    result = topicResponse;
                    doneProcessing = true;
                }

                System.Threading.Thread.Sleep(60000);
            }

            return result;
        }

        #endregion Methods
    }
}