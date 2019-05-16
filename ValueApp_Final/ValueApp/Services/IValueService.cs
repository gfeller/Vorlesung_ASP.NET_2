using System.Collections.Generic;

namespace ValueApp.Services
{
    public interface IValueService
    {
        IEnumerable<Value> All();
        Value Get(int id);
        Value Add(Value value);
        Value Change(int id, Value value);
        Value Delete(int id);
    }
}