using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza.Services;
using Pizza.Utilities;
using Pizza.ViewModels;

namespace Pizza.Controllers
{

    [Route("api/orders")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class OrderApiController : Controller
    {
        private readonly OrderService _orderSerivce;
        
        public OrderApiController(OrderService orderSerivce)
        {
            _orderSerivce = orderSerivce;
        }

        [HttpPost]
        public IActionResult Create([FromBody]NewOrderViewModel newOrder)
        {
            var order = _orderSerivce.Add(User.GetId(), newOrder);
            return new CreatedAtActionResult("Get", "OrderApi", new {Id = order.Id}, order);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult(_orderSerivce.GetAll(User));
        }


        [HttpGet("{id}")]
        public IActionResult Show(long id)
        {
            return new OkObjectResult(_orderSerivce.Get(User, id));
        }
        
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            return new OkObjectResult(_orderSerivce.Delete(User, id));
        }
    }
}