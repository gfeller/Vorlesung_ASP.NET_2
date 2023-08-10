using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace BmiRechner.Test
{
    public class Test
    {
        private readonly SUT _sut;

        public Test()
        {
            _sut = new SUT();
        }


        [Fact]
        public async void TestBmi()
        {
            KeyValuePair<string, string>[] bmiData = new[]
            {
                new KeyValuePair<string, string>("weight", "150"),
                new KeyValuePair<string, string>("height","150"),
            };

            var result = await _sut.RequestHandler("/api/bmi", HttpMethod.Get, bmiData);
            var resultValue = await result.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("66.67", resultValue);
        }


        [Fact]
        public async void TestBmiWrong()
        {
            KeyValuePair<string, string>[] bmiData = new[]
            {
                new KeyValuePair<string, string>("weight", "10"),
                new KeyValuePair<string, string>("height","10"),
            };

            var result = await _sut.RequestHandler("/api/bmi", HttpMethod.Get, bmiData);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode); 
        }
    }


    public class SUT
    {
        internal TestServer Server;
        internal HttpClient Client;


        public SUT()
        {
            var factory = new WebApplicationFactory<Program>();

            Client = factory.CreateClient();
            Server = factory.Server;
        }


        public async Task<HttpResponseMessage> Request(string url, HttpMethod method = null, HttpContent content = null)
        {
            method = method ?? HttpMethod.Post;

            var request = new HttpRequestMessage(method, url) {Content = content};
            return await Client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> RequestHandler(string url, HttpMethod method = null, KeyValuePair<string, string>[] data = null)
        {
            method = method ?? HttpMethod.Post;
           
            HttpContent content = null;
            if (data != null)
            {
                if (method == HttpMethod.Post)
                {
                    content = new FormUrlEncodedContent(data);
                }
                else
                {
                    url += "?"+ String.Join("", data.Select(x => $"&{x.Key}={x.Value}&").ToArray());
                }
            }

            return await Request(url, method, content);
        }
    }
}