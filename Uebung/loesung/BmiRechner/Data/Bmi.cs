using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BmiRechner.Data
{
    public class Bmi
    {
        [Range(0, 300)]
        [Display(Name = "Gewicht in kg")]
        public double Weight { get; set; }

        [Range(30, 250)]
        [Display(Name = "Höhe in cm")]
        public double Height { get; set; }
    }
}
