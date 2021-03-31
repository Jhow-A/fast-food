using FastFood.Data.Context;
using FastFood.Data.Repository;
using FastFood.Data.Repository.Interfaces;
using FastFood.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FastFood
{
    // Startup.cs equivalente ao Global.asax.
    // Cont�m o c�digo de inicializa��o e configura��o.
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // Cont�m a lista de servi�os da aplica��o
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc(); -- Vers�o 3.1

            services.AddControllersWithViews()
                // AddRazorRuntimeCompilation(): habilitar refreshing ap�s mudan�as na view
                .AddRazorRuntimeCompilation()
                // AddNewtonsoftJson(): erro de serializa��o no TempData "cannot serialize an object of type"
                .AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Servi�o de identity
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                /* Por padr�o, se n�o for personalizado nada, o Identity configurar� um conjunto padr�o de regras de valida��o para novas senhas:
                    - As senhas devem ter pelo menos 6 caracteres
                    - As senhas devem ter pelo menos uma letra min�scula ('a' - 'z')
                    - As senhas devem ter pelo menos uma letra mai�scula ('A' - 'Z')
                    - As senhas devem ter pelo menos um d�gito ('0' - '9')
                    - As senhas devem ter pelo menos um caractere n�o alfanum�rico
                */
                options.Password.RequiredLength = 3;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
                // Adiciona uma implementa��o do EF que armazena as informa��es de indentidade
                .AddEntityFrameworkStores<AppDbContext>()
                // Adiciona os tokens que cuida da troca de senha e envio de email
                .AddDefaultTokenProviders();

            // AddTransient: n�o necessitamos manter estado de nada dentro dele, ou seja, a cada inje��o, ser� uma nova inst�ncia resolvida
            services.AddTransient<ICategoriaRepository, CategoriaRepository>();
            services.AddTransient<ILancheRepository, LancheRepository>();
            services.AddTransient<IPedidoRepository, PedidoRepository>();

            // AddScoped: objeto � o mesmo dentro de um request, mas diferente atrav�s de diferentes requests
            services.AddScoped(cp => CarrinhoCompras.GetCarrinho(cp));

            // AddSingleton: objeto ser� o mesmo para todas as requisi��es
            // HttpContextAccessor: ter acesso a sess�o do contexto
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configura��o do uso de sess�o
            services.AddMemoryCache();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Recebe o IApplicationBuilder onde pode se configurar o ambiente, p�gina de erros, etc;
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Midlewares
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "AdminArea",
                    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "filtrarPorCategoria",
                    pattern: "Lanches/{action}/{categoria?}",
                    defaults: new { Controller = "Lanches", Action = "List" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            /* -- Vers�o 3.1
            app.UseMvc(routes =>
             {
                 routes.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}");
             });*/
        }
    }
}
