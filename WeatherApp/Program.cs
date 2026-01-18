using System.Text.Json;
using var httpClient = new HttpClient();

var apiKey = Environment.GetEnvironmentVariable("VISUAL_CROSSING_API_KEY");

if (string.IsNullOrEmpty(apiKey)) {
  Console.WriteLine("API key not found.");
  return;
}

var weatherService = new WeatherService(httpClient, apiKey);

Console.WriteLine("Weather Console App 🌤️");

var city = ConsoleInput.ReadRequired("Enter a city: ");
var startDate = string.Empty;
var endDate = string.Empty;
var field = string.Empty;
var today = DateTime.Now.ToString("yyyy-MM-dd");

WeatherResponse? response = null;

var weatherDataType = ConsoleInput.ReadRequired(
  "What weather data do you want? (historical, current, forecast)"
);

var weatherField = ConsoleInput.ReadRequired(
  "What weather field do you want? (humidity, conditions, min temp, max temp, sunrise, sunset.)"
).ToLower().Replace(" ", "");

if (weatherDataType.ToLower() == "current") {
  response = await weatherService.GetWeatherAsync(city, today, null);
} else if (weatherDataType.ToLower() == "forecast") {
  var days = ConsoleInput.ReadRequired(
    "Enter number of days for forecast (up to 15): "
  );

  if (!int.TryParse(days, out int numDays) || numDays < 1 || numDays > 15) {
    Console.WriteLine("Invalid number of days. Please enter a number between 1 and 15.");
    return;
  }

  endDate = DateTime.Now.AddDays(numDays).ToString("yyyy-MM-dd");
  response = await weatherService.GetWeatherAsync(city, today, endDate);
} else if (weatherDataType.ToLower() == "historical") {
  startDate = ConsoleInput.ReadRequired("Enter start date (YYYY-MM-DD):");

  // endDate is optional for historical data
  Console.WriteLine("Enter end date (YYYY-MM-DD):");
  endDate = Console.ReadLine();

  response = await weatherService.GetWeatherAsync(city, startDate, endDate);
} 

try {
  if (response == null) {
    Console.WriteLine("No weather data retrieved.");
    return;
  }
  var filteredResponse = WeatherHandle.GetDailyWeatherField(response, weatherField);

  if (filteredResponse.Days == null || filteredResponse.Days.Count == 0) {
      Console.WriteLine("No data found for the specified field.");
      return;
  }

  foreach (var day in filteredResponse.Days) {
    string value = weatherField switch {
      "maxtemp" or "max temp" => day.MaxTemp.ToString(),
      "mintemp" or "min temp" => day.MinTemp.ToString(),
      "humidity" => day.Humidity.ToString(),
      "conditions" => day.Conditions ?? "N/A",
      "sunrise" => day.Sunrise ?? "N/A",
      "sunset" => day.Sunset ?? "N/A",
      _ => "Unknown field"
    };
    Console.WriteLine($"{day.Datetime}: {value}");
  }
}
catch (Exception ex) {
  Console.WriteLine($"Error: {ex.Message}");
}