using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class ImmoController
    {
        public static List<Immo> Select(string Code = null, bool fetchAll = false)
        {
            string sql;
            long lCode = 0;

            if (fetchAll)
            {
                sql = Code == null ? SqlImmo.SelectAll : SqlImmo.SelectByCode;
            }
            else
            {
                sql = Code == null ? SqlImmo.SelectAllActive : SqlImmo.SelectActiveByCode;
                
            }

            if (Code != null && !long.TryParse(Code, out lCode)) Code = $"%{Code}%";

            if (lCode != 0)
            {
                return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByLongIds, new { IdEtiquette = lCode });
            }

            return SqlDataAccess.SelectData<Immo>(sql, new { Code });
        }

        public static Immo SelectById(long Id)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectById, new { Id }).FirstOrDefault();
        }

        public static string SelectNameById(long Id)
        {
            return SqlDataAccess.SelectData<string>(SqlImmo.SelectNameById, new { Id }).FirstOrDefault();
        }

        public static List<Immo> SelectByArticle(long IdArticle)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByArticle, new { IdArticle });
        }

        public static List<Immo> SelectByOrgane(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByOrgane, new { CodeOrgane });
        }

        public static List<Immo> SelectByEntite(string CodeOrgane)
        {
            string sql = CodeOrgane == null ? SqlImmo.SelectAllByEntite : SqlImmo.SelectByEntite;

            return SqlDataAccess.SelectData<Immo>(sql, new { CodeOrgane });
        }

        public static List<Immo> SelectByQrCode(Guid QrCode)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByQrCode, new { QrCode });
        }
        public static List<GetImmoRequest> SelectByQrCodeWithDetails(Guid QrCode)
        {
            return SqlDataAccess.SelectData<GetImmoRequest>(SqlImmo.SelectByQrCode, new { QrCode });
        }

        public static Immo SelectByIdEtiquette(long IdEtiquette)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByIdEtiquette, new { IdEtiquette }).FirstOrDefault();
        }

        public static List<Immo> SelectByLocal(long IdLocal)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByLocal, new { IdLocal });
        }

        public static List<Immo> SelectByLocalAndArticle(long IdLocal, long IdArticle)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByLocalAndArticle, new { IdLocal, IdArticle });
        }
        public static List<Immo> SelectByResponsable(string Responsable)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectByResponsable, new { Responsable });
        }

        public static List<Immo> SelectLitigeByResponsable(string Responsable)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectLitigeByResponsable, new { Responsable });
        }
        public static long SelectNbreBienByResponsable(string Responsable)
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectNbreBienByResponsable, new { Responsable }).FirstOrDefault();
        }

        public static long SelectNbreBienLitigieuxByResponsable(string Responsable)
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectNbreBienLitigieuxByResponsable, new { Responsable }).FirstOrDefault();
        }
        public static List<Immo> SelectImmoPrincipalByLocal(long IdLocal)
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectImmoPrincipalByLocal, new { IdLocal });
        }

        public static List<Immo> SelectSansLocal()
        {
            return SqlDataAccess.SelectData<Immo>(SqlImmo.SelectSansLocal);
        }
        public static List<string> SelectResponsableByOrgane(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<string>(SqlImmo.SelectResponsableByOrgane, new { CodeOrgane });
        }

        public static List<ResponsabletRequest> SelectResponsableLitigieux(string CodeOrgane = null)
        {
            string sql = CodeOrgane == null ? SqlImmo.SelectResponsableLitigieux : SqlImmo.SelectResponsableLitigieuxByOrgane;

            return SqlDataAccess.SelectData<ResponsabletRequest>(sql, new { CodeOrgane });
        }

        public static List<ImmoPhotos> SelectPhotosById(long Id)
        {
            return SqlDataAccess.SelectData<ImmoPhotos>(SqlImmo.SelectPhotosById, new { Id });
        }

        public static long SelectQuantiteByArticle(long IdArticle)
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantiteByArticle, new { IdArticle }).FirstOrDefault();
        }

        public static long SelectQuantiteByLocal(long IdLocal)
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantiteByLocal, new { IdLocal }).FirstOrDefault();
        }
        public static long SelectQuantiteImmoIdentifieByLocal(long IdLocal) 
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantiteIdentifierByLocal, new { IdLocal }).FirstOrDefault();
        }

        public static long SelectQuantite()
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantite).FirstOrDefault();
        }

        public static long SelectQuantiteExistant()
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantiteExistant).FirstOrDefault();
        }

        public static long SelectQuantiteByEtat(string LastEtat)
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantiteByEtat, new { LastEtat }).FirstOrDefault();
        }
        public static long SelectQuantiteIdentifie()
        {
            return SqlDataAccess.SelectData<long>(SqlImmo.SelectQuantiteIdentifie).FirstOrDefault();
        }
        public static long SelectQuantiteNonVu(int? Annee = null)
        {

            Annee = Annee == null ? InventaireController.SelectAnneeEnCours() : Annee;

            string sql = Annee == null ? SqlImmo.SelectQuantiteNonVu : SqlImmo.SelectQuantiteNonVuByAnnee;

            return SqlDataAccess.SelectData<long>(sql, new { Annee }).FirstOrDefault();
        }

        public static bool AffectQRCode(AffectQRCodeToImmoRequest AffectQRCodeRequest, Identity user)
        {
            Etiquette etiquette = EtiquetteController.SelectPrintedByQrCode(AffectQRCodeRequest.QrCode).FirstOrDefault();
            AffectQRCodeRequest.IdEtiquette = etiquette.Id;

            int idTypeEtiquette = 1; //un bien

            List<(string, object)> sqlAndData = new List<(string, object)>();

            sqlAndData.Add((SqlImmo.AffectQRCode, AffectQRCodeRequest));
            sqlAndData.Add((SqlEtiquette.UseIt, new { UsedBy = user.UserName, QrCode = AffectQRCodeRequest.QrCode, IdEtiquetteType = idTypeEtiquette }));

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }
        public static bool DesAffectQRCode(Immo immo, bool liberate, Identity user)
        {
            bool result = false;

            if(immo != null)
            {
                List<(string, object)> sqlAndData = new List<(string, object)>();

                sqlAndData.Add((SqlImmo.DesaffectQRCode, new { immo.Id }));

                if (liberate)
                {
                    sqlAndData.Add((SqlEtiquette.LibereIt, new { immo.QrCode }));
                }

                int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

                result = nbreOfRow > 0;
            }            

            return result;
        }

        public static bool InsertImmo(InsertImmoRequest insertImmoValues, Identity user, out long id)
        {
            id = 0;
            insertImmoValues.UserCreation = user.UserName;
            insertImmoValues.LastAnneeComptable = InventaireController.SelectAnneeEnCours();
            insertImmoValues.Nbre = insertImmoValues.Nbre <= 0 ? 1 : insertImmoValues.Nbre;

            if (insertImmoValues.IdImmoParent != null)
            {
                Immo immoParent = SelectById((long)insertImmoValues.IdImmoParent);
                insertImmoValues.IdImmoPrincipal = immoParent.IdImmoPrincipal;
            }


            var result = SqlDataAccess.QueryMultipleUsingStoredProcedure(SqlImmo.Insert, insertImmoValues);

            if (result != null)
            {
                var resultMessageObject = result.Read().FirstOrDefault();
                id = resultMessageObject.Id;
            }

            return id > 0;
        }

        public static bool InsertImmoSync(InsertImmoSyncRequest insertImmoSyncValues, Identity user, out long id)
        {
            id = 0;
            insertImmoSyncValues.UserCreation = user.UserName;
            insertImmoSyncValues.LastAnneeComptable = InventaireController.SelectAnneeEnCours();

            if (insertImmoSyncValues.IdImmoParent != null && insertImmoSyncValues.IdImmoParent !=0)
            {
                Immo immoParent = SelectById((long)insertImmoSyncValues.IdImmoParent);
                insertImmoSyncValues.IdImmoPrincipal = immoParent.IdImmoPrincipal;
            }
            else
            {
                insertImmoSyncValues.IdImmoParent = null;
            }


            var result = SqlDataAccess.QueryMultipleUsingStoredProcedure(SqlImmo.ImmoAndInventaireDetailsInsert, insertImmoSyncValues);

            if (result != null)
            {
                var resultMessageObject = result.Read().FirstOrDefault();
                id = resultMessageObject.Id;
            }

           return id > 0;
           
        }

        public static bool UpdateImmo(ModifyImmoRequest modifyImmoRequest, Identity user)
        {
            modifyImmoRequest.UserCreation = user.UserName;
            modifyImmoRequest.DateCreation = DateTime.Today;

            if (string.IsNullOrEmpty(modifyImmoRequest.Responsable))
            {
                modifyImmoRequest.Responsable = null;
            }

            if (modifyImmoRequest.IdImmoParent != null && modifyImmoRequest.IdImmoParent != 0)
            {
                Immo immoParent = SelectById((long)modifyImmoRequest.IdImmoParent);
                modifyImmoRequest.IdImmoPrincipal = immoParent.IdImmoPrincipal;
            }


            List<(string, object)> sqlAndData = new List<(string, object)>();
            sqlAndData.Add((SqlImmo.Update, modifyImmoRequest));


            if (modifyImmoRequest.IdPhoto != null)
            {
                List<string> idPhoto = modifyImmoRequest.IdPhoto.Split(',').ToList();
                List<string> constat = modifyImmoRequest.ConstatToModify.Split(',').ToList();

                for (int i = 0; i < idPhoto.Count; i++)
                {
                    sqlAndData.Add((SqlImmo.UpdatePhoto, new { Constat = constat[i], Id = Guid.Parse(idPhoto[i]) }));
                }
               
            }

            if(modifyImmoRequest.IdPhotoToDelete != null)
            {
                List<string> idPhotoToDelete = modifyImmoRequest.IdPhotoToDelete.Split(',').ToList();

                for (int i = 0; i < idPhotoToDelete.Count; i++)
                {
                    sqlAndData.Add((SqlImmo.DeletePhoto, new { Id = Guid.Parse(idPhotoToDelete[i]) }));
                }
            }

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }

        public static bool UpdateImmoSync(ModifyImmoSyncRequest modifyImmoRequest, Identity user)
        {
            modifyImmoRequest.UserCreation = user.UserName;
            modifyImmoRequest.DateCreation = DateTime.Today;

            if (string.IsNullOrEmpty(modifyImmoRequest.Responsable))
            {
                modifyImmoRequest.Responsable = null;
            }

            if (modifyImmoRequest.IdImmoParent != null && modifyImmoRequest.IdImmoParent != 0)
            {
                Immo immoParent = SelectById((long)modifyImmoRequest.IdImmoParent);
                modifyImmoRequest.IdImmoPrincipal = immoParent.IdImmoPrincipal;
            }

            int idTypeEtiquette = 1; //un bien

            List<(string, object)> sqlAndData = new List<(string, object)>();
            sqlAndData.Add((SqlImmo.Update, modifyImmoRequest));
            sqlAndData.Add((SqlImmo.AffectQRCode, modifyImmoRequest));
            sqlAndData.Add((SqlEtiquette.UseIt, new { UsedBy = user.UserName, QrCode = modifyImmoRequest.QrCode, IdEtiquetteType = idTypeEtiquette }));

            InsertInventaireDetailsRequest InsertInventaireDetailsValues = new InsertInventaireDetailsRequest() 
            {
                IdImmo = modifyImmoRequest.Id,
                ImmoExist = modifyImmoRequest.ImmoExist,
                Etat = modifyImmoRequest.LastEtat,
                IdObservation = modifyImmoRequest.IdLastObservation,
                Responsable = modifyImmoRequest.Responsable,
                IdLocal = modifyImmoRequest.IdLocal                
            };


            if (modifyImmoRequest.Inventorieur != null)
            {
                Local local = LocalController.SelectById(modifyImmoRequest.IdLocal).FirstOrDefault();

                InsertInventaireDetailsValues.UserCreation = user.UserName;
                InsertInventaireDetailsValues.Annee = InventaireController.SelectAnneeEnCours();
                InsertInventaireDetailsValues.CodeOrgane = local.CodeOrgane;
                InsertInventaireDetailsValues.DesignationLocal = local.Designation;

                sqlAndData.Add((SqlInventaire.InsertDetails, InsertInventaireDetailsValues));
                sqlAndData.Add((SqlImmo.Inventaire, InsertInventaireDetailsValues));
            }


            if (modifyImmoRequest.IdPhoto != null)
            {
                List<string> idPhoto = modifyImmoRequest.IdPhoto.Split(',').ToList();
                List<string> constat = modifyImmoRequest.ConstatToModify.Split(',').ToList();

                for (int i = 0; i < idPhoto.Count; i++)
                {
                    sqlAndData.Add((SqlImmo.UpdatePhoto, new { Constat = constat[i], Id = Guid.Parse(idPhoto[i]) }));
                }

            }

            if (modifyImmoRequest.IdPhotoToDelete != null)
            {
                List<string> idPhotoToDelete = modifyImmoRequest.IdPhotoToDelete.Split(',').ToList();

                for (int i = 0; i < idPhotoToDelete.Count; i++)
                {
                    sqlAndData.Add((SqlImmo.DeletePhoto, new { Id = Guid.Parse(idPhotoToDelete[i]) }));
                }
            }

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }
        public static bool InsertPhoto(InsertPhotoImmoRequest InsertPhotoImmoValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlImmo.InsertPhoto, InsertPhotoImmoValues, user);

            return nbreRow > 0;
        }

        public static bool ChangeLocal(ChangeLocalImmoRequest ChangeLocalImmoValues, Identity user)
        {
            Local localDestination = LocalController.SelectById(ChangeLocalImmoValues.IdLocal).FirstOrDefault();
            Immo immo = ImmoController.SelectById(ChangeLocalImmoValues.Id);
            Local localOrigine = LocalController.SelectById((long)immo.IdLocal).FirstOrDefault();

            Organe organeOrigine = OrganeController.SelectById(localOrigine.CodeOrgane);

            Organe entiteOrigine = OrganeController.SelectById(organeOrigine.IdStructure);

            string nomEntite = organeOrigine.Id != entiteOrigine.Id ? $" / {entiteOrigine.Nom}" : "";

            ChangeLocalImmoValues.DesignationLocal = localDestination.IdTypeLocal != 1 ? $"{localOrigine.Designation} ({organeOrigine.Nom} {nomEntite})" : null;

            int nbreRow = SqlDataAccess.SaveData(SqlImmo.ChangeLocal, ChangeLocalImmoValues, user);

            return nbreRow > 0;
        }

        public static bool MisEnService(MisEnServiceImmoRequest MisEnServiceImmoValues, Identity user)
        {
            MisEnServiceImmoValues.UserMisEnService = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlImmo.MisEnService, MisEnServiceImmoValues, user);

            return nbreRow > 0;
        }

        public static bool Declassement(ChangeLocalDeclassementImmoRequest ChangeLocalDeclassementImmoValues, Identity user)
        {

            Local localDestination = LocalController.SelectById(ChangeLocalDeclassementImmoValues.IdLocal).FirstOrDefault();
            Immo immo = ImmoController.SelectById(ChangeLocalDeclassementImmoValues.Id);
            Local localOrigine = LocalController.SelectById((long)immo.IdLocal).FirstOrDefault();

            Organe organeOrigine = OrganeController.SelectById(localOrigine.CodeOrgane);

            Organe entiteOrigine = OrganeController.SelectById(organeOrigine.IdStructure);

            string nomEntite = organeOrigine.Id != entiteOrigine.Id ? $" / {entiteOrigine.Nom}" : "";

            ChangeLocalDeclassementImmoValues.DesignationLocal = localDestination.IdTypeLocal != 1 ? $"{localOrigine.Designation} ({organeOrigine.Nom} {nomEntite})" : null;
            ChangeLocalDeclassementImmoValues.UserDeclassement = user.UserName;

            ChangeLocalDeclassementImmoValues.Annee = InventaireController.SelectAnneeEnCours();


            List<(string, object)> sqlAndData = new List<(string, object)>();

            sqlAndData.Add((SqlImmo.Declassement, ChangeLocalDeclassementImmoValues));
            sqlAndData.Add((SqlImmo.ChangeLocal, ChangeLocalDeclassementImmoValues));

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }

        public static bool Ceder(DeclassementImmoRequest DeclassementImmoValues, Identity user)
        {
            DeclassementImmoValues.UserDeclassement = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlImmo.Cession, DeclassementImmoValues, user);

            return nbreRow > 0;
        }

        public static bool CederLocal(DeclassementLocalImmoRequest DeclassementLocalImmoValues, Identity user)
        {
            DeclassementLocalImmoValues.UserDeclassement = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlImmo.CessionLocal, DeclassementLocalImmoValues, user);

            return nbreRow > 0;
        }

    }
}