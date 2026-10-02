using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;

namespace LocateApp.Utilities
{
    /// <summary>
    /// Cache mémoire des statistiques coûteuses (tableau de bord, graphiques). Une valeur n'est conservée que si son calcul
    /// s'est déroulé sans erreur SQL, afin de ne jamais figer des zéros dus à un délai dépassé.
    /// Réglages (Web.config) : StatCacheMinutes (durée, 0 = désactivé) et StatCommandTimeout (délai SQL des statistiques, en secondes).
    /// </summary>
    public static class StatCache
    {
        private const string Prefixe = "stat:";
        private static readonly MemoryCache Cache = MemoryCache.Default;

        private sealed class Boite<T>
        {
            public T Valeur;
            public DateTime CalculeLe;
        }

        /// <summary>Durée de conservation en minutes (10 par défaut, 0 pour désactiver le cache).</summary>
        public static int DureeMinutes => Lire("StatCacheMinutes", 10);

        /// <summary>Délai SQL accordé aux requêtes statistiques chargées en arrière-plan (120 s par défaut, null si 0).</summary>
        public static int? TimeoutStatistiques
        {
            get
            {
                int secondes = Lire("StatCommandTimeout", 120);
                return secondes > 0 ? secondes : (int?)null;
            }
        }

        private static int Lire(string cle, int defaut)
        {
            return int.TryParse(ConfigurationManager.AppSettings[cle], out int valeur) && valeur >= 0 ? valeur : defaut;
        }

        /// <summary>Renvoie la valeur en cache ou la calcule. Le résultat n'est mis en cache que s'il n'est pas null,
        /// qu'aucune erreur SQL n'est survenue pendant le calcul et que <paramref name="peutMettreEnCache"/> (facultatif) l'accepte.</summary>
        public static T Get<T>(string cle, Func<T> charger, Func<T, bool> peutMettreEnCache = null)
        {
            string cleComplete = Prefixe + cle;

            if (DureeMinutes > 0 && Cache.Get(cleComplete) is Boite<T> boite)
            {
                return boite.Valeur;
            }

            int erreursAvant = SqlDataAccess.NbErreursThread;
            T valeur = charger();

            bool sansErreur = SqlDataAccess.NbErreursThread == erreursAvant;
            if (DureeMinutes > 0 && sansErreur && valeur != null && (peutMettreEnCache == null || peutMettreEnCache(valeur)))
            {
                Cache.Set(cleComplete, new Boite<T> { Valeur = valeur, CalculeLe = DateTime.Now }, DateTimeOffset.Now.AddMinutes(DureeMinutes));
            }

            return valeur;
        }

        /// <summary>Date de calcul de la valeur en cache, ou null si elle n'y est pas.</summary>
        public static DateTime? CalculeLe<T>(string cle)
        {
            return Cache.Get(Prefixe + cle) is Boite<T> boite ? boite.CalculeLe : (DateTime?)null;
        }

        /// <summary>Vide toutes les statistiques en cache (après une modification massive des données, par exemple).</summary>
        public static void Vider()
        {
            List<string> cles = Cache.Where(e => e.Key.StartsWith(Prefixe, StringComparison.Ordinal)).Select(e => e.Key).ToList();
            cles.ForEach(c => Cache.Remove(c));
        }
    }
}
