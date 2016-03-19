using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartMirror
{
    public partial class MainPage
    {
        async Task GetWeather()
        {
            var position = await LocationManager.GetPosition();

            var lat = position.Coordinate.Point.Position.Latitude;
            var lon = position.Coordinate.Point.Position.Longitude;

            RootObject myWeather = await OpenWeatherMapProxy.GetWeather(lat, lon);

            string icon = String.Format("ms-appx:///Assets/Weather/{0}.png", myWeather.weather[0].icon);

            TempTextBlock.Text = ((int)myWeather.main.temp).ToString() + "°C";
            DescTextBlock.Text = myWeather.weather[0].description;
            LocationTextBlock.Text = myWeather.name;

            ResultImage.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
        }

    }
}
