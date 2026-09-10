using Microsoft.AspNetCore.Mvc;

namespace SmartQueueApi.Responses
{
    public class ServiceResult : IActionResult
    {
        private readonly int _statusCode;
        private readonly object _data;
        private readonly string _errorCode;
        private readonly bool _isFailedResult;

        private ServiceResult(int statusCode)
        {
            _statusCode = statusCode;
            _data = null;
            _errorCode = null;
            _isFailedResult = false;
        }

        private ServiceResult(int statusCode,
            object data,
            bool isFailedResult)
        {
            _statusCode = statusCode;
            _data = data;
            _errorCode = null;
            _isFailedResult = isFailedResult;
        }

        private ServiceResult(int statusCode, string errorCode)
        {
            _statusCode = statusCode;
            _data = null;
            _errorCode = errorCode;
            _isFailedResult = true;
        }

        public int GetStatusCode()
        {
            return _statusCode;
        }

        public string GetErrorCode()
        {
            return _errorCode;
        }

        public T GetData<T>()
        {
            if (_data is not null)
            {
                return (T)_data;
            }

            return default;
        }

        public bool IsFailedResult()
        {
            return _isFailedResult;
        }

        public static ServiceResult Success(int statusCode)
        {
            return new ServiceResult(statusCode);
        }

        public static ServiceResult Success<T>(int statusCode, T data)
        {
            return new ServiceResult(statusCode, data!, false);
        }

        public static ServiceResult Fail(int statusCode, string errorCode)
        {
            return new ServiceResult(statusCode, errorCode);
        }

        public static ServiceResult Fail<T>(int statusCode, T data)
        {
            return new ServiceResult(statusCode, data!, true);
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (_data is IActionResult actionResult)
            {
                await actionResult.ExecuteResultAsync(context);
            }
            else
            {
                var objectResult = new ObjectResult(_data ?? _errorCode)
                {
                    StatusCode = _statusCode,
                };

                await objectResult.ExecuteResultAsync(context);
            }
        }
    }
}
