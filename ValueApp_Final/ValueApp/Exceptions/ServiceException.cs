using System;
using System.Net;

namespace ValueApp.Exceptions
{
    public enum ServiceExceptionType
    {
        Unkown = HttpStatusCode.InternalServerError,
        NotFound = HttpStatusCode.NotFound,
        Duplicated = HttpStatusCode.BadRequest,
        ForbiddenByRule = HttpStatusCode.BadRequest
    }
    public class ServiceException : Exception
    {
        public ServiceExceptionType Type { get; private set; }

        public ServiceException(ServiceExceptionType type) 
        {
            Type = type;
        }
       
    }
}
