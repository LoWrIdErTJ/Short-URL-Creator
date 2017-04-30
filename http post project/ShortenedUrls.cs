using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UBotPlugin;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSVtoHTML
{

    // API KEY HERE
    public class PluginInfo
    {
        public static string HashCode { get { return "b2532f63b4326d9809d03cf152d8a5d9ba7d6aa4"; } }
    }

    // ---------------------------------------------------------------------------------------------------------- //
    //
    // ---------------------------------               FUNCTIONS              ----------------------------------- //
    //
    // ---------------------------------------------------------------------------------------------------------- //


    //
    //
    // GET new urls
    //
    //
    public class ShortUrlsn : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public ShortUrlsn()
        {
            _parameters.Add(new UBotParameterDefinition("Http URL", UBotType.String));
            var xService = new UBotParameterDefinition("Service?", UBotType.String);
            xService.Options = new[] { "", "Tinyurl", "Google", "Bitly", "Owly", "Isgd" };
            _parameters.Add(xService);
            _parameters.Add(new UBotParameterDefinition("Api Key", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Login name", UBotType.String));
            
        }

        public string Category
        {
            get { return "Text Functions"; }
        }

        public string FunctionName
        {
            get { return "$shorten url"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            string myUrl = parameters["Http URL"];
            string serviceToUse = parameters["Service?"];
            string apkey = parameters["Api Key"];
            string lname = parameters["Login name"];

            if (serviceToUse == "Tinyurl")
            {
                string new_url = ShortUrlTiny(myUrl);
                _returnValue = new_url;
            }
            else if (serviceToUse == "Google")
            {
                string new_url = GShorten(myUrl, apkey);
                _returnValue = new_url;
            }
            else if (serviceToUse == "Bitly")
            {
                string new_url = ShortenUrlBitly(myUrl, lname, apkey);
                _returnValue = new_url;
            }
            //else if (serviceToUse == "Tinycc")
            //{
            //    string new_url = ShortUrltinycc(myUrl, apkey, lname);
            //    _returnValue = new_url;
            //}
            else if (serviceToUse == "Owly")
            {
                string new_url = ShortUrlowly(myUrl, apkey);
                _returnValue = new_url;
            }
            else if (serviceToUse == "Isgd")
            {
                string new_url = ShortUrlisgd(myUrl);
                _returnValue = new_url;
            }
            else
            {
                string new_url = ShortUrlTiny(myUrl);
                _returnValue = new_url;
            }

        }

        public string ShortUrlTiny(string url)
        {
            WebRequest request = WebRequest.Create(string.Format("http://tinyurl.com/api-create.php?url={0}", url));
            Stream stream = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();

        }

        public string ShortUrlowly(string url, string apiKey)
        {
            WebRequest request = WebRequest.Create(string.Format("http://ow.ly/api/1.1/url/shorten?apiKey={1}&longUrl={0}", url, apiKey));
            Stream stream = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();

        }

        public string ShortUrltinycc(string url, string apiKey, string name)
        {
            string newurl = System.Uri.EscapeDataString(url);
            WebRequest request = WebRequest.Create(string.Format("http://tiny.cc/?c=rest_api&m=shorten&version=2.0.3&format=json&longUrl={0}&login={2}&apiKey={1}", newurl, apiKey, name));
            Stream stream = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            string theresponse = reader.ReadLine();

            //var json_serializer = new JavaScriptSerializer();
            //var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject("{ \"test\":\"some data\" }");
            //Console.WriteLine(routes_list["test"]);

            return theresponse;

        }


        public string ShortUrlisgd(string url)
        {
            string newurl = System.Uri.EscapeDataString(url); 
            WebRequest request = WebRequest.Create(string.Format("http://is.gd/create.php?format=simple&url={0}", newurl));
            Stream stream = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();

        }


        public static string GShorten(string url, string apiKey)
        {
            string finalURL = "";
            string post = "{\"longUrl\": \"" + url + "\"}";
            string shortUrl = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + apiKey);
            try
            {
                request.ServicePoint.Expect100Continue = false;
                request.Method = "POST";
                request.ContentLength = post.Length;
                request.ContentType = "application/json";
                request.Headers.Add("Cache-Control", "no-cache");
                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] postBuffer = Encoding.ASCII.GetBytes(post);
                    requestStream.Write(postBuffer, 0, postBuffer.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(responseStream))
                        {
                            string json = responseReader.ReadToEnd();
                            string jsonZ = JsonConvert.SerializeObject(json);
                            dynamic data = JObject.Parse(json);

                            finalURL = data.id.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if Google's URL Shortener is down...
                return ex.Message.ToString();
                //return ex.StackTrace.ToString();
            }
            return finalURL;
        }
        

        public string ShortenUrlBitly(string url, string name, string apiKey)//, Format format = Format.XML, Domain domain = Domain.BITLY
        {
            WebRequest request = WebRequest.Create(string.Format("http://api.bitly.com/v3/shorten/?login=" + name + "&apiKey=" + apiKey + "&longUrl=" + url + "&format=txt"));
            Stream stream = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();
        }
        
        
        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

}
