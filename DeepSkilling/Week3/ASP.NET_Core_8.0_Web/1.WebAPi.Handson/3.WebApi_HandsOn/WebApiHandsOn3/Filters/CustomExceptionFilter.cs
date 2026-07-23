using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;

namespace WebApiHandsOn3.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly string _logFilePath;

        public CustomExceptionFilter()
        {
            // Directory for exception logs
            string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            _logFilePath = Path.Combine(logDir, "exceptions.log");
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string logEntry = $"[{timestamp}] EXCEPTION CAUGHT BY CustomExceptionFilter:\n" +
                              $"Type: {exception.GetType().FullName}\n" +
                              $"Message: {exception.Message}\n" +
                              $"StackTrace: {exception.StackTrace}\n" +
                              new string('-', 60) + "\n";

            // Write exception details to log file
            File.AppendAllText(_logFilePath, logEntry);

            // Set result to 500 Internal Server Error
            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred. Logged by CustomExceptionFilter.",
                DetailedError = exception.Message
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
