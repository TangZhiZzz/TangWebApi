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
        public async Task<SendEmailResponse> SendEmail([FromBody] SendEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new SendEmailResponse
                    {
                        Success = false,
                        Message = "请求参数验证失败",
                        Error = string.Join("; ", errors),
                        StatusCode = 400
                    };
                }

                var result = await _emailService.SendEmailAsync(request);

                return new SendEmailResponse
                {
                    Success = result.Success,
                    Message = result.Success ? "邮件发送成功" : "邮件发送失败",
                    Data = result,
                    Error = result.ErrorMessage,
                    StatusCode = result.Success ? 200 : 500
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送邮件时发生异常");
                return new SendEmailResponse
                {
                    Success = false,
                    Message = "发送邮件时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 使用模板发送邮件
        /// </summary>
        /// <param name="request">模板邮件发送请求</param>
        /// <returns>发送结果</returns>
        [HttpPost("send-template")]
        public async Task<SendTemplateEmailResponse> SendTemplateEmail([FromBody] SendTemplateEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new SendTemplateEmailResponse
                    {
                        Success = false,
                        Message = "请求参数验证失败",
                        Error = string.Join("; ", errors),
                        StatusCode = 400
                    };
                }

                var result = await _emailService.SendTemplateEmailAsync(request);

                return new SendTemplateEmailResponse
                {
                    Success = result.Success,
                    Message = result.Success ? "模板邮件发送成功" : "模板邮件发送失败",
                    Data = result,
                    Error = result.ErrorMessage,
                    StatusCode = result.Success ? 200 : 500
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送模板邮件时发生异常");
                return new SendTemplateEmailResponse
                {
                    Success = false,
                    Message = "发送模板邮件时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 创建邮件模板
        /// </summary>
        /// <param name="template">邮件模板</param>
        /// <returns>创建结果</returns>
        [HttpPost("templates")]
        public async Task<CreateTemplateResponse> CreateTemplate([FromBody] EmailTemplate template)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new CreateTemplateResponse
                    {
                        Success = false,
                        Message = "请求参数验证失败",
                        Error = string.Join("; ", errors),
                        StatusCode = 400
                    };
                }

                var result = await _emailService.CreateTemplateAsync(template);

                return new CreateTemplateResponse
                {
                    Success = result,
                    Message = result ? "邮件模板创建成功" : "邮件模板创建失败，模板ID可能已存在",
                    Data = result ? template : null,
                    Error = result ? null : "模板ID已存在",
                    StatusCode = result ? 200 : 400
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建邮件模板时发生异常");
                return new CreateTemplateResponse
                {
                    Success = false,
                    Message = "创建邮件模板时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 更新邮件模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="template">邮件模板</param>
        /// <returns>更新结果</returns>
        [HttpPut("templates/{id}")]
        public async Task<UpdateTemplateResponse> UpdateTemplate(string id, [FromBody] EmailTemplate template)
        {
            try
            {
                if (id != template.Id)
                {
                    return new UpdateTemplateResponse
                    {
                        Success = false,
                        Message = "路径中的模板ID与请求体中的模板ID不匹配",
                        Error = "模板ID不匹配",
                        StatusCode = 400
                    };
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new UpdateTemplateResponse
                    {
                        Success = false,
                        Message = "请求参数验证失败",
                        Error = string.Join("; ", errors),
                        StatusCode = 400
                    };
                }

                var result = await _emailService.UpdateTemplateAsync(template);

                return new UpdateTemplateResponse
                {
                    Success = result,
                    Message = result ? "邮件模板更新成功" : "邮件模板更新失败，模板不存在",
                    Data = result ? template : null,
                    Error = result ? null : "模板不存在",
                    StatusCode = result ? 200 : 404
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新邮件模板时发生异常");
                return new UpdateTemplateResponse
                {
                    Success = false,
                    Message = "更新邮件模板时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("templates/{id}")]
        public async Task<DeleteTemplateResponse> DeleteTemplate(string id)
        {
            try
            {
                var result = await _emailService.DeleteTemplateAsync(id);

                return new DeleteTemplateResponse
                {
                    Success = result,
                    Message = result ? "邮件模板删除成功" : "邮件模板删除失败，模板不存在",
                    Error = result ? null : "模板不存在",
                    StatusCode = result ? 200 : 404
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除邮件模板时发生异常");
                return new DeleteTemplateResponse
                {
                    Success = false,
                    Message = "删除邮件模板时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 获取邮件模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>邮件模板</returns>
        [HttpGet("templates/{id}")]
        public async Task<GetTemplateResponse> GetTemplate(string id)
        {
            try
            {
                var template = await _emailService.GetTemplateAsync(id);

                return new GetTemplateResponse
                {
                    Success = template != null,
                    Message = template != null ? "获取邮件模板成功" : "邮件模板不存在",
                    Data = template,
                    Error = template != null ? null : "模板不存在",
                    StatusCode = template != null ? 200 : 404
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邮件模板时发生异常");
                return new GetTemplateResponse
                {
                    Success = false,
                    Message = "获取邮件模板时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 获取所有邮件模板
        /// </summary>
        /// <returns>邮件模板列表</returns>
        [HttpGet("templates")]
        public async Task<GetAllTemplatesResponse> GetAllTemplates()
        {
            try
            {
                var templates = await _emailService.GetAllTemplatesAsync();

                return new GetAllTemplatesResponse
                {
                    Success = true,
                    Message = "获取邮件模板列表成功",
                    Data = templates,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邮件模板列表时发生异常");
                return new GetAllTemplatesResponse
                {
                    Success = false,
                    Message = "获取邮件模板列表时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 测试邮件连接
        /// </summary>
        /// <returns>连接测试结果</returns>
        [HttpPost("test-connection")]
        public async Task<TestEmailConnectionResponse> TestConnection()
        {
            try
            {
                var result = await _emailService.TestConnectionAsync();

                return new TestEmailConnectionResponse
                {
                    Success = result,
                    Message = result ? "邮件连接测试成功" : "邮件连接测试失败",
                    Data = new { Connected = result, TestedAt = DateTime.Now },
                    Error = result ? null : "连接失败，请检查邮件配置",
                    StatusCode = result ? 200 : 500
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试邮件连接时发生异常");
                return new TestEmailConnectionResponse
                {
                    Success = false,
                    Message = "测试邮件连接时发生异常",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }

    // 响应数据模型
    public class SendEmailResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public EmailSendResult? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class SendTemplateEmailResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public EmailSendResult? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class CreateTemplateResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public EmailTemplate? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class UpdateTemplateResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public EmailTemplate? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class DeleteTemplateResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class GetTemplateResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public EmailTemplate? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class GetAllTemplatesResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public List<EmailTemplate>? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class TestEmailConnectionResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }
}