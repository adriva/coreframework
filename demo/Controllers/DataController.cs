using System.Net.Http;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace demo.Controllers
{
    public class DataController : Controller
    {
        private readonly HttpClient HttpClient;
        private readonly ICache Cache;

        public DataController(IHttpClientFactory httpClientFactory, ICache cache)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.Cache = cache;
        }

        private async Task<string> CallTestApiAsync(string url)
        {
            return await this.Cache.GetOrCreateAsync(url, async (entry) =>
            {
                return await this.HttpClient.GetStringAsync(url);
            });
        }

        public async Task<IActionResult> Albums()
        {
            string json = await this.CallTestApiAsync("https://jsonplaceholder.typicode.com/albums");
            return this.Content(json, "application/json");
        }
    }
}