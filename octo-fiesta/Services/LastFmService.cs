using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OctoFiesta.Services
{
    public class LastFmService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LastFmService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<string>> GetSimilarTracksAsync(string artist, string track)
        {
            string apiKey = _configuration["LASTFM_API_KEY"] ?? Environment.GetEnvironmentVariable("LASTFM_API_KEY");
            string url = $"http://ws.audioscrobbler.com/2.0/?method=track.getsimilar&artist={Uri.EscapeDataString(artist)}&track={Uri.EscapeDataString(track)}&api_key={apiKey}&format=json";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<string>();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            var trackNames = new List<string>();
            if (doc.RootElement.TryGetProperty("similartracks", out var similarTracks) &&
                similarTracks.TryGetProperty("track", out var trackArray))
            {
                foreach (var t in trackArray.EnumerateArray())
                {
                    string name = t.GetProperty("name").GetString();
                    string artistName = t.GetProperty("artist").GetProperty("name").GetString();
                    trackNames.Add($"{artistName} - {name}");
                }
            }

            return trackNames;
        }
    }
}
