using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SmartMirror
{
    public partial class MainPage
    {
        private async Task GetEventsGmail()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                new Uri("ms-appx:///Assets/client_secret.json"),
                                new[] { Uri.EscapeUriString(CalendarService.Scope.Calendar) },
                                "user",
                                CancellationToken.None);

            var calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Smart Mirror",
            });


            var calendarListResource = await calendarService.CalendarList.List().ExecuteAsync();
            string calendarId = calendarListResource.Items[0].Id;

            calendarService.Events.List(calendarId).TimeMax = DateTime.Now;
            var events = await calendarService.Events.List(calendarId).ExecuteAsync();

            string eventsText = "";

            for (int i = events.Items.Count - 1; i > 0; i--)
            {
                if (events.Items[i].Start.DateTime >= DateTime.Now)
                {
                    eventsText = eventsText + "\n" + events.Items[i].Start.DateTime + ": " + events.Items[i].Summary;
                    Debug.WriteLine(eventsText);
                }
            }

            eventsList_txt.Text = eventsText;
        }
    }
}
