using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pizza.Data;
using Pizza.Models;

namespace Pizza.Pages.Order
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly Pizza.Data.ApplicationDbContext _context;

        public DetailModel(Pizza.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [TempData]
        public bool? IsNew { get; set; }

        public Models.Order Order { get; set; }

        public async Task<IActionResult> OnGetDeleteAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Order = await _context.Order.Include(x=>x.Customer).FirstOrDefaultAsync(m => m.Id == id);
            if (Order == null)
            {
                return NotFound();
            }

            Order.State = OrderState.Deleted;
            _context.SaveChanges();
            return Page();
            
        }

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Order = await _context.Order.Include(o => o.Customer).FirstOrDefaultAsync(m => m.Id == id);
            if (Order == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
