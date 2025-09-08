using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// 邮件服务接口
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="request">邮件发送请求</param>
        /// <returns>发送结果</returns>
        Task<EmailSendResult> SendEmailAsync(SendEmailRequest request);

        /// <summary>
        /// 使用模板发送邮件
        /// </summary>
        /// <param name="request">模板邮件发送请求</param>
        /// <returns>发送结果</returns>
        Task<EmailSendResult> SendTemplateEmailAsync(SendTemplateEmailRequest request);

        /// <summary>
        /// 创建邮件模板
        /// </summary>
        /// <param name="template">邮件模板</param>
        /// <returns>是否创建成功</returns>
        Task<bool> CreateTemplateAsync(EmailTemplate template);

        /// <summary>
        /// 更新邮件模板
        /// </summary>
        /// <param name="template">邮件模板</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateTemplateAsync(EmailTemplate template);

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteTemplateAsync(string templateId);

        /// <summary>
        /// 获取邮件模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>邮件模板</returns>
        Task<EmailTemplate?> GetTemplateAsync(string templateId);

        /// <summary>
        /// 获取所有邮件模板
        /// </summary>
        /// <returns>邮件模板列表</returns>
        Task<List<EmailTemplate>> GetAllTemplatesAsync();

        /// <summary>
        /// 测试邮件连接
        /// </summary>
        /// <returns>连接测试结果</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// 渲染模板内容
        /// </summary>
        /// <param name="template">模板内容</param>
        /// <param name="parameters">参数字典</param>
        /// <returns>渲染后的内容</returns>
        string RenderTemplate(string template, Dictionary<string, object>? parameters);
    }
}