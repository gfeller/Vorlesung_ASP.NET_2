using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BmiRechner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BmiRechner.Pages
{
    public class IndexModel : PageModel
    {
        public Bmi Bmi { get; set; } = new Bmi(){Height = 170, Weight = 80}; // Startdata

        public void OnGet()
        {

        }
    }
}
