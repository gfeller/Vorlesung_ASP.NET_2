using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace ValueApp.Services
{
    
    public class Value : RestRessource
    {
        [Required]
        [StringLength(50, MinimumLength = 10)]
        public string Content { get; set; }
    }


    public class NewValueViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 10)]
        public string Content { get; set; }
    }
}