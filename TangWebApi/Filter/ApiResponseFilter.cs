using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TangWebApi.Models;

namespace TangWebApi.Filter
{
    /// <summary>
    /// API响应统一格式化过滤器
    /// </summary>
    public class ApiResponseFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 执行前不需要处理
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 如果已经是异常结果，不处理
            if (context.Exception != null)
            {
                return;
            }

            // 如果结果为空，不处理
            if (context.Result == null)
            {
                return;
            }

            // 如果已经是ApiResponse类型，不重复包装
            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value != null && 
                    (objectResult.Value.GetType().IsGenericType && 
                     objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>) ||
                     objectResult.Value.GetType() == typeof(ApiResponse)))
                {
                    return;
                }

                // 包装成统一的ApiResponse格式
                var statusCode = objectResult.StatusCode ?? 200;
                var success = statusCode >= 200 && statusCode < 300;
                var message = GetDefaultMessage(statusCode, success);

                ApiResponse<object> apiResponse;
                
                if (success)
                {
                    apiResponse = new ApiResponse<object>
                    {
                        Success = true,
                        Message = message,
                        Data = objectResult.Value
                    };
                }
                else
                {
                    // 对于错误响应，尝试从原始值中提取错误信息
                    var errorMessage = ExtractErrorMessage(objectResult.Value) ?? message;
                    apiResponse = new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        Data = null,
                        Error = objectResult.Value?.ToString()
                    };
                }

                context.Result = new ObjectResult(apiResponse)
                {
                    StatusCode = statusCode
                };
            }
            else if (context.Result is StatusCodeResult statusCodeResult)
            {
                // 处理只有状态码的结果
                var success = statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300;
                var message = GetDefaultMessage(statusCodeResult.StatusCode, success);

                var apiResponse = new ApiResponse<object>
                {
                    Success = success,
                    Message = message,
                    Data = null
                };

                context.Result = new ObjectResult(apiResponse)
                {
                    StatusCode = statusCodeResult.StatusCode
                };
            }
        }

        /// <summary>
        /// 获取默认消息
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="success">是否成功</param>
        /// <returns>默认消息</returns>
        private static string GetDefaultMessage(int statusCode, bool success)
        {
            return statusCode switch
            {
                200 => "操作成功",
                201 => "创建成功",
                204 => "操作成功",
                400 => "请求参数错误",
                401 => "未授权访问",
                403 => "禁止访问",
                404 => "资源不存在",
                409 => "资源冲突",
                422 => "请求参数验证失败",
                500 => "服务器内部错误",
                502 => "网关错误",
                503 => "服务不可用",
                _ => success ? "操作成功" : "操作失败"
            };
        }

        /// <summary>
        /// 从响应对象中提取错误消息
        /// </summary>
        /// <param name="value">响应值</param>
        /// <returns>错误消息</returns>
        private static string? ExtractErrorMessage(object? value)
        {
            if (value == null) return null;

            // 尝试从匿名对象中提取message属性
            var type = value.GetType();
            var messageProperty = type.GetProperty("message") ?? type.GetProperty("Message");
            if (messageProperty != null)
            {
                var messageValue = messageProperty.GetValue(value);
                return messageValue?.ToString();
            }

            return null;
        }
    }
}