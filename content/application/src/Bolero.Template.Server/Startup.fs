namespace Bolero.Template.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open Bolero.Template
//#if (hotreload_actual)
open Bolero.Templating.Server
//#endif

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
//#if (hostpage == "razor")
        services.AddMvc().AddRazorRuntimeCompilation() |> ignore
//#else
        services.AddMvc() |> ignore
//#endif
        services.AddServerSideBlazor() |> ignore
        services
//#if (!minimal)
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<BookService>()
//#endif
//#if (hostpage != "html")
            .AddBoleroHost()
//#endif
//#if (hotreload_actual)
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Bolero.Template.Client")
#endif
//#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
//#if (!minimal)
            .UseAuthentication()
            .UseRemoting()
//#endif
            .UseStaticFiles()
            .UseRouting()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
//#if (hotreload_actual)
#if DEBUG
                endpoints.UseHotReload()
#endif
//#endif
//#if (hostpage == "razor")
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToPage("/_Host") |> ignore)
//#elseif (hostpage == "bolero")
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToBolero(Index.page) |> ignore)
//#elseif (hostpage == "html")
                endpoints.MapControllers() |> ignore
                endpoints.MapFallbackToFile("index.html") |> ignore)
//#endif
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStaticWebAssets()
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
