using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using LocateApp.Utilities;
using System.IO;
using LocateApp.Models;
using System.Runtime.CompilerServices;
using Nancy.Authentication.Forms;
using Nancy.Session;
using Nancy.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;
using Nancy.Extensions;
using Nancy.Configuration;

namespace LocateApp
{
    public class AppBootstrapper:DefaultNancyBootstrapper
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(AppBootstrapper));
        private static readonly string AppName = ConfigurationManager.AppSettings["Logiciel"];
        private static readonly string Url = ConfigurationManager.AppSettings["Url"];
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {

            log4net.Config.XmlConfigurator.Configure();

            base.ApplicationStartup(container, pipelines); // la partie importante

            CookieBasedSessions.Enable(pipelines); // pour activer la gestion des sesions

            //log des demandes entrantes ! 
            pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                if (ctx != null)
                {
                    Log.Request(ctx.Request.GetHashCode(), ctx.Request.Method, ctx.Request.Path, ctx.Request.UserHostAddress, ctx.Request.Headers.UserAgent);
                }
                return null;
            });

            //log des demandes sortantes ! 
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                if (ctx != null)
                {
                    Log.Response(ctx.Request.GetHashCode(), ctx.Response.StatusCode);                                     

                }

                if(ctx.Response.StatusCode != HttpStatusCode.OK)
                {
                    // Seules les vraies erreurs (4xx/5xx) sont journalisées : les redirections 303 et les 401 vers la page de connexion sont normales.
                    if ((int)ctx.Response.StatusCode >= 400 && ctx.Response.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        Log.Error(null, $"{(int)ctx.Response.StatusCode} {ctx.Response.StatusCode} sur {ctx.Request.Method} {ctx.Request.Path} {ctx.Response.ReasonPhrase}");
                    }

                    if (!(ctx.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html") && !ctx.Request.IsAjaxRequest()))
                    {
                        ctx.Response = JsonConvert.SerializeObject(
                                        new
                                        {
                                            Success = 0,
                                            Message = ConfigurationManager.AppSettings[$"{(int)ctx.Response.StatusCode}"],
                                            Content = ""
                                        });

                    }
                    
                }
            });
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            // Toute exception non gérée (y compris à l'affichage d'une vue Razor) est journalisée avec sa pile,
            // sinon la page 500 personnalisée masque complètement la cause.
            pipelines.OnError.AddItemToEndOfPipeline((ctx, exception) =>
            {
                Log.Error(null, $"Exception non gérée sur {ctx?.Request?.Method} {ctx?.Request?.Path} : {exception}");
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                            .WithHeader("Access-Control-Allow-Methods", "POST,GET,PUT,DELETE,HEAD,OPTIONS")
                            .WithHeader("Access-Control-Allow-Headers", "Accept,Origin,Content-type,Authorization");    
            }

            );

            base.RequestStartup(container, pipelines, context);

            // Vérifie s'il y a un Header pour la gestion de token
            if (context.Request.Headers["Authorization"].SingleOrDefault() == null)
            {
                var formsAuthConfiguration = new FormsAuthenticationConfiguration
                {
                    RedirectUrl = "~/auth/login",
                    UserMapper = container.Resolve<IUserMapper>()
                };

                FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
            }
            else
            {
                //Authorization header found. Enable Token authentication.
                //TokenAuthentication.Enable(pipelines, new TokenAuthenticationConfiguration(container.Resolve<ITokenizer>()));
                var keyByteArray = Encoding.ASCII.GetBytes("Y2F0Y2hlciUyMHdvbmclMjBsb3ZlJTIwLm5ldA==");
                var signingKey = new SymmetricSecurityKey(keyByteArray);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match !
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = true,
                    ValidIssuer = Url,
                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = true,
                    ValidAudience = AppName,
                    // Validate the token expiry
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var configuration = new JwtBearerAuthenticationConfiguration
                {
                    TokenValidationParameters = tokenValidationParameters
                };

                //enable the JwtBearer authentication
                pipelines.EnableJwtBearerAuthentication(configuration);
            }
        }

        /// <summary>Remplace le moteur Razor de Nancy par sa version protégée contre les rendus simultanés (voir SafeRazorViewEngine).</summary>
        protected override IEnumerable<Type> ViewEngines
        {
            get
            {
                // SafeRazorViewEngine est découvert automatiquement (il implémente IViewEngine) : on retire seulement le moteur d'origine.
                return base.ViewEngines.Where(t => t != typeof(Nancy.ViewEngines.Razor.RazorViewEngine)).Distinct();
            }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            //base.ConfigureApplicationContainer(container);
            //we don't call "base" here to prevent auto-discovery of type/dependecies
        }

#if DEBUG
        // En Debug : affiche le détail des erreurs de compilation Razor (jamais en Release).
        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);
            environment.Tracing(enabled: false, displayErrorTraces: true);
            // Recharge les vues Razor modifiées sans redémarrer l'application
            environment.Views(runtimeViewDiscovery: true, runtimeViewUpdates: true);
        }
#endif

        //lOGIN

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            //container.Register<IUserMapper, UserDatabase>();
            container.Register<IUserMapper, AppUserMapper>(); //ajouter 1
        }

    }
}