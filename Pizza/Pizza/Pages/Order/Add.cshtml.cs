using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pizza.Data;
using Pizza.Models;
using Pizza.Utilities;

namespace Pizza.Pages.Order
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly ApplicationDbContext _context;


        [TempData]
        public bool IsNew { get; set; }

        [BindProperty]
        public ViewModels.NewOrderViewModel Order { get; set; }


        public AddModel(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var toAdd = new Models.Order() {Name = Order.Name, CustomerId = User.GetId()};
            _context.Order.Add(toAdd);
            await _context.SaveChangesAsync();

            IsNew = true;
            return RedirectToPage("./Detail", new {toAdd.Id});
        }
    }
}