using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ValueApp.Services;

namespace ValueApp.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IValueService _valueService;

        public ValuesController(IValueService valueService)
        {
            _valueService = valueService;
        }
        [HttpGet]
        public IEnumerable<Value> Get()
        {
            return _valueService.All();
        }


        [HttpGet("{id}")]
        public Value Get(int id)
        {
            return _valueService.Get(id);
        }
        
        [HttpPost]
        public Value Post([FromBody]Value value)
        {

            value = _valueService.Add(value);
            return value;
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]Value value)
        {
            _valueService.Change(id, value);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _valueService.Delete(id);
        }
    }
}


/*
        [ProducesResponseType(typeof(Value), 200)]
        [HttpPost]
        public IActionResult Post([FromBody] Value value)
        {

            value = _valueService.Add(value);
            return new CreatedAtActionResult("Get", "Values", new { id = value.Id) }, value);
        }
*/