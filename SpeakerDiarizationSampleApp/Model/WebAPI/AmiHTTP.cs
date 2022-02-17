using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SpeakerDiarizationSampleApp
{
    /*
     * References
     * https://briangrinstead.com/blog/multipart-form-post-in-c/
     * http://surferonwww.info/BlogEngine/post/2019/08/11/file-upload-by-using-httpclient.aspx
     * https://iwasiman.hatenablog.com/entry/20210622-CSharp-HttpClient
     * https://husk.hatenablog.com/entry/2018/07/24/231738
     */
    public class AmiHTTP
    {
        // -> const
        private const string urlPath = "https://acp-api-async.amivoice.com/v1/recognitions";
        private static readonly Encoding encoding = Encoding.UTF8;

        // 
        private static HttpClient _client = new(); // 枯渇対策

        // 
        public class AmiHTTPResult
        {
            public string success = null;
            public string error = null;
        }

        // -> POST (音声認識のリクエスト)
        public static async Task RequestSpeechRecog(string filePath, string appKey, string dValue, Action<AmiHTTPResult> action)
        {

            // -> make content
            using MultipartFormDataContent content = new();

            // -> u
            content.Add(new StringContent(appKey, encoding), "\"u\"");
            // -> d
            content.Add(new StringContent(dValue, encoding), "\"d\"");
            // -> a
            //content.Add(new StringContent(wavStr, Encoding.Unicode, "application/octet-stream"), "\"a\""); // -> この送り方は間違い
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            StreamContent streamContent = new(fs);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"a\"",
            };
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            content.Add(streamContent);

            HttpRequestMessage request = new(HttpMethod.Post, urlPath);
            request.Content = content;

            //Debug.WriteLine(request.Content.ReadAsStringAsync().Result); // requestの文字化

            await DoRequest(request, action);
        }

        // -> GET (ジョブの取得)
        public static async Task GetJobState(string session_id, string appKey, Action<AmiHTTPResult> action)
        {
            string path_ = urlPath + "/" + session_id;

            HttpRequestMessage request = new(HttpMethod.Get, path_);
            request.Headers.Add("Authorization", "Bearer " + appKey);

            await DoRequest(request, action);
        }

        // -> Request
        private static async Task DoRequest(HttpRequestMessage request, Action<AmiHTTPResult> action)
        {
            HttpStatusCode statusCoode = HttpStatusCode.NotFound;
            string bodyStr;
            AmiHTTPResult result = new();

            try
            {
                HttpResponseMessage response = await _client.SendAsync(request);
                bodyStr = await response.Content.ReadAsStringAsync();
                statusCoode = response.StatusCode;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("e: " + e.Message);
                result.error = "ERROR: " + e.Message;
                action(result);
                return;
            }

            if (!statusCoode.Equals(HttpStatusCode.OK))
            {
                result.error = "ERROR: StatusCode = " + statusCoode;
                action(result);
                return;
            }

            if (string.IsNullOrEmpty(bodyStr))
            {
                result.error = "ERROR: Response body is empty";
                action(result);
                return;
            }

            result.success = bodyStr;
            action(result);
        }
    }
}
