using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STIKS.Common;
using STIKS.Redis;

namespace STIKS.Server.Game
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", async context =>
                {
                    using (StreamWriter writer = new StreamWriter(context.Response.Body))
                    {
                        await writer.WriteAsync("Hellow Word");
                    }
                });
            });

            RedisCacheEngine.Instance.Connect();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            ProtocolEngine.Instance.Init();
            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {

                string[] splitPath = context.Request.Path.Value.Split('/');

                if (splitPath != null && splitPath.Length > 1 && splitPath[1] == "game")
                {
                    try
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                            var session = splitPath[splitPath.Length - 1];
                            if (string.IsNullOrEmpty(session))
                            {
                                context.Response.StatusCode = 400;
                            }
                            else
                            {
                                var item = await SocketEngine.Instance.Add(webSocket, session);
                                if (item == null)
                                {
                                    context.Response.StatusCode = 400;
                                }
                                else
                                    await item.Receive();
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Save(e);
                    }
                }
                else
                {
                    await next();
                }

            });
        }
    }
}
