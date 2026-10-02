using LocateApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LocateApp.Services
{
    public static class CallApiService
    {
        public async static Task<HttpResponseMessage> PostAsync<T>(T model, string url)
        {
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Clear();

            return await client.PostAsync(url, data);
        }

        public async static Task<HttpResponseMessage> PostAsync<T>(T model, string url, Token token)
        {
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.access_token}");

            return await client.PostAsync(url, data);
        }

        internal static Task PostAsync(object loginViewModel, string url)
        {
            throw new NotImplementedException();
        }

        public static HttpResponseMessage GetAsync(string url, Token token)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.access_token}");

            var response = client.GetAsync(url).Result;

            return response;
        }

        public static HttpResponseMessage GetAsync(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();

            var response = client.GetAsync(url).Result;

            return response;
        }
    }
}