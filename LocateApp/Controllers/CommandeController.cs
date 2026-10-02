using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class CommandeController
    {
        public static List<Commande> Select(Guid? Id = null)
        {
            string sql = Id == null ? SqlCommande.SelectAll : SqlCommande.SelectById;

            return SqlDataAccess.SelectData<Commande>(sql, new { Id });
        }

        public static List<Commande> SelectPrinted(Guid? Id = null)
        {
            string sql = Id == null ? SqlCommande.SelectAllPrint : SqlCommande.SelectPrintById;

            return SqlDataAccess.SelectData<Commande>(sql, new { Id });
        }

        public static int SelectNbreEtiquette(Guid IdCommande)
        {
            return SqlDataAccess.SelectData<int>(SqlCommande.SelectNbreEtiquette, new { IdCommande }).FirstOrDefault();
        }

        public static int SelectNbreEtiquetteUsed(Guid IdCommande)
        {
            return SqlDataAccess.SelectData<int>(SqlCommande.SelectNbreEtiquetteUsed, new { IdCommande }).FirstOrDefault();
        }

        public static int SelectNbreEtiquetteNotUsed(Guid IdCommande)
        {
            return SqlDataAccess.SelectData<int>(SqlCommande.SelectNbreEtiquetteNotUsed, new { IdCommande }).FirstOrDefault();
        }

        public static bool Insert(CommandeInsertRequest insertCommande, Identity user)
        {
            insertCommande.CreatedBy = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlCommande.Insert, insertCommande, user);

            return nbreRow > 0;
        }

        public static bool InsertUsingStoredProcedure(CommandeInsertRequest insertCommande, Identity user)
        {
            insertCommande.CreatedBy = user.UserName;

            int nbreRow = SqlDataAccess.SaveDataUsingStoredProcedure(SqlCommande.InsertUsingStoredProcedure, insertCommande, user);

            return nbreRow > 0;
        }

        public static bool Update(CommandeUpdateRequest updateCommande, Identity user)
        {
            updateCommande.CreatedBy = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlCommande.Update, updateCommande, user);

            return nbreRow > 0;
        }
                
    }
}