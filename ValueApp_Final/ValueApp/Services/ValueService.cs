using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ValueApp.Exceptions;

namespace ValueApp.Services
{
    public class ValueService : IValueService
    {
        private static int _idCounter = 0;
        private static List<Value> Values { get; set; } = new List<Value>();
        
        public ValueService()
        {
            Add(new Value() { Content = "0" });
            Add(new Value() { Content = "1" });
            Add(new Value() { Content = "2" });
            Add(new Value() { Content = "3" });
        }

        public IEnumerable<Value> All()
        {
            return Values;
        }


        public Value Get(int id)
        {
            return EnsureValue(id);
        }

        public Value Add(Value value)
        {
            Values.Add(value);
            value.Id = _idCounter++;
            return value;
        }

        public Value Change(int id, Value value)
        {
            EnsureValue(id);
            
            Values[id] = value;
            value.Id = id;
            return Values[id];
        }

        public Value Delete(int id)
        {
            EnsureValue(id);

            Values[id] = null;
            return Values[id];
        }


        private static Value EnsureValue(int id)
        {
            var value = Values.FirstOrDefault(x => x.Id == id);
            if (value == null)
            {
                throw new ServiceException(ServiceExceptionType.NotFound);
            }
            return value;
        }
    }
}
