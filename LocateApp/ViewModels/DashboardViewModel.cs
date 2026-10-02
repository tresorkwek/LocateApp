using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LocateApp.ViewModels
{
    public class DashboardViewModel:ViewModel
    {
        public int LastAnneeComptable { get; set; }
        public long NbreBien { get; set; }
        public long NbreBienExistant { get; set; }
        public long NbreBienIdentifie { get; set; }
        public long NbreBienInventorie { get; set; }
        public long NbreBienSansQRCode { get; set; }
        public decimal PourcentageBienSansQRCode { get; set; }
        public long NbreBienBon { get; set; }
        public long NbreBienMauvais { get; set; }
        public long NbreBienNonVu { get; set; }
        public long NbreLocal { get; set; }
        public long NbreLocalIdentifie { get; set; }
        public long NbreLocalSansQRCode { get; set; }
        public decimal PourcentageLocalSansQRCode { get; set; }
        public decimal VariationBienTotal { get; set; }
        public decimal VariationBienBon { get; set; }
        public decimal VariationBienMauvais { get; set; }
        public decimal VariationBienNonVu { get; set; }
        public decimal PourcentageInventaire { get; set; }
        public List<Organe> OrganeEntite { get; set; }
        public List<Agent> ResponsableLitigieuxBien { get; set; }
        public List<SelectStatObservationRequest> StatObservation { get; set; }
        public List<FamillieStatPieRequest> StatFamillePie { get; set; }
        public List<FamillieStatRadardRequest> StatFamilleRadar { get; set; }
        public List<Famille> FamillesBiens { get; set; }
        public List<Famille> FamillesIdentifies { get; set; }

        /// <summary>
        /// Données des graphiques en JSON (noms en camelCase, comme les réponses AJAX) : dashboard.js les dessine sans attendre de requête.
        /// Une liste vide laisse dashboard.js interroger le serveur, comme avant.
        /// </summary>
        public string DonneesGraphiquesJson => JsonConvert.SerializeObject(new
        {
            observations = StatObservation,
            famillesPie = StatFamillePie,
            famillesRadar = StatFamilleRadar,
            famillesBiens = FamillesBiens,
            famillesIdentifies = FamillesIdentifies
        }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), StringEscapeHandling = StringEscapeHandling.EscapeHtml });

        /// <summary>Vrai si le calcul des statistiques a été interrompu (base trop lente) : certaines valeurs sont à zéro.</summary>
        public bool StatistiquesIncompletes { get; private set; }

        /// <summary>Heure du calcul des statistiques affichées (elles sont conservées en cache quelques minutes, voir StatCache).</summary>
        public DateTime StatistiquesCalculeesLe { get; private set; }

        private const string CleCache = "dashboard";

        public DashboardViewModel(string userName = null,MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            // Les statistiques sont calculées une fois puis partagées entre tous les utilisateurs pendant StatCache.DureeMinutes.
            // Un instantané incomplet (budget dépassé ou erreur SQL) n'est jamais mis en cache.
            Instantane s = StatCache.Get(CleCache, () => new Calculateur(Identity).Calculer(), i => !i.Incomplet);

            StatistiquesIncompletes = s.Incomplet;
            StatistiquesCalculeesLe = s.CalculeLe;
            LastAnneeComptable = s.LastAnneeComptable;
            NbreBien = s.NbreBien;
            NbreBienExistant = s.NbreBienExistant;
            NbreBienIdentifie = s.NbreBienIdentifie;
            NbreBienInventorie = s.NbreBienInventorie;
            NbreBienBon = s.NbreBienBon;
            NbreBienMauvais = s.NbreBienMauvais;
            NbreBienNonVu = s.NbreBienNonVu;
            NbreBienSansQRCode = s.NbreBienExistant - s.NbreBienIdentifie;
            PourcentageBienSansQRCode = Pourcentage(NbreBienSansQRCode, s.NbreBienExistant);
            NbreLocal = s.NbreLocal;
            NbreLocalIdentifie = s.NbreLocalIdentifie;
            NbreLocalSansQRCode = s.NbreLocal - s.NbreLocalIdentifie;
            PourcentageLocalSansQRCode = Pourcentage(NbreLocalSansQRCode, s.NbreLocal);
            PourcentageInventaire = Pourcentage(s.NbreBienInventorie, s.NbreBienExistant);
            VariationBienTotal = s.VariationBienTotal;
            VariationBienBon = s.VariationBienBon;
            VariationBienMauvais = s.VariationBienMauvais;
            VariationBienNonVu = s.VariationBienNonVu;
            OrganeEntite = s.OrganeEntite ?? new List<Organe>();
            ResponsableLitigieuxBien = s.ResponsableLitigieuxBien ?? new List<Agent>();
            StatObservation = s.StatObservation ?? new List<SelectStatObservationRequest>();
            StatFamillePie = s.StatFamillePie ?? new List<FamillieStatPieRequest>();
            StatFamilleRadar = s.StatFamilleRadar ?? new List<FamillieStatRadardRequest>();
            FamillesBiens = s.FamillesBiens ?? new List<Famille>();
            FamillesIdentifies = s.FamillesIdentifies ?? new List<Famille>();
        }

        private static decimal Pourcentage(decimal partie, decimal total) => total == 0 ? 0 : partie / total * 100;

        /// <summary>Variation en % entre une quantité de référence (exercice précédent) et la quantité actuelle.</summary>
        private static decimal Variation(decimal actuelle, decimal precedente) => precedente == 0 ? 0 : (actuelle - precedente) / precedente * 100;

        public string VariationBienIcone(decimal variationValue)
        {
            return variationValue > 0 ? "trending_up" : (variationValue < 0 ? "trending_down" : "");
        }

        public string VariationBienColor(decimal variationValue)
        {
            return variationValue >= 0 ? "green" : "red";
        }

        /// <summary>Résultat brut du calcul des statistiques, tel que conservé en cache.</summary>
        public sealed class Instantane
        {
            public DateTime CalculeLe = DateTime.Now;
            public bool Incomplet;
            public int LastAnneeComptable;
            public long NbreBien, NbreBienExistant, NbreBienIdentifie, NbreBienInventorie, NbreBienBon, NbreBienMauvais, NbreBienNonVu, NbreLocal, NbreLocalIdentifie;
            public decimal VariationBienTotal, VariationBienBon, VariationBienMauvais, VariationBienNonVu;
            public List<Organe> OrganeEntite;
            public List<Agent> ResponsableLitigieuxBien;
            public List<SelectStatObservationRequest> StatObservation;
            public List<FamillieStatPieRequest> StatFamillePie;
            public List<FamillieStatRadardRequest> StatFamilleRadar;
            public List<Famille> FamillesBiens;
            public List<Famille> FamillesIdentifies;
        }

        /// <summary>Enchaîne les requêtes statistiques sous contrainte de temps : au-delà du budget, les étapes restantes sont sautées
        /// pour que la page s'affiche quand même au lieu de bloquer la connexion pendant des minutes.</summary>
        private sealed class Calculateur
        {
            /// <summary>Temps maximal accordé au calcul (ms), réglable par la clé DashboardBudgetMs du Web.config.</summary>
            private static readonly int BudgetMs = int.TryParse(ConfigurationManager.AppSettings["DashboardBudgetMs"], out int budget) && budget > 0 ? budget : 20000;
            private static readonly Logger Log = Logger.GetLogger(typeof(DashboardViewModel));

            private readonly Identity identite;
            private readonly Stopwatch chrono = Stopwatch.StartNew();
            private readonly Instantane s = new Instantane();

            public Calculateur(Identity identite)
            {
                this.identite = identite;
            }

            private T Calcul<T>(Func<T> calcul, string etape)
            {
                if (chrono.ElapsedMilliseconds > BudgetMs)
                {
                    if (!s.Incomplet)
                    {
                        s.Incomplet = true;
                        Log.Warning(identite, $"Tableau de bord : budget de {BudgetMs} ms dépassé ({chrono.ElapsedMilliseconds} ms) avant l'étape « {etape} » ; les statistiques restantes sont ignorées. Vérifiez la charge du serveur SQL.");
                    }
                    return default;
                }

                int erreursAvant = SqlDataAccess.NbErreursThread;
                try
                {
                    T valeur = calcul();
                    if (SqlDataAccess.NbErreursThread != erreursAvant)
                    {
                        s.Incomplet = true; // l'erreur SQL elle-même est déjà dans le journal
                    }
                    return valeur;
                }
                catch (Exception e)
                {
                    s.Incomplet = true;
                    Log.Error(identite, $"Tableau de bord : échec de l'étape « {etape} » : {e.Message}");
                    return default;
                }
            }

            public Instantane Calculer()
            {
                s.NbreBien = Calcul(() => ImmoController.SelectQuantite(), "biens");
                s.NbreBienIdentifie = Calcul(() => ImmoController.SelectQuantiteIdentifie(), "biens identifiés");
                s.NbreLocal = Calcul(() => LocalController.SelectQuantite(), "locaux");
                s.NbreLocalIdentifie = Calcul(() => LocalController.SelectQuantiteIdentifie(), "locaux identifiés");
                s.NbreBienExistant = Calcul(() => ImmoController.SelectQuantiteExistant(), "biens existants");
                s.NbreBienInventorie = Calcul(() => InventaireController.SelectQuantite(), "biens inventoriés");
                s.LastAnneeComptable = Calcul(() => InventaireController.SelectLastAnneeComptable(), "dernière année comptable");
                s.NbreBienBon = Calcul(() => ImmoController.SelectQuantiteByEtat("B"), "biens en bon état");
                s.NbreBienMauvais = Calcul(() => ImmoController.SelectQuantiteByEtat("M"), "biens en mauvais état");
                s.NbreBienNonVu = Calcul(() => ImmoController.SelectQuantiteNonVu(), "biens non vus");

                int anneePrecedente = s.LastAnneeComptable - 1;
                s.VariationBienTotal = Calcul(() => Variation(s.NbreBienExistant, InventaireController.SelectQuantite(anneePrecedente)), "variation des biens");
                s.VariationBienBon = Calcul(() => Variation(s.NbreBienBon, InventaireController.SelectQuantiteByEtat("B", anneePrecedente)), "variation bon état");
                s.VariationBienMauvais = Calcul(() => Variation(s.NbreBienMauvais, InventaireController.SelectQuantiteByEtat("M", anneePrecedente)), "variation mauvais état");
                s.VariationBienNonVu = Calcul(() => Variation(s.NbreBienNonVu, InventaireController.SelectQuantiteNonVu(anneePrecedente)), "variation non vus");

                s.OrganeEntite = Calcul(() => OrganeController.OrganeEntite(), "entités");
                s.ResponsableLitigieuxBien = Calcul(() => AgentController.SelectResponsableLitigieux(), "responsables litigieux");
                s.StatObservation = Calcul(() => ObservationsController.SelectStat(), "observations");
                s.StatFamillePie = Calcul(() => FamilleController.SelectStatPie(), "familles (anneau)");
                s.StatFamilleRadar = Calcul(() => FamilleController.SelectStatRadar(), "familles (radar)");
                s.FamillesBiens = Calcul(() => FamilleController.SelectWithNumber(), "biens par famille");
                s.FamillesIdentifies = Calcul(() => FamilleController.SelectIdentifie(), "biens identifiés par famille");

                if (s.Incomplet || chrono.ElapsedMilliseconds > BudgetMs)
                {
                    Log.Warning(identite, $"Tableau de bord calculé en {chrono.ElapsedMilliseconds} ms (budget {BudgetMs} ms){(s.Incomplet ? ", statistiques incomplètes non mises en cache" : "")}.");
                }

                return s;
            }
        }
    }
}
