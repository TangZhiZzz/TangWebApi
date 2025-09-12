using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// 邮件服务实现类
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly TangWebApi.Options.EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly List<EmailTemplate> _templates;

        public EmailService(IOptions<TangWebApi.Options.EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _templates = new List<EmailTemplate>();
            InitializeDefaultTemplates();
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public async Task<EmailSendResult> SendEmailAsync(SendEmailRequest request)
        {
            try
            {
                using var client = CreateSmtpClient();
                using var message = CreateMailMessage(request);

                await client.SendMailAsync(message);

                _logger.LogInformation("邮件发送成功，收件人: {To}, 主题: {Subject}", request.To, request.Subject);

                return new EmailSendResult
                {
                    Success = true,
                    MessageId = message.Headers["Message-ID"] ?? Guid.NewGuid().ToString(),
                    SentAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件发送失败，收件人: {To}, 主题: {Subject}", request.To, request.Subject);

                return new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 使用模板发送邮件
        /// </summary>
        public async Task<EmailSendResult> SendTemplateEmailAsync(SendTemplateEmailRequest request)
        {
            try
            {
                var template = await GetTemplateAsync(request.TemplateId);
                if (template == null)
                {
                    return new EmailSendResult
                    {
                        Success = false,
                        ErrorMessage = $"模板 {request.TemplateId} 不存在",
                        SentAt = DateTime.Now
                    };
                }

                var subject = RenderTemplate(template.Subject, request.Parameters);
                var body = RenderTemplate(template.Body, request.Parameters);

                var emailRequest = new SendEmailRequest
                {
                    To = request.To,
                    Cc = request.Cc,
                    Bcc = request.Bcc,
                    Subject = subject,
                    Body = body,
                    IsHtml = template.IsHtml
                };

                return await SendEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "模板邮件发送失败，收件人: {To}, 模板ID: {TemplateId}", request.To, request.TemplateId);

                return new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 创建邮件模板
        /// </summary>
        public async Task<bool> CreateTemplateAsync(EmailTemplate template)
        {
            try
            {
                if (_templates.Any(t => t.Id == template.Id))
                {
                    return false; // 模板已存在
                }

                template.CreatedAt = DateTime.Now;
                template.UpdatedAt = DateTime.Now;
                _templates.Add(template);

                _logger.LogInformation("邮件模板创建成功，模板ID: {TemplateId}", template.Id);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件模板创建失败，模板ID: {TemplateId}", template.Id);
                return false;
            }
        }

        /// <summary>
        /// 更新邮件模板
        /// </summary>
        public async Task<bool> UpdateTemplateAsync(EmailTemplate template)
        {
            try
            {
                var existingTemplate = _templates.FirstOrDefault(t => t.Id == template.Id);
                if (existingTemplate == null)
                {
                    return false; // 模板不存在
                }

                existingTemplate.Name = template.Name;
                existingTemplate.Subject = template.Subject;
                existingTemplate.Body = template.Body;
                existingTemplate.IsHtml = template.IsHtml;
                existingTemplate.UpdatedAt = DateTime.Now;

                _logger.LogInformation("邮件模板更新成功，模板ID: {TemplateId}", template.Id);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件模板更新失败，模板ID: {TemplateId}", template.Id);
                return false;
            }
        }

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        public async Task<bool> DeleteTemplateAsync(string templateId)
        {
            try
            {
                var template = _templates.FirstOrDefault(t => t.Id == templateId);
                if (template == null)
                {
                    return false; // 模板不存在
                }

                _templates.Remove(template);

                _logger.LogInformation("邮件模板删除成功，模板ID: {TemplateId}", templateId);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件模板删除失败，模板ID: {TemplateId}", templateId);
                return false;
            }
        }

        /// <summary>
        /// 获取邮件模板
        /// </summary>
        public async Task<EmailTemplate?> GetTemplateAsync(string templateId)
        {
            try
            {
                var template = _templates.FirstOrDefault(t => t.Id == templateId);
                return await Task.FromResult(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取邮件模板失败，模板ID: {TemplateId}", templateId);
                return null;
            }
        }

        /// <summary>
        /// 获取所有邮件模板
        /// </summary>
        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            try
            {
                return await Task.FromResult(_templates.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有邮件模板失败");
                return new List<EmailTemplate>();
            }
        }

        /// <summary>
        /// 测试邮件连接
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var client = CreateSmtpClient();
                await client.SendMailAsync(new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    To = { _emailSettings.FromEmail },
                    Subject = "邮件服务连接测试",
                    Body = "这是一封测试邮件，用于验证邮件服务连接是否正常。",
                    IsBodyHtml = false
                });

                _logger.LogInformation("邮件连接测试成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件连接测试失败");
                return false;
            }
        }

        /// <summary>
        /// 渲染模板内容
        /// </summary>
        public string RenderTemplate(string template, Dictionary<string, object>? parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return template;
            }

            var result = template;
            foreach (var parameter in parameters)
            {
                var placeholder = $"{{{{{parameter.Key}}}}}";
                result = result.Replace(placeholder, parameter.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        /// <summary>
        /// 创建SMTP客户端
        /// </summary>
        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            return client;
        }

        /// <summary>
        /// 创建邮件消息
        /// </summary>
        private MailMessage CreateMailMessage(SendEmailRequest request)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsHtml
            };

            message.To.Add(request.To);

            // 添加抄送
            if (request.Cc != null && request.Cc.Any())
            {
                foreach (var cc in request.Cc)
                {
                    message.CC.Add(cc);
                }
            }

            // 添加密送
            if (request.Bcc != null && request.Bcc.Any())
            {
                foreach (var bcc in request.Bcc)
                {
                    message.Bcc.Add(bcc);
                }
            }

            // 添加附件
            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var attachment in request.Attachments)
                {
                    if (File.Exists(attachment))
                    {
                        message.Attachments.Add(new Attachment(attachment));
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// 初始化默认模板
        /// </summary>
        private void InitializeDefaultTemplates()
        {
            var welcomeTemplate = new EmailTemplate
            {
                Id = "welcome",
                Name = "欢迎邮件模板",
                Subject = "欢迎使用 {{AppName}}",
                Body = @"<html><body>
                    <h2>欢迎 {{UserName}}！</h2>
                    <p>感谢您注册 {{AppName}}，我们很高兴为您提供服务。</p>
                    <p>如果您有任何问题，请随时联系我们。</p>
                    <p>祝好！<br/>{{AppName}} 团队</p>
                </body></html>",
                IsHtml = true
            };

            var resetPasswordTemplate = new EmailTemplate
            {
                Id = "reset-password",
                Name = "重置密码邮件模板",
                Subject = "{{AppName}} - 重置密码",
                Body = @"<html><body>
                    <h2>重置密码</h2>
                    <p>您好 {{UserName}}，</p>
                    <p>我们收到了您重置密码的请求。请点击下面的链接重置您的密码：</p>
                    <p><a href='{{ResetLink}}'>重置密码</a></p>
                    <p>如果您没有请求重置密码，请忽略此邮件。</p>
                    <p>此链接将在 {{ExpiryTime}} 小时后失效。</p>
                    <p>祝好！<br/>{{AppName}} 团队</p>
                </body></html>",
                IsHtml = true
            };

            _templates.AddRange(new[] { welcomeTemplate, resetPasswordTemplate });
        }
    }
}