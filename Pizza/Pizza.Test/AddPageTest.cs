using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pizza.Data;
using Pizza.Pages.Order;
using Pizza.ViewModels;
using Xunit;

namespace Pizza.Test
{
    public class AddPageTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;

        public AddPageTest()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("Pizza"));

            var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
            tempDataFactory.Setup(x => x.GetTempData(It.IsAny<HttpContext>())).Returns(new Mock<ITempDataDictionary>().Object);
            services.AddSingleton(tempDataFactory.Object);


            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetService<ApplicationDbContext>();
        }


        [Fact]
        public async void CreateFailing()
        {
            AddModel model = new AddModel(null);
            model.Order = new NewOrderViewModel();
            ValidateModel(model.Order, model);
            Assert.NotNull(await model.OnPostAsync());
        }

        [Fact]
        public async void CreateValid()
        {
            var controller = new AddModel(_dbContext);
            var context = new DefaultHttpContext {User = CreateUser(), RequestServices = _serviceProvider};
            controller.PageContext = new PageContext() {HttpContext = context};

            controller.Order = new NewOrderViewModel() {Name = "Michael"};
            var result = await controller.OnPostAsync();

            Assert.True(_dbContext.Order.Any());
            Assert.IsType<RedirectToPageResult>(result);
        }


        private static void ValidateModel(object model, AddModel page)
        {
            var context = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, validationResults, true);
            foreach (var validationResult in validationResults)
            {
                page.ModelState.AddModelError("CustomError", validationResult.ErrorMessage);
            }
        }


        private GenericPrincipal CreateUser()
        {
            string username = "username";
            string userid = Guid.NewGuid().ToString("N"); //could be a constant
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userid)
            };

            var genericIdentity = new GenericIdentity("");
            genericIdentity.AddClaims(claims);
            var genericPrincipal = new GenericPrincipal(genericIdentity, new[] { "Administrator" });
            return genericPrincipal;
        }
    }
}
