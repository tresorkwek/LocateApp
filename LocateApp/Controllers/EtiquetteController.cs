using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class EtiquetteController
    {
        public static List<Etiquette> Select(Guid IdCommande)
        {
            return SqlDataAccess.SelectData<Etiquette>(SqlEtiquette.SelectAll, new { IdCommande });
        }
        public static List<Etiquette> SelectByQrCode(Guid QrCode)
        {
            return SqlDataAccess.SelectData<Etiquette>(SqlEtiquette.SelectByQrCode, new { QrCode });
        }

        public static List<Etiquette> SelectPrintedByQrCode(Guid? QrCode)
        {
            return SqlDataAccess.SelectData<Etiquette>(SqlEtiquette.SelectPrintedByQrCode, new { QrCode });
        }

        public static List<Etiquette> SelectUnusedPrinted()
        {
            return SqlDataAccess.SelectData<Etiquette>(SqlEtiquette.SelectUnusedPrinted);
        }

        public static bool Insert(Commande commande, Identity user)
        {

            List<(string, object)> sqlAndData = new List<(string, object)>();

            EtiquetteFormat formatEtiquette = commande.GetFormatEtiquette();

            int length = commande.NbrePage * formatEtiquette.Colone * formatEtiquette.Ligne;

            for (int i = 0; i < length; i++)
            {
                sqlAndData.Add((SqlEtiquette.Insert, new { IdCommande = commande.Id, IdFormat = commande.IdFormat,UserCreation = user.UserName }));
            }

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }

        public static bool InsertUsingStoredProcedure(Commande commande, Identity user)
        {

            List<(string, object)> sqlAndData = new List<(string, object)>();

            EtiquetteFormat formatEtiquette = commande.GetFormatEtiquette();

            int length = commande.NbrePage * formatEtiquette.Colone * formatEtiquette.Ligne;

            for (int i = 0; i < length; i++)
            {
                sqlAndData.Add((SqlEtiquette.InsertUsingStoredProcedure, new { IdCommande = commande.Id, IdFormat = commande.IdFormat, UserCreation = user.UserName }));
            }

            int nbreOfRow = SqlDataAccess.SaveDataWithTransactionUsingStoreProcedure(sqlAndData, user);

            bool result = nbreOfRow > 0;

            if (result)
            {
                commande.ValidateBy = user.UserName;
                _ = SqlDataAccess.SaveData(SqlCommande.Validate, new { commande.Id, commande.ValidateBy }, user);
            }


            return result;
        }

        public static bool Print(Commande commande, Identity user)
        {
            List<(string, object)> sqlAndData = new List<(string, object)>();
           
            sqlAndData.Add((SqlEtiquette.Print, new { IdCommande = commande.Id, UserPrint = user.UserName }));
            sqlAndData.Add((SqlCommande.Print, new { commande.Id, PrintBy = user.UserName }));            

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }

    }
}