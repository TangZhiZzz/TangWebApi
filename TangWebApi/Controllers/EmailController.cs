using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 邮件服务控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="request">邮件发送请求</param>
        /// <returns>发送结果</returns>
        [HttpPost("send")]
        public async Task<EmailResult> SendEmail([FromBody] SendEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("请求参数无效");
                }

                var result = await _emailService.SendEmailAsync(request);

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送邮件时发生异常");
                throw new InvalidOperationException($"发送邮件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 使用模板发送邮件
        /// </summary>
        /// <param name="request">模板邮件发送请求</param>
        /// <returns>发送结果</returns>
        [HttpPost("send-template")]
        public async Task<EmailResult> SendTemplateEmail([FromBody] SendTemplateEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("请求参数无效");
                }

                var result = await _emailService.SendTemplateEmailAsync(request);

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送模板邮件时发生异常");
                throw new InvalidOperationException($"发送模板邮件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建邮件模板
        /// </summary>
        /// <param name="template">邮件模板</param>
        /// <returns>创建结果</returns>
        [HttpPost("templates")]
        public async Task<EmailTemplate> CreateTemplate([FromBody] EmailTemplate template)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("请求参数无效");
                }

                var result = await _emailService.CreateTemplateAsync(template);

                if (!result)
                {
                    throw new InvalidOperationException("模板ID已存在");
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建邮件模板时发生异常");
                throw new InvalidOperationException($"创建邮件模板失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新邮件模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="template">邮件模板</param>
        /// <returns>更新结果</returns>
        [HttpPut("templates/{id}")]
        public async Task<EmailTemplate> UpdateTemplate(string id, [FromBody] EmailTemplate template)
        {
            try
            {
                if (id != template.Id)
                {
                    throw new ArgumentException("模板ID不匹配");
                }

                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("请求参数无效");
                }

                var result = await _emailService.UpdateTemplateAsync(template);

                if (!result)
                {
                    throw new KeyNotFoundException("模板不存在");
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新邮件模板时发生异常");
                throw new InvalidOperationException($"更新邮件模板失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("templates/{id}")]
        public async Task DeleteTemplate(string id)
        {
            try
            {
                var result = await _emailService.DeleteTemplateAsync(id);

                if (!result)
                {
                    throw new KeyNotFoundException("模板不存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除邮件模板时发生异常");
                throw new InvalidOperationException($"删除邮件模板失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取邮件模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>邮件模板</returns>
        [HttpGet("templates/{id}")]
        public async Task<EmailTemplate> GetTemplate(string id)
        {
            try
            {
                var template = await _emailService.GetTemplateAsync(id);

                if (template == null)
                {
                    throw new KeyNotFoundException("模板不存在");
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邮件模板时发生异常");
                throw new InvalidOperationException($"获取邮件模板失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取所有邮件模板
        /// </summary>
        /// <returns>邮件模板列表</returns>
        [HttpGet("templates")]
        public async Task<List<EmailTemplate>> GetAllTemplates()
        {
            try
            {
                var templates = await _emailService.GetAllTemplatesAsync();
                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邮件模板列表时发生异常");
                throw new InvalidOperationException($"获取邮件模板列表失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 测试邮件连接
        /// </summary>
        /// <returns>连接测试结果</returns>
        [HttpPost("test-connection")]
        public async Task<EmailConnectionTestResult> TestConnection()
        {
            try
            {
                var result = await _emailService.TestConnectionAsync();

                if (!result)
                {
                    throw new InvalidOperationException("连接失败，请检查邮件配置");
                }

                return new EmailConnectionTestResult { Connected = result, TestedAt = DateTime.Now };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试邮件连接时发生异常");
                throw new InvalidOperationException($"测试邮件连接失败: {ex.Message}", ex);
            }
        }
    }

    public class EmailConnectionTestResult
    {
        public bool Connected { get; set; }
        public DateTime TestedAt { get; set; }
    }
}