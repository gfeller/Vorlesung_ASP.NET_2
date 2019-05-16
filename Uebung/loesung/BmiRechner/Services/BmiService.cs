using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BmiRechner.Data;

namespace BmiRechner.Services
{
    public interface IBmiService
    {
        double Calculcate(Bmi data);
    }

    public class BmiService : IBmiService
    {
        public double Calculcate(Bmi data)
        {
            return Math.Round(data.Weight / Math.Pow((data.Height / 100), 2), 2);
        }
    }
}
