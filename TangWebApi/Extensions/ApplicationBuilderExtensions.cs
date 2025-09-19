using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.OpenApi.Models;
using TangWebApi.Filter;
using TangWebApi.Middleware;

namespace TangWebApi.Extensions
{
    /// <summary>
    /// 应用程序构建器扩展方法
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 配置Swagger中间件
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerService(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TangWebApi v1");
                c.RoutePrefix = "swagger";
            });

            return app;
        }

        /// <summary>
        /// 配置Knife4j中间件
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static IApplicationBuilder UseKnife4jService(this IApplicationBuilder app)
        {
            // 配置Knife4j
            app.UseKnife4UI(c =>
            {
                c.RoutePrefix = "kapi";
                c.SwaggerEndpoint("//swagger/v1/swagger.json", "TangWebApi v1");
                c.DocumentTitle = "TangWebApi 接口文档";
            });

            return app;
        }

        /// <summary>
        /// 配置CORS中间件
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCorsService(this IApplicationBuilder app)
        {
            // 为SignalR Hub使用特殊的CORS策略
            app.UseCors("SignalRPolicy");
            return app;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static async Task<IApplicationBuilder> InitializeDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var sqlSugarService = scope.ServiceProvider.GetRequiredService<Services.SqlSugarService>();
            await sqlSugarService.InitializeDatabaseAsync();
            return app;
        }

        /// <summary>
        /// 配置开发环境中间件
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        public static IApplicationBuilder UseDevelopmentEnvironment(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerService();
                app.UseKnife4jService();
            }

            return app;
        }

        /// <summary>
        /// 使用JWT认证中间件
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

        /// <summary>
        /// 使用请求日志中间件
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            return app;
        }

        /// <summary>
        /// 使用SignalR服务
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSignalRService(this IApplicationBuilder app)
        {
            // 映射SignalR Hub
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TangWebApi.Hubs.ChatHub>("/chathub");
            });
            
            return app;
        }
    }
}