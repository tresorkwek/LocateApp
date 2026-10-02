using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using Nancy.Configuration;
using Nancy.ViewEngines;
using Nancy.ViewEngines.Razor;

namespace LocateApp.Utilities
{
    /// <summary>
    /// Moteur Razor de Nancy protégé contre les rendus simultanés.
    /// Nancy.ViewEngines.Razor 2.0 compile les vues avec un générateur de code qui n'est pas thread-safe : quand deux requêtes
    /// affichent la même vue pour la première fois (typiquement juste après un démarrage, ex. deux GET /auth/login émis par le
    /// navigateur), l'une d'elles échoue avec « Value cannot be null. Parameter name: type » ou « The output writer for code
    /// generation and the writer supplied don't match » et la page 500 s'affiche.
    /// Ce décorateur sérialise le rendu (compilation comprise) avec un verrou réentrant : les vues partielles rendues depuis une
    /// vue restent sur le même thread et ne bloquent pas. Une fois les vues compilées et mises en cache, le coût est négligeable.
    /// </summary>
    public sealed class SafeRazorViewEngine : IViewEngine
    {
        private static readonly object Verrou = new object();
        private readonly RazorViewEngine moteur;

        public SafeRazorViewEngine(IRazorConfiguration configuration, INancyEnvironment environment, IAssemblyCatalog assemblyCatalog)
        {
            moteur = new RazorViewEngine(configuration, environment, assemblyCatalog);
        }

        public IEnumerable<string> Extensions => moteur.Extensions;

        public void Initialize(ViewEngineStartupContext viewEngineStartupContext)
        {
            moteur.Initialize(viewEngineStartupContext);
        }

        public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            Response reponse;
            lock (Verrou)
            {
                reponse = moteur.RenderView(viewLocationResult, model, renderContext);
            }

            // La compilation et l'exécution de la vue ont lieu à l'écriture du corps de la réponse : on protège aussi cette étape.
            Action<Stream> contenu = reponse.Contents;
            if (contenu != null)
            {
                reponse.Contents = flux =>
                {
                    lock (Verrou)
                    {
                        contenu(flux);
                    }
                };
            }

            return reponse;
        }
    }
}
