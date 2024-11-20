using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    [Header("OpenWeatherMap API Settings")]
    [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";

    public enum City
    {
        Orlando,
        London,
        Tokyo,
        Cairo,
        Sydney
    }

    [Header("Weather Settings")]
    [SerializeField] private City selectedCity = City.Orlando; // Default city
    [SerializeField] private Material[] skyboxes; // Assign skyboxes in inspector
    [SerializeField] private Light sun;

    private const string apiUrl = "https://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&appid={1}";
    private City lastCity; // Track the last city to avoid redundant updates

    private void Start()
    {
        lastCity = selectedCity; // Initialize the last city
        UpdateWeather();         // Fetch weather data for the initial city
    }

    /// <summary>
    /// Updates the weather for the currently selected city.
    /// </summary>
    public void UpdateWeather()
    {
        string cityName = selectedCity.ToString(); // Convert enum to string
        string url = string.Format(apiUrl, cityName, apiKey);
        StartCoroutine(CallAPI(url, OnWeatherDataLoaded));
    }

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    private void OnWeatherDataLoaded(string data)
    {
        Debug.Log(data);

        WeatherResponse response = JsonUtility.FromJson<WeatherResponse>(data);

        AdjustScene(response);
    }

    private void AdjustScene(WeatherResponse response)
    {
        string weatherMain = response.weather[0].main.ToLower();
        Debug.Log($"Weather: {weatherMain}, Temp: {response.main.temp}°C");

        // Change skybox
        if (weatherMain.Contains("clear")) RenderSettings.skybox = skyboxes[0];
        else if (weatherMain.Contains("rain")) RenderSettings.skybox = skyboxes[1];
        else if (weatherMain.Contains("snow")) RenderSettings.skybox = skyboxes[2];
        else if (weatherMain.Contains("clouds")) RenderSettings.skybox = skyboxes[3];
        else RenderSettings.skybox = skyboxes[0]; // Default/Cloudy

        // Adjust sun properties
        if (weatherMain.Contains("night"))
        {
            sun.intensity = 0.2f;
            sun.color = Color.blue;
        }
        else
        {
            sun.intensity = 1.0f;
            sun.color = Color.white;
        }
    }

    /// <summary>
    /// Automatically trigger weather updates when the selected city changes in the Inspector.
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (selectedCity != lastCity) // Only update if the city has changed
        {
            lastCity = selectedCity; // Update the tracked city
            UpdateWeather();
        }
    }
}

[Serializable]
public class WeatherResponse
{
    public Weather[] weather;
    public Main main;
}

[Serializable]
public class Weather
{
    public string main;
    public string description;
}

[Serializable]
public class Main
{
    public float temp;
}
