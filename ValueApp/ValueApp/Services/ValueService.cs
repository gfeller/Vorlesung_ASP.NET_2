using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace ValueApp.Services
{
    public class ValueService : IValueService
    {
        private static int _idCounter = 1;
        private static List<Value> Values { get; set; } = new List<Value>();


        public ValueService()
        {
            Add(new Value() {Content = "1"});
            Add(new Value() {Content = "2"});
            Add(new Value() {Content = "3"});
            Add(new Value() {Content = "4"});
        }

        public IEnumerable<Value> All()
        {
            return Values;
        }

   
        public Value Get(int id)
        {
            return Values[id];
        }

        public Value Add(Value value)
        {
            Values.Add(value);
            value.Id = _idCounter++;
            return value;
        }

        public Value Change(int id, Value value)
        {
            Values[id] = value;
            value.Id = id;
            return Values[id];
        }
        
        public Value Delete(int id)
        {
            Values[id] = null;
            return Values[id];
        }
    }
}
