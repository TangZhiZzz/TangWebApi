namespace TangWebApi.Models
{
    /// <summary>
    /// API统一响应模型
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 响应数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 创建成功响应
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="message">消息</param>
        /// <returns>成功响应</returns>
        public static ApiResponse<T> CreateSuccess(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="error">详细错误信息</param>
        /// <returns>失败响应</returns>
        public static ApiResponse<T> CreateError(string message, string? error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }

    /// <summary>
    /// 无数据的API响应模型
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// 创建成功响应
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>成功响应</returns>
        public static ApiResponse CreateSuccess(string message = "操作成功")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="error">详细错误信息</param>
        /// <returns>失败响应</returns>
        public static new ApiResponse CreateError(string message, string? error = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}