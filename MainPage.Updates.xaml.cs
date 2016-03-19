using System;
using Windows.UI.Xaml;

namespace SmartMirror
{
    public partial class MainPage
    {
        DispatcherTimer _hourTimer;
        DispatcherTimer _timeTimer;   

        public void DispatcherTimerSetup()
        {
            //Update every second
            _timeTimer = new DispatcherTimer();
            _timeTimer.Tick += dispatcherTimerTime_Tick;
            _timeTimer.Interval = TimeSpan.FromSeconds(1);

            //Update every hour
            _hourTimer = new DispatcherTimer();
            _hourTimer.Tick += dispatcherTimerHour_Tick;
            _hourTimer.Interval = TimeSpan.FromHours(1);

            if(!_timeTimer.IsEnabled)
                _timeTimer.Start();

            if (!_hourTimer.IsEnabled)
                _hourTimer.Start();
        }

        void dispatcherTimerTime_Tick(object sender, object e)
        {
            TimeUpdater();
        }

        async void dispatcherTimerHour_Tick(object sender, object e)
        {
            await GetWeather();
            if (userName == "Lilian")
                await GetEventsGmail();
        }

        void TimeUpdater()
        {
            TimeTextBlock.Text = string.Format("{0:hh:mm tt}", DateTime.Now);

            DateTime moment = DateTime.Now;
            int year = moment.Year;
            string month = moment.ToString("MMMM");
            int day = moment.Day;

            string date = moment.DayOfWeek + ", " + day + " " + month + " " + year;
            DateTextBlock.Text = date;
        }
    }
}
