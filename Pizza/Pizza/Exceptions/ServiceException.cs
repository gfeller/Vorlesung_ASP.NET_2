using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Pizza_Demo.Exceptions
{
    public enum ServiceExceptionType
    {
        Unkown = HttpStatusCode.InternalServerError,
        NotFound = HttpStatusCode.NotFound,
        Duplicated = HttpStatusCode.BadRequest,
        ForbiddenByRule = HttpStatusCode.BadRequest,
        Forbidden = HttpStatusCode.Forbidden
    }
    public class ServiceException : Exception
    {
        public string Code { get; private set; }
        public ServiceExceptionType Type { get; private set; }

        public ServiceException(ServiceExceptionType type, [CallerMemberName] string name = "", [CallerLineNumber] int line = -1, [CallerFilePath] string path = "") 
        {
            Type = type;
            CreateCode(name, line, path);
        }

      
        public ServiceException(string message, Exception innerException, ServiceExceptionType type, [CallerMemberName] string name = "", [CallerLineNumber] int line = -1, [CallerFilePath] string path = "") : base(message, innerException)
        {
            CreateCode(name, line, path);
        }

        private void CreateCode(string name, int line, string path)
        {
            path = path.Substring(path.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            path = path.Substring(0, path.LastIndexOf(".", StringComparison.Ordinal));
            Code = $"{path}-{name}-{line}";  //TODO: Check on linux!
        }
    }
}
