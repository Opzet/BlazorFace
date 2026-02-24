// Copyright (c) Georg Jung. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Blazored.Modal;
using BlazorFace.Services;
using Microsoft.Extensions.FileProviders;

namespace BlazorFace.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureBlazorFaceServices(builder);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<IFileOpener, DefaultFileOpener>();

            BlazorFace.Startup.ShowTryLocallySection = true;
            BlazorFace.Startup.AddBlazorFaceServices(builder.Services);

            //// Add the following line:
            //builder.WebHost.UseSentry(o =>
            //{
            //    o.TracesSampleRate = 1.0;
            //});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            //app.UseSentryTracing();

            // CRITICAL: Serve ONNX models as static files
            var onnxPath = Path.Combine(app.Environment.ContentRootPath, "onnx");
            if (Directory.Exists(onnxPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(onnxPath),
                    RequestPath = "/onnx",
                    ServeUnknownFileTypes = true, // Allow .onnx files
                    OnPrepareResponse = ctx =>
                    {
                        // Log ONNX file requests for debugging
                        if (ctx.File.Name.EndsWith(".onnx"))
                        {
                            Console.WriteLine($"[ONNX] Serving model: {ctx.File.Name} ({ctx.File.Length} bytes)");
                        }
                    }
                });
                Console.WriteLine($"[Startup] ONNX models directory configured: {onnxPath}");
            }
            else
            {
                Console.WriteLine($"[WARNING] ONNX models directory not found: {onnxPath}");
            }

            app.MapStaticAssets();
            app.MapRazorComponents<Components.App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(typeof(BlazorFace.Components.Routes).Assembly);

            app.Run();
        }

        public static void ConfigureBlazorFaceServices(WebApplicationBuilder builder)
        {
            Console.WriteLine("[Startup] Configuring BlazorFace services...");

            BlazorFace.Startup.ConfigureBlazorFaceServices(builder.Services, builder.Configuration);

            // Verify ONNX models exist
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var models = new[]
            {
                Path.Combine(exeDir, @"onnx/arcfaceresnet100-11-int8.onnx"),
                Path.Combine(exeDir, @"onnx/scrfd_2.5g_kps.onnx"),
                Path.Combine(exeDir, @"onnx/open_closed_eye.onnx")
            };

            foreach (var modelPath in models)
            {
                if (File.Exists(modelPath))
                {
                    var fileInfo = new FileInfo(modelPath);
                    Console.WriteLine($"[Startup] ✓ Model found: {Path.GetFileName(modelPath)} ({fileInfo.Length / 1024 / 1024:F1} MB)");
                }
                else
                {
                    Console.WriteLine($"[Startup] ✗ ERROR: Model NOT found: {modelPath}");
                }
            }
        }
    }
}
