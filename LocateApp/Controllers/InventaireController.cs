using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class InventaireController
    {

        public static List<Inventaire> SelectEntete(int? Annee = null)
        {
            string sql = Annee == null ? SqlInventaire.SelectAllEntete : SqlInventaire.SelectEnteteById;

            return SqlDataAccess.SelectData<Inventaire>(sql, new { Annee });
        }

        public static List<Inventaire> SelectEnteteCloturer(int? Annee = null)
        {
            string sql = Annee == null ? SqlInventaire.SelectAllEnteteCloturer : SqlInventaire.SelectEnteteCloturerById;

            return SqlDataAccess.SelectData<Inventaire>(sql, new { Annee });
        }

        public static List<InventaireDetails> SelectDetails(int Annee)
        {
            return SqlDataAccess.SelectData<InventaireDetails>(SqlInventaire.SelectDetails, new { Annee });
        }

        public static List<InventaireDetailsOrgane> SelectDetailsParOrgane(int Annee)
        {
            return SqlDataAccess.SelectData<InventaireDetailsOrgane>(SqlInventaire.SelectDetailsParOrgane, new { Annee });
        }

        public static List<InventaireDetails> SelectDetailsByOrgane(int Annee, string CodeOrgane)
        {
            return SqlDataAccess.SelectData<InventaireDetails>(SqlInventaire.SelectDetailsByOrgane, new { Annee, CodeOrgane });
        }

        public static InventaireDetails SelectDetailsByImmo(long IdImmo, int? Annee = null)
        {
            Annee = Annee == null ? SelectLastAnneeComptable() : Annee;

            return SqlDataAccess.SelectData<InventaireDetails>(SqlInventaire.SelectDetailsByImmo, new { Annee, IdImmo }).FirstOrDefault();
        }

        public static int SelectAnneeEnCours()
        {
            return SqlDataAccess.SelectData<int>(SqlInventaire.SelectAnneeEnCours).FirstOrDefault();
        }

        public static int SelectLastAnneeComptable()
        {
            return SqlDataAccess.SelectData<int>(SqlInventaire.SelectLastAnneeComptable).FirstOrDefault();
        }

        public static long SelectQuantite(int? Annee = null)
        {
            long quantite = 0;

            Annee = Annee == null ? SelectAnneeEnCours() : Annee;

            if (Annee != null && Annee > 0)
            {
                quantite = SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantite, new { Annee }).FirstOrDefault();
            }

            return quantite;
        }

        public static long SelectQuantiteByEtat(string Etat, int? Annee = null)
        {
            Annee = Annee == null ? SelectLastAnneeComptable() : Annee;

            return SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteByEtat, new { Etat, Annee }).FirstOrDefault();
        }

        public static long SelectQuantiteNonVu( int? Annee = null)
        {
            Annee = Annee == null ? SelectLastAnneeComptable() : Annee;

            return SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteNonVu, new { Annee }).FirstOrDefault();
        }

        public static long SelectQuantiteByArticle(long idArticle, int? anneeEnCours = null)
        {
            long quantite = 0;
            anneeEnCours = anneeEnCours == null ? SelectAnneeEnCours() : anneeEnCours;

            if(anneeEnCours!= null && anneeEnCours > 0)
            {
                SelectQuantiteImmoByArticleRequest SelectQuantiteImmoByArticleValues = new SelectQuantiteImmoByArticleRequest()
                {
                    Annee = (int)anneeEnCours,
                    IdArticle = idArticle
                };
                quantite = SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteImmoByArticle, SelectQuantiteImmoByArticleValues).FirstOrDefault();
            }

            return quantite;
        }

        public static long SelectQuantiteByArticleAndLocal(long idArticle, long idLocal, int? anneeEnCours = null)
        {
            long quantite = 0;
            anneeEnCours = anneeEnCours == null ? SelectAnneeEnCours() : anneeEnCours;

            if (anneeEnCours != null && anneeEnCours > 0)
            {
                SelectQuantiteImmoByArticleAndLocalRequest SelectQuantiteImmoByArticleAndLocalValues = new SelectQuantiteImmoByArticleAndLocalRequest()
                {
                    Annee = (int)anneeEnCours,
                    IdArticle = idArticle,
                    IdLocal = idLocal
                };
                quantite = SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteImmoByArticleAndLocal, SelectQuantiteImmoByArticleAndLocalValues).FirstOrDefault();
            }

            return quantite;
        }

        public static long SelectQuantiteByLocal(long idLocal, int? anneeEnCours = null)
        {
            long quantite = 0;

            anneeEnCours = anneeEnCours == null ? SelectAnneeEnCours() : anneeEnCours;

            if (anneeEnCours != null && anneeEnCours > 0)
            {
                SelectQuantiteImmoByLocalRequest SelectQuantiteImmoByLocalValues = new SelectQuantiteImmoByLocalRequest()
                {
                    Annee = (int)anneeEnCours,
                    IdLocal = idLocal
                };
                quantite = SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteImmoByLocal, SelectQuantiteImmoByLocalValues).FirstOrDefault();
            }

            return quantite;
        }

        public static long SelectQuantiteByOrgance(string codeOrgane, int? anneeEnCours = null)
        {
            long quantite = 0;

            anneeEnCours = anneeEnCours == null ? SelectAnneeEnCours() : anneeEnCours;

            if (anneeEnCours != null && anneeEnCours > 0)
            {
                SelectQuantiteImmoByOrganeRequest SelectQuantiteImmoByOrganeValues = new SelectQuantiteImmoByOrganeRequest()
                {
                    Annee = (int)anneeEnCours,
                    CodeOrgane = codeOrgane
                };
                quantite = SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteImmoByOrgane, SelectQuantiteImmoByOrganeValues).FirstOrDefault();
            }

            return quantite;
        }

        public static long SelectQuantiteByResponsable(string userName, int? anneeEnCours = null)
        {
            long quantite = 0;

            anneeEnCours = anneeEnCours == null ? SelectAnneeEnCours() : anneeEnCours;

            if (anneeEnCours != null && anneeEnCours > 0)
            {
                SelectQuantiteImmoByResponsableRequest SelectQuantiteImmoByResponsableValues = new SelectQuantiteImmoByResponsableRequest()
                {
                    Annee = (int)anneeEnCours,
                    Responsable = userName
                };
                quantite = SqlDataAccess.SelectData<long>(SqlInventaire.SelectQuantiteImmoByResponsable, SelectQuantiteImmoByResponsableValues).FirstOrDefault();
            }

            return quantite;
        }

        public static bool InsertDetails(InsertInventaireDetailsRequest InsertInventaireDetailsValues, Identity user)
        {
            int nbreOfRow = 0;
            Immo immo = ImmoController.SelectById(InsertInventaireDetailsValues.IdImmo);

            if (!string.IsNullOrEmpty(immo.UserVu))
            {
                Local local = LocalController.SelectById(InsertInventaireDetailsValues.IdLocal).FirstOrDefault();

                InsertInventaireDetailsValues.UserCreation = user.UserName;
                InsertInventaireDetailsValues.Annee = SelectAnneeEnCours();
                InsertInventaireDetailsValues.DesignationLocal = local.Designation;

                List<(string, object)> sqlAndData = new List<(string, object)>();

                sqlAndData.Add((SqlInventaire.InsertDetails, InsertInventaireDetailsValues));

                sqlAndData.Add((SqlImmo.Inventaire, InsertInventaireDetailsValues));

                nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);
            }

            return nbreOfRow > 0;

        }

        public static bool InsertDetailsLocal(InsertInventaireLocalRequest InsertInventaireDetailsLocalValues, Identity user)
        {

            Local local = LocalController.SelectById(InsertInventaireDetailsLocalValues.IdLocal).FirstOrDefault();

            InsertInventaireDetailsLocalValues.UserCreation = user.UserName;
            InsertInventaireDetailsLocalValues.Annee = SelectAnneeEnCours();
            InsertInventaireDetailsLocalValues.CodeOrgane = local.CodeOrgane;

            List<(string, object)> sqlAndData = new List<(string, object)>();

            sqlAndData.Add((SqlInventaire.InsertDetailsLocal, InsertInventaireDetailsLocalValues));

            sqlAndData.Add((SqlImmo.InventaireLocal, InsertInventaireDetailsLocalValues));

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;

        }


        public static bool InsertDetailsNonVu(DeclareImmoNonVuRequest declareImmoNonVuValues, Identity user)
        {
            Immo immo = ImmoController.SelectById(declareImmoNonVuValues.IdImmo);
            Local local = LocalController.SelectById((long)immo.IdLocal).FirstOrDefault();

            InsertInventaireDetailsNonVuRequest InsertInventaireDetailsValues = new InsertInventaireDetailsNonVuRequest()
            {
                Annee = SelectAnneeEnCours(),
                IdImmo = immo.Id,
                ImmoExist = false,
                Etat = null,
                IdObservation = null,
                UserCreation = user.UserName,
                Responsable = immo.Responsable,
                IdLocal = (long)immo.IdLocal,
                CodeOrgane = immo.GetLocal().CodeOrgane,
                Observation = declareImmoNonVuValues.Observation,
                DesignationLocal = local.Designation
            };

            List<(string, object)> sqlAndData = new List<(string, object)>();

            sqlAndData.Add((SqlInventaire.InsertDetailsNonVu, InsertInventaireDetailsValues));

            sqlAndData.Add((SqlImmo.InventaireNonVu, InsertInventaireDetailsValues));

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;

        }

        public static bool InsertEntete(LancementInventaireRequest lancementInventaireValues, Identity user)
        {
            lancementInventaireValues.UserCreation = user.UserName;

            //int nbreRow = SqlDataAccess.SaveData(SqlInventaire.InsertEntete, lancementInventaireValues, user);


            List<(string, object)> sqlAndData = new List<(string, object)>();

            sqlAndData.Add((SqlInventaire.InsertEntete, lancementInventaireValues));
            sqlAndData.Add((SqlImmo.InventaireReset, lancementInventaireValues));

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);


            return nbreOfRow > 0;
        }

        public static bool IdentifierImmo(IdentifierImmoRequest IdentifierImmoValues, Identity user)
        {
            IdentifierImmoValues.UserVu = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlImmo.IdentifierImmo, IdentifierImmoValues, user);
            return nbreRow > 0;
        }

        public static bool UpdateEntete(ModifyInventaireRequest modifyInventaireValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlInventaire.UpdateEntete, modifyInventaireValues, user);
            return nbreRow > 0;
        }

        public static bool Cloture(int annee, Identity user)
        {
            CloturenventaireRequest clotureInventaireValues = new CloturenventaireRequest()
            {
                Annee = annee,
                UserCloture = user.UserName
            };

            int nbreRow = SqlDataAccess.SaveData(SqlInventaire.Cloture, clotureInventaireValues, user);
            return nbreRow > 0;
        }

    }
}