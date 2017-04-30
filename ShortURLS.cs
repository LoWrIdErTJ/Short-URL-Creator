using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using UBotPlugin;
using System.Linq;
using System.Windows;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Security.Cryptography;
using System.Configuration;
using System.Media;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Data.OleDb;

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
    // GET SOME PROXYS FROM URL - YUM
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
            xService.Options = new[] { "", "TinyURL", "Google", "Bit.ly" };
            _parameters.Add(xService);
        }

        public string Category
        {
            get { return "System Functions"; }
        }

        public string FunctionName
        {
            get { return "$shorten url"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            string myUrl = parameters["Http URL"];
            string serviceToUse = parameters["Service?"];

            if (serviceToUse == "TinyURL")
            {
                string new_url = ShortUrlTiny(myUrl);
                _returnValue = new_url;
            }
            else if (serviceToUse == "Google")
            {
                string new_url = GShorten(myUrl);
                _returnValue = new_url;
            }
            else if (serviceToUse == "Bit.ly")
            {
                string new_url = ShortUrlBitly(myUrl);
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

        private const string code = "R_f483d44720cb719c52832a00c9da4200";
        public static String ShortUrlBitly(String inURL)
        {
            String shortURL = "";
            String queryURL = "http://api.bit.ly/shorten?version=2.0.1&login=o_7ol2fr5vda&apiKey=" + code + "&longUrl=" + inURL;

            HttpWebRequest request = WebRequest.Create(queryURL) as HttpWebRequest;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                String jsonResults = reader.ReadToEnd();
                int indexOfBefore = jsonResults.IndexOf("shortUrl\": \"") + 12;
                int indexOfAfter = jsonResults.IndexOf("\"", indexOfBefore);
                shortURL = jsonResults.Substring(indexOfBefore, indexOfAfter - indexOfBefore);
            }

            return shortURL;
        }


        private const string gkey = "AIzaSyBW1WVGwj6GFdJlDgB-FZBrQqy6kk7kopg";

        public static string GShorten(string url)
        {
            string post = "{\"longUrl\": \"" + url + "\"}";
            string shortUrl = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + gkey);
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
                            shortUrl = Regex.Match(json, @"""id"": ?""(?<id>.+)""").Groups["id"].Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if Google's URL Shortner is down...
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            return shortUrl;
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
