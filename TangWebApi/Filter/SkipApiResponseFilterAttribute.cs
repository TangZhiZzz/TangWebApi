using Microsoft.AspNetCore.Mvc.Filters;

namespace TangWebApi.Filter
{
    /// <summary>
    /// 跳过API响应格式化过滤器的标记属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkipApiResponseFilterAttribute : Attribute
    {
    }
}