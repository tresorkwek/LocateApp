using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.DataTransferObjects;
using LocateApp.Utilities;

namespace LocateApp.Controllers
{
    public static class OrganeController
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(OrganeController));

        public static List<Organe> Select(string valueToSelect = null)
        {

            string sql = valueToSelect == null ? SqlOrgane.SelectAll : SqlOrgane.SelectByName;
            var selectOrganeValues = valueToSelect == null ? null : new { ValueToSelect = valueToSelect + "%" };
           
            return SqlDataAccess.SelectData<Organe>(sql,selectOrganeValues);
        }
        public static Organe SelectById(string Id)
        {
            return SqlDataAccess.SelectData<Organe>(SqlOrgane.SelectById, new { Id }).FirstOrDefault();
        }

        public static Institution SelectInstitutionById(string Id)
        {
            return SqlDataAccess.SelectData<Institution>(SqlOrgane.SelectById, new { Id }).FirstOrDefault();
        }

        public static List<Organe> SelectOrganesStructure(string Id)
        {
            string sql = Id == null ? SqlOrgane.SelectAllWithNumber : SqlOrgane.SelectOrganesStructureByIdWithNumber;

            return SqlDataAccess.SelectData<Organe>(sql, new { Id });
        }
        public static List<Organe> SelectEntite()
        {
            return SqlDataAccess.SelectData<Organe>(SqlOrgane.SelectEntite);
        }

        public static List<string> SelectName(string valueToSelect)
        {
            return SqlDataAccess.SelectData<string>(SqlOrgane.SelectName, new { ValueToSelect = valueToSelect + "%" });
        }
        public static string SelectNameById(string Id)
        {
            return SqlDataAccess.SelectData<string>(SqlOrgane.SelectNameById, new { Id }).FirstOrDefault();
        }

        public static string SelectType(string Id)
        {
            return SqlDataAccess.SelectData<string>(SqlOrgane.SelectTypeById, new { Id }).FirstOrDefault();
        }

        public static List<int> SelectNbreLocaux(string Id)
        {
            return SqlDataAccess.SelectData<int>(SqlOrgane.SelectNbreLocauxById, new { Id });
        }

        public static int SelectMaxOrdreInterne(string IdStructure)
        {
            return SqlDataAccess.SelectData<int>(SqlOrgane.SelectMaxOrdreInterneByStructure, new { IdStructure }).FirstOrDefault();
        }

        private static IEnumerable<Organe> GetOrganes(IEnumerable<Organe> organes, string id = null)
        {
            //var organesParents = organes.Where(o => o.IdOrganeParent is null).ToList();
            //var organesParents = organes.Where(o => !organes.Any(i => i.Id.Equals(o.IdOrganeParent))).ToList();
            IEnumerable<Organe> organesParents  = new List<Organe>();

            if (id == null)
            {
                //organesParents = organes.Where(o => o.IdOrganeParent is null).ToList();
                organesParents = organes.Where(o => !organes.Any(i => i.Id.Equals(o.IdOrganeParent))).ToList();
            }
            else
            {
                organesParents = organes.Where(o => o.Id.Equals(id)).ToList();
            }

            foreach (var organeParent in organesParents)
            {
                var organesEnfants = organes.Where(o => o.IdOrganeParent != null && o.IdOrganeParent.Equals(organeParent.Id));
                organeParent.Organes = GetOrganesEnfants(organesEnfants, organes).ToList();
            }
            return organesParents;
        }

        private static IEnumerable<Organe> GetOrganesEnfants(IEnumerable<Organe> organesParents, IEnumerable<Organe> organes)
        {
            foreach (var organe in organesParents)
            {
                var organesEnfants = organes.Where(o => o.IdOrganeParent != null && o.IdOrganeParent.Equals(organe.Id));
                organe.Organes = GetOrganesEnfants(organesEnfants, organes).ToList();
            }
            return organesParents;
        }

        public static List<Organe> Organigramme(string Id = null)
        {
            string sql = Id == null ? SqlOrgane.SelectAllWithNumber : SqlOrgane.SelectOrganesStructureByIdWithNumber;

            List<Organe> organes = SqlDataAccess.SelectData<Organe>(sql, new { Id });

            if (organes == null)
            {
                // La requête a échoué (détail déjà journalisé par SqlDataAccess) : on renvoie un organigramme vide
                // plutôt qu'une erreur 500, sans exposer le message au client.
                Log.Error(null, $"Organigramme indisponible pour la structure '{Id ?? "toutes"}' : la requête SQL a échoué, voir l'erreur précédente.");
                return new List<Organe>();
            }

            return GetOrganes(organes,Id).ToList();
        }

        public static List<Organe> OrganigrammeByVersion(string Version)
        {
            string sql = SqlOrgane.SelectAllByVersion;

            List<Organe> organes = SqlDataAccess.SelectData<Organe>(sql, new { Version });
            return GetOrganes(organes).ToList();
        }

        public static List<Organe> OrganigrammeEntite(string Id = null)
        {
            string sql = Id == null ? SqlOrgane.SelectEntiteWithNumber : SqlOrgane.SelectEntiteWithNumberById;

            List<Organe> organes = SqlDataAccess.SelectData<Organe>(sql, new { Id });
            return GetOrganes(organes, Id).ToList();
        }

        public static List<Organe> OrganigrammeForConfig(string Id = null)
        {
            string sql = Id == null ? SqlOrgane.SelectAll : SqlOrgane.SelectOrganesStructureById;

            List<Organe> organes = SqlDataAccess.SelectData<Organe>(sql, new { Id });
            return GetOrganes(organes, Id).ToList();
        }

        public static List<Organe> OrganeEntite(int? Annee = null, string Id = null)
        {
            Annee = Annee == null ? InventaireController.SelectLastAnneeComptable() : Annee;

            string sql = Id == null ? SqlOrgane.SelectEntiteWithDetails : SqlOrgane.SelectEntiteWithDetailsById;

            return SqlDataAccess.SelectData<Organe>(sql, new { Id , Annee });
        }

        public static bool Insert(UpSetOrganeRequest upSetOrganeValues, Identity user)
        {

            string idOrganeParent = upSetOrganeValues.IdOrganeParent;

            if(idOrganeParent != null)
            {
                Organe organeParent = SelectById(upSetOrganeValues.IdOrganeParent);
                upSetOrganeValues.Ordre = organeParent.Ordre;
                upSetOrganeValues.IdStructure = upSetOrganeValues.Entite ? upSetOrganeValues.Id : organeParent.IdStructure;
            }
            else
            {
                upSetOrganeValues.Ordre = 1;
                upSetOrganeValues.IdStructure = upSetOrganeValues.Id;
            }
                        
            upSetOrganeValues.Actif = true;

            int nbreRow = SqlDataAccess.SaveData(SqlOrgane.Insert, upSetOrganeValues, user);

            return nbreRow > 0;
        }

        public static bool Update(UpSetOrganeRequest upSetOrganeValues, Identity user)
        {
            Organe organeParent = SelectById(upSetOrganeValues.IdOrganeParent);

            //upSetOrganeValues.Ordre = organeParent.Ordre;

            if(upSetOrganeValues.IdOrganeParent != null)
            {
                upSetOrganeValues.IdStructure = upSetOrganeValues.Entite ? upSetOrganeValues.Id : organeParent.IdStructure;
            }
            else
            {
                upSetOrganeValues.IdStructure = upSetOrganeValues.Id;
            }            

            int nbreRow = SqlDataAccess.SaveData(SqlOrgane.Update, upSetOrganeValues, user);

            return nbreRow > 0;
        }

    
        /// <summary>Persistance du glisser-déposer : nouveau parent et nouvel ordre de chaque organe, en une transaction.</summary>
        public static bool ReorderOrganigramme(OrganigrammeReorderRequest request, Identity user)
        {
            if (request == null || request.Items == null || request.Items.Count == 0) return false;

            List<(string, object)> batch = new List<(string, object)>();
            foreach (OrganeReorderRequest item in request.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                string parent = string.IsNullOrWhiteSpace(item.Parent) ? null : item.Parent.Trim();
                batch.Add((SqlOrgane.UpdateParentOrdre, (object)new { Id = item.Id.Trim(), Parent = parent, Ordre = item.Ordre }));
            }

            return SqlDataAccess.SaveDataWithTransaction(batch, user) > 0;
        }

        /// <summary>Éléments rattachés à un organe (sous-organes, locaux, utilisateurs, inventaires).</summary>
        public static OrganeDependances Dependances(string id)
        {
            return SqlDataAccess.SelectData<OrganeDependances>(SqlOrgane.SelectDependances, new { Id = id })?.FirstOrDefault() ?? new OrganeDependances();
        }

        public static bool Delete(string id, Identity user)
        {
            return SqlDataAccess.SaveData(SqlOrgane.Delete, new { Id = id }, user) == 1;
        }

}
}