using System.Text.Json;
// make user enter city, then ask if they want historical, current or forecast weather
// if historical, ask for date range
// if forecast, ask for number of days (up to 15)
// if current, ask what field (temperature, humidity, wind speed, etc.)

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, string apiKey)
    {
      _httpClient = httpClient;
      _apiKey = apiKey;
    }

    public async Task<WeatherResponse> GetWeatherAsync(
      string city, 
      string? startDate = null, 
      string? endDate = null
      )
    {
      var url =
        $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{city}" + 
        (string.IsNullOrEmpty(startDate) ? "" : $"/{startDate}") +
        (string.IsNullOrEmpty(endDate) ? "" : $"/{endDate}") +
        $"?unitGroup=metric&contentType=json&key={_apiKey}";

      var response = await _httpClient.GetAsync(url);

      if (!response.IsSuccessStatusCode)
        throw new Exception("Failed to fetch weather data.");

      var json = await response.Content.ReadAsStringAsync();

      var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(
        json,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

      if (weatherResponse == null)
        throw new Exception("Failed to parse weather data.");
      return weatherResponse;
    }
}