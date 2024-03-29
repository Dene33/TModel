﻿using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CUE4Parse.Compression;
using Newtonsoft.Json.Linq;
using Serilog;

namespace CUE4Parse.MappingsProvider
{
    public class BenBotMappingsProvider : UsmapTypeMappingsProvider
    {
        static string MappingsFile = Path.Combine(TModel.Preferences.StorageFolder, "BenbotMappings.usmap");

        private readonly string? _specificVersion;
        private readonly string _gameName;
        private readonly bool _isWindows64Bit;
        public bool Successful;

        public BenBotMappingsProvider(string gameName, string? specificVersion = null)
        {
            _specificVersion = specificVersion;
            _gameName = gameName;
            _isWindows64Bit = Environment.Is64BitOperatingSystem && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            Successful = Reload();
        }

        public const string BenMappingsEndpoint = "https://benbot.app/api/v1/mappings";

        private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(2), DefaultRequestHeaders = { { "User-Agent", "CUE4Parse" } } };

        public sealed override bool Reload()
        {
            return ReloadAsync().GetAwaiter().GetResult();
        }

        public static Exception? FailedMappingsException;

        public sealed override async Task<bool> ReloadAsync()
        {


            byte[] usmapBytes = Array.Empty<byte>();
            string? usmapUrl = null;
            string? usmapName = null;

            try
            {
                if (!File.Exists(MappingsFile))
                {
                    Log.Information("Downloading mappings from Benbot");
                    var jsonText = _specificVersion != null
    ? await LoadEndpoint(BenMappingsEndpoint + $"?version={_specificVersion}")
    : await LoadEndpoint(BenMappingsEndpoint);
                    if (jsonText == null)
                    {
                        Log.Warning("Failed to get BenBot Mappings Endpoint");
                        return false;
                    }
                    var json = JArray.Parse(jsonText);
                    var preferredCompression = _isWindows64Bit ? "Oodle" : "Brotli";

                    if (!json.HasValues)
                    {
                        Log.Warning("Couldn't reload mappings, json array was empty");
                        return false;
                    }
                    foreach (var arrayEntry in json)
                    {
                        var method = arrayEntry["meta"]?["compressionMethod"]?.ToString();
                        if (method != null && method == preferredCompression)
                        {
                            usmapUrl = arrayEntry["url"]?.ToString();
                            usmapName = arrayEntry["fileName"]?.ToString();
                            break;
                        }
                    }

                    if (usmapUrl == null)
                    {
                        usmapUrl = json[0]["url"]?.ToString()!;
                        usmapName = json[0]["fileName"]?.ToString()!;
                    }

                    usmapBytes = await LoadEndpointBytes(usmapUrl);
                    if (usmapBytes == null)
                    {
                        Log.Warning("Failed to download usmap");
                        return false;
                    }

                    File.WriteAllBytes(MappingsFile, usmapBytes);
                }
                else
                {
                    Log.Information("Loading Mappings from existing file");
                    usmapBytes = File.ReadAllBytes(MappingsFile);
                }

                AddUsmap(usmapBytes, _gameName, usmapName!);
                return true;
            }
            catch (Exception e)
            {
                FailedMappingsException = e;
                Log.Warning(e, "Uncaught exception while reloading mappings from BenBot");
                return false;
            }
        }

        private async Task<string?> LoadEndpoint(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        private async Task<byte[]?> LoadEndpointBytes(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e)
            {
                Log.Error("Failed to download Mappings:\n" + e.ToString());
                return null;
            }
        }
    }
}