using System.Collections.Generic;
using System.Linq;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class DeviseController
    {
        public static List<Devise> SelectAll()
        {
            return SqlDataAccess.SelectData<Devise>(SqlDevise.SelectAll) ?? new List<Devise>();
        }

        public static List<Devise> SelectActives()
        {
            return SqlDataAccess.SelectData<Devise>(SqlDevise.SelectActives) ?? new List<Devise>();
        }

        public static Devise SelectByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return SqlDataAccess.SelectData<Devise>(SqlDevise.SelectByCode, new { Code = code.Trim().ToUpper() })?.FirstOrDefault();
        }

        /// <summary>Devise de référence (taux 1). Repli sur USD si la table est vide.</summary>
        public static Devise Reference()
        {
            return SqlDataAccess.SelectData<Devise>(SqlDevise.SelectReference)?.FirstOrDefault()
                   ?? new Devise { Code = "USD", Libelle = "Dollar américain", Symbole = "$", Taux = 1, EstReference = true, Actif = true };
        }

        public static bool Insert(AddDeviseRequest values, Identity user)
        {
            Normaliser(values, user);

            bool result = SqlDataAccess.SaveData(SqlDevise.Insert, values, user) == 1;
            if (result && values.EstReference) AppliquerReference(values.Code, user);

            return result;
        }

        public static bool Update(ModifyDeviseRequest values, Identity user)
        {
            Normaliser(values, user);

            Devise actuelle = SelectByCode(values.Code);
            if (actuelle == null) return false;

            // La devise de référence ne peut être ni désactivée ni perdre son statut sans qu'une autre la remplace.
            if (actuelle.EstReference && !values.EstReference) values.EstReference = true;
            if (values.EstReference) { values.Taux = 1; values.Actif = true; }

            bool result = SqlDataAccess.SaveData(SqlDevise.Update, values, user) == 1;
            if (result && values.EstReference) AppliquerReference(values.Code, user);

            return result;
        }

        private static void Normaliser(AddDeviseRequest values, Identity user)
        {
            values.Code = (values.Code ?? "").Trim().ToUpper();
            values.Libelle = (values.Libelle ?? "").Trim();
            values.Symbole = string.IsNullOrWhiteSpace(values.Symbole) ? null : values.Symbole.Trim();
            values.UserMaj = user?.UserName;
            if (values.EstReference) { values.Taux = 1; values.Actif = true; }
        }

        private static void AppliquerReference(string code, Identity user)
        {
            SqlDataAccess.SaveData(SqlDevise.ResetReference, new { Code = code }, user);
            SqlDataAccess.SaveData(SqlDevise.ForceTauxReference, new { Code = code }, user);
        }
    }
}
