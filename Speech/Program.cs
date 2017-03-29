using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace Speech
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BingSpeechAPIのアクセスキーを入力してください");
            string accessKey = Console.ReadLine();
            string token = GetToken(accessKey).Result;
            SpeechRecognition(token, "ys.wav");
            Console.Read();
        }

        //Json用クラス
        public class Properties
        {
            public string requestid { get; set; }
            public string HIGHCONF { get; set; }
        }
        public class Header
        {
            public string status { get; set; }
            public string scenario { get; set; }
            public string name { get; set; }
            public string lexical { get; set; } //認識結果
            public Properties properties { get; set; }
        }
        public class RecognitionResultJson
        {
            public string version { get; set; }
            public Header header { get; set; }
            public List<Result> results { get; set; }
        }
        public class Properties2
        {
            public string HIGHCONF { get; set; }
        }
        public class Result
        {
            public string scenario { get; set; }
            public string name { get; set; }
            public string lexical { get; set; }
            public string confidence { get; set; }
            public Properties2 properties { get; set; }
        }


        //アクセストークンの作成
        static async Task<string> GetToken(string accessKey)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", accessKey);
            var uri = @"https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            using (var content = new ByteArrayContent(new byte[0]))
            {
                content.Headers.ContentLength = 0;
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        //音声認識実行
        static async Task SpeechRecognition(string token, string filename)
        {
            var client = new HttpClient();
            var query = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            query["version"] = "3.0";
            query["requestid"] = Guid.NewGuid().ToString(); //リクエスト識別GUID
            query["appID"] = "D4D52672-91D7-4C74-8AD8-42B1D98141A5"; //固定
            query["format"] = "json";
            query["locale"] = "ja-JP"; //使用言語
            query["device.os"] = "Windows OS"; //使用OS
            query["scenarios"] = "ulm"; //uml or web Search
            query["instanceid"] = Guid.NewGuid().ToString(); //デバイス識別GUID
            var uri = @"https://speech.platform.bing.com/recognize?" + query;
            try
            {
                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var byteData = new byte[fileStream.Length];
                    var readBytes = fileStream.Read(byteData, 0, (int)fileStream.Length);
                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                        var response = await client.PostAsync(uri, content);
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(responseString);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
