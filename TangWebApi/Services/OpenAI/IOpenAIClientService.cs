using OpenAI;

namespace TangWebApi.Services
{
    /// <summary>
    /// OpenAI客户端服务接口
    /// </summary>
    public interface IOpenAIClientService
    {
        /// <summary>
        /// 获取OpenAI客户端实例
        /// </summary>
        /// <returns>OpenAI客户端</returns>
        OpenAIClient GetClient();

        /// <summary>
        /// 检查客户端是否已初始化
        /// </summary>
        /// <returns>是否已初始化</returns>
        bool IsInitialized { get; }
    }
}