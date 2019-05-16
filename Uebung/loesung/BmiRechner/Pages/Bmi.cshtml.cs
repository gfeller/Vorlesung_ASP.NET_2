using BmiRechner.Data;
using BmiRechner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BmiRechner.Pages
{
    public class BmiModel : PageModel
    {
        private readonly IBmiService _bmiService;

        public BmiModel(IBmiService bmiService)
        {
            _bmiService = bmiService;
        }


        [BindProperty(SupportsGet = true)]
        public Bmi Bmi { get; set; }

        public bool WrongData { get; set; }
        public double BmiValue { get; set; }

        public void OnGet()
        {
            if (ModelState.IsValid)
            {
                BmiValue = _bmiService.Calculcate(Bmi);
            }
            else
            {
                WrongData = true;
            }
        }
    }
}