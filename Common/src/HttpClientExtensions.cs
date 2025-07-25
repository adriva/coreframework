using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Provides extension methods to help working with HttpClient class.
    /// </summary>
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient httpClient, string url, Action<HttpRequestHeaders> configureHeaders = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, url))
            {
                configureHeaders?.Invoke(request.Headers);
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                return response;
            }
        }

        public static async Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string url, Action<HttpRequestHeaders> configureHeaders = null)
        {
            return await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, configureHeaders);
        }

        public static async Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string url, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, Action<HttpRequestHeaders> configureHeaders = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                configureHeaders?.Invoke(request.Headers);
                var response = await httpClient.SendAsync(request, completionOption);
                return response;
            }
        }

        public static async Task<TData> GetJsonAsync<TData>(this HttpClient httpClient, string url, Action<HttpRequestHeaders> configureHeaders = null)
        {
            return await httpClient.GetJsonAsync<TData>(url, HttpCompletionOption.ResponseHeadersRead, configureHeaders);
        }

        public static async Task<TData> GetJsonAsync<TData>(this HttpClient httpClient, string url, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, Action<HttpRequestHeaders> configureHeaders = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                configureHeaders?.Invoke(request.Headers);
                var response = await httpClient.SendAsync(request, completionOption);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                return Utilities.SafeDeserialize<TData>(json);
            }
        }

        public static async Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, string url, HttpContent data, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, Action<HttpRequestHeaders> configureHeaders = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                configureHeaders?.Invoke(request.Headers);
                request.Content = data;
                var response = await httpClient.SendAsync(request, completionOption);
                return response;
            }
        }

        public static async Task<HttpResponseMessage> PostJsonAsync<TData>(this HttpClient httpClient, string url, TData data, Action<HttpRequestHeaders> configureHeaders = null)
        {
            return await httpClient.PostJsonAsync<TData>(url, data, HttpCompletionOption.ResponseHeadersRead, configureHeaders);
        }

        public static async Task<HttpResponseMessage> PostJsonAsync<TData>(this HttpClient httpClient, string url, TData data, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, Action<HttpRequestHeaders> configureHeaders = null)
        {
            string json = Utilities.SafeSerialize(data);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                configureHeaders?.Invoke(request.Headers);
                request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
                var response = await httpClient.SendAsync(request, completionOption);
                return response;
            }
        }
    }
}