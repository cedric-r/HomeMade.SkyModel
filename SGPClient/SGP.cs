using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SGPClient
{
    public class SGP
    {
        private const string _SGPUrl = "http://localhost:59590/";
        private const string SgAbortImage = "json/reply/SgAbortImage";
        private const string SgCaptureImage = "json/reply/SgCaptureImage";
        private const string SgGetDeviceStatus = "json/reply/SgGetDeviceStatus";
        private const string SgGetTelescopePosition = "json/reply/SgGetTelescopePosition";

        private static string _SetupFilename = "HomeMadeSkyFlatsSGP.config";
        private static string _LogFilename = "HomeMadeSkyFlatsSGP.log";
        private static string _LocalAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HomeMadeSkyFlatsSGP");
        private static string _SetupFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _SetupFilename);
        private static string _Logile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _LogFilename);

        private Logger _Log = new Logger(_Logile) { Output = true };

        public SGP()
        {

        }

        public void AborImage()
        {
            Message(SgAbortImage, "");
        }

        public string DeviceStatus(string device)
        {
            string temp = "";

            Dictionary<string, string> request = new Dictionary<string, string>();
            request.Add("Device", device);
            string httpResponse = Message(SgGetDeviceStatus, JsonConvert.SerializeObject(request));
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(httpResponse);
            if (response["Success"] == "true")
            {
                if (response["State"] != "IDLE") throw new Exception("Device " + device + " is not idle: " + response["State"]);
                temp = response["State"];
            }
            else
            {
                throw new Exception("Error in DeviceStatus:" + response["Message"]);
            }


            return temp;
        }

        public string CaptureImage(string binningMode, string exposureLength, string gain, string iso, string speed, string frameType, string path, string useSubframe, string x , string y, string width, string height)
        {
            string temp;

            Dictionary<string, string> request = new Dictionary<string, string>();
            request.Add("BinningMode", binningMode);
            request.Add("BinningMode", exposureLength);
            request.Add("BinningMode", gain);
            request.Add("BinningMode", iso);
            request.Add("BinningMode", speed);
            request.Add("BinningMode", frameType);
            request.Add("BinningMode", path);
            request.Add("BinningMode", useSubframe);
            request.Add("BinningMode", x);
            request.Add("BinningMode", y);
            request.Add("BinningMode", width);
            request.Add("BinningMode", height);

            string httpResponse = Message(SgCaptureImage, JsonConvert.SerializeObject(request));
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(httpResponse);
            if (response["Success"] == "true")
            {
                temp = response["Receipt"];
            }
            else
            {
                throw new Exception("Error in CaptureImage:" + response["Message"]);
            }

            return temp;
        }

        public Dictionary<string, double> GetTelescopePosition()
        {
            Dictionary<string, double> temp = new Dictionary<string, double>();

            string httpResponse = Message(SgGetTelescopePosition, "{ }");
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(httpResponse);
            if (response["Success"] == "true")
            {
                temp.Add("RA", Double.Parse(response["Ra"]));
                temp.Add("DEC", Double.Parse(response["Dec"]));
            }
            else
            {
                throw new Exception("Error in GetTelescopePosition:" + response["Message"]);
            }

            return temp;
        }

        private string Message(string command, string body)
        {
            string temp = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_SGPUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpContent httpContent = new StringContent(body, Encoding.UTF8);
            HttpResponseMessage response = client.PostAsync(command, httpContent).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                temp = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
            }
            else
            {
                _Log.Log("Error during REST call: " + response.StatusCode + "\n" + response.ReasonPhrase);
            }

            client.Dispose();
            return temp;
        }

    }
}
