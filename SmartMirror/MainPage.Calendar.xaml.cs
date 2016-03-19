using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace SmartMirror
{
    public partial class MainPage
    {
        private async Task GetEvents()
        {
            //ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010);
            //service.Credentials = new WebCredentials("likasem@microsoft.com", "dodge*93");
            //service.UseDefaultCredentials = false;
            //service.AutodiscoverUrl("likasem@microsoft.com", RedirectionUrlValidationCallback);
            //EmailMessage email = new EmailMessage(service);
            //email.ToRecipients.Add("liliankasem@gmail.com");
            //email.Subject = "HelloWorld";
            //email.Body = new MessageBody("This is the first email I've sent by using the EWS Managed API.");
            //email.Send();

            /*
            v2 authentication endpoints
            https://login.microsoftonline.com/common/oauth2/v2.0/authorize
            https://login.microsoftonline.com/common/oauth2/v2.0/token
            */


           // var data = "";
            try
            {
                var http = new HttpClient();
                var url = String.Format("https://outlook.office.com/api/v2.0/me/calendarview?startDateTime=2016-02-29T01:00:00&endDateTime=2014-03-04T23:00:00&$select=Subject,Location,Start");

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/*"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSIsImtpZCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSJ9");
                var response = await http.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                //RootObject data = new RootObject();
                //JObject jObject = JObject.Parse(result);

                //data = JsonConvert.DeserializeObject<RootObject>(result);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

        }
    }
}
