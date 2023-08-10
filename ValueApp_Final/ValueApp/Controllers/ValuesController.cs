﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using ValueApp.Services;

namespace ValueApp.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IValueService _valueService;
        private readonly IUrlHelper _uriHelper;

        public ValuesController(IActionContextAccessor actionContextAccessor, IUrlHelperFactory helper, IValueService valueService)
        {
            _uriHelper = helper.GetUrlHelper(actionContextAccessor.ActionContext);
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

        [ProducesResponseType(typeof(Value), 200)]
        [HttpPost]
        public IActionResult Post([FromBody] NewValueViewModel newValue)
        {
            Value value = _valueService.Add(new Value() {Content = newValue.Content});
            return new CreatedAtActionResult("Get", "Values", new {id = value.Id}, value);
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
