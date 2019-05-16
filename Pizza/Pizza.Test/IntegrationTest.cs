using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Pizza.Data;
using Pizza.Models;
using Pizza.Services;
using Pizza.ViewModels;
using Xunit;

namespace Pizza.Test
{
    public class IntegrationTest
    {
        private readonly SUT _sut;

        public IntegrationTest()
        {
            _sut = new SUT();
        }

        [Fact]
        public async Task<Task> CreateOrder()
        {
            var wrongOrder = JsonConvert.SerializeObject(new NewOrderViewModel() {Name = "X"});
            var okOrder = JsonConvert.SerializeObject(new NewOrderViewModel() {Name = "Hawaii"});

            var result = await _sut.Request("/api/orders", null, HttpMethod.Post, new StringContent(wrongOrder, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

            var result2 = await _sut.Request("/api/orders", _sut.Admin, HttpMethod.Post, new StringContent(wrongOrder, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, result2.StatusCode);

            var result3 = await _sut.Request("/api/orders", _sut.Admin, HttpMethod.Post, new StringContent(okOrder, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Created, result3.StatusCode);

            Assert.Equal(1, _sut.Server.Host.Services.GetService<ApplicationDbContext>().Order.Count());

            return Task.CompletedTask;
        }


        [Fact]
        public async Task<Task> CreateAndDeleteOrder()
        {
            var order = JsonConvert.SerializeObject(new NewOrderViewModel() {Name = "Hawaii"});

            var result1 = await _sut.Request("/api/orders", _sut.User1, HttpMethod.Post, new StringContent(order, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Created, result1.StatusCode);
            var orderFromServer = JsonConvert.DeserializeObject<Order>(await result1.Content.ReadAsStringAsync());

            var result2 = await _sut.Request($"/api/orders/{orderFromServer.Id}", _sut.User2, HttpMethod.Delete, new StringContent(order, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Forbidden, result2.StatusCode);

            var result3 = await _sut.Request($"/api/orders/{orderFromServer.Id}", _sut.User1, HttpMethod.Delete, new StringContent(order, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, result3.StatusCode);

            var result4 = await _sut.Request($"/api/orders/{orderFromServer.Id}", _sut.User1, HttpMethod.Delete, new StringContent(order, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, result4.StatusCode);


            return Task.CompletedTask;
        }
    }


    public class SUT
    {
        internal TestServer Server;
        internal HttpClient Client;
 
        public TokenInformation Admin { get; set; }
        public TokenInformation User1 { get; set; }
        public TokenInformation User2 { get; set; }

        public SUT()
        {
            var factory = new CustomWebApplicationFactory();

            Client = factory.CreateClient();
            Server = factory.Server;

            Server.Host.Services.GetService<UserManager<IdentityUser>>().CreateAsync(new IdentityUser {UserName = "admin@admin.ch"}, "123456").Wait();
            Server.Host.Services.GetService<UserManager<IdentityUser>>().CreateAsync(new IdentityUser() {UserName = "user1@user.ch"}, "123456").Wait();
            Server.Host.Services.GetService<UserManager<IdentityUser>>().CreateAsync(new IdentityUser() {UserName = "user2@user.ch"}, "123456").Wait();
            Admin = Server.Host.Services.GetService<SecurityService>().GetToken(new AuthRequest() {Username = "admin@admin.ch", Password = "123456"}).Result;
            User1 = Server.Host.Services.GetService<SecurityService>().GetToken(new AuthRequest() {Username = "user1@user.ch", Password = "123456"}).Result;
            User2 = Server.Host.Services.GetService<SecurityService>().GetToken(new AuthRequest() {Username = "user2@user.ch", Password = "123456"}).Result;
        }


        public async Task<HttpResponseMessage> Request(string url, TokenInformation info = null, HttpMethod method = null, HttpContent content = null)
        {
            method = method ?? HttpMethod.Post;

            var request = new HttpRequestMessage(method, url) {Content = content};
            if (info != null)
            {
                request.Headers.Add("Authorization", $"Bearer {info.Token}");
            }

            return await Client.SendAsync(request);
        }
    }

    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration config) : base(config)
        {
        }

        public override void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TEST"));
        }
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("TEST");
            builder.ConfigureServices(x => x.AddMvc().AddRazorPagesOptions(o => o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute())));
            base.ConfigureWebHost(builder);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();
        }
    }
}