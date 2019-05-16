using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BmiRechner.Data;
using BmiRechner.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BmiRechner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BmiController : ControllerBase
    {
        private readonly IBmiService _bmiService;

        public BmiController(IBmiService bmiService)
        {
            _bmiService = bmiService;
        }

        [HttpGet]
        public double Calculate([FromQuery]Bmi data)
        {
            return _bmiService.Calculcate(data);
        }
    }
}