using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class LocalController
    {
        public static List<Local> SelectAll(string Designation = null, bool fetchAll=false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Designation == null ? SqlLocal.SelectAll : SqlLocal.SelectByName;
            }
            else
            {
                sql = Designation == null ? SqlLocal.SelectAllActive : SqlLocal.SelectActiveByName;
            }

            Designation = Designation == null ? null : Designation + "%";

            return SqlDataAccess.SelectData<Local>(sql, new { Designation });
        }

        public static List<Local> SelectLocal(string Designation = null, bool fetchAll = false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Designation == null ? SqlLocal.SelectAllLocal : SqlLocal.SelectLocalByName;
            }
            else
            {
                sql = Designation == null ? SqlLocal.SelectAllLocalActive : SqlLocal.SelectLocalActiveByName;
            }

            Designation = Designation == null ? null : Designation + "%";

            return SqlDataAccess.SelectData<Local>(sql, new { Designation });
        }

        public static List<Local> SelectById(long Id)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectById, new { Id });
        }

        public static List<GetLocalRequest> SelectRequestById(long Id)
        {
            return SqlDataAccess.SelectData<GetLocalRequest>(SqlLocal.SelectById, new { Id });
        }

        public static List<Local> SelectAllByCodeOrgane(string CodeOrgane)
        {
            CodeOrgane = CodeOrgane == null ? null : CodeOrgane + "%";

            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectByOrgane, new { CodeOrgane });
        }
        public static List<Local> SelectLocalByCodeOrgane(string CodeOrgane)
        {
            CodeOrgane = CodeOrgane == null ? null : CodeOrgane + "%";

            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectLocalByOrgane, new { CodeOrgane });
        } 

        public static List<Local> SelectByQrCode(Guid QrCode)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectByQrCode, new { QrCode });
        }
        public static List<Local> SelectNonVu(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectNonVu, new { CodeOrgane });
        }

        public static List<Local> SelectDeclasser(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectDeclasser, new { CodeOrgane });
        }

        public static List<Local> SelectTransit(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectTransit, new { CodeOrgane });
        }

        public static List<GetLocalRequest> SelectRequestByQrCode(Guid QrCode)
        {
            return SqlDataAccess.SelectData<GetLocalRequest>(SqlLocal.SelectByQrCode, new { QrCode });
        }

        public static List<Local> SelectByIdEtiquette(long IdEtiquette)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectByIdEtiquette, new { IdEtiquette });
        }
        public static List<Local> SelectSpaceByQrCode(Guid QrCode)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectSpaceByQrCode, new { QrCode });
        }

        public static List<Local> SelectSpaceByIdEtiquette(long IdEtiquette)
        {
            return SqlDataAccess.SelectData<Local>(SqlLocal.SelectSpaceByIdEtiquette, new { IdEtiquette });
        }

        public static long SelectQuantite()
        {
            return SqlDataAccess.SelectData<long>(SqlLocal.SelectQuantite).FirstOrDefault();
        }

        public static long SelectQuantiteIdentifie()
        {
            return SqlDataAccess.SelectData<long>(SqlLocal.SelectQuantiteIdentifie).FirstOrDefault();
        }

        public static bool AffectQRCode(AffectQRCodeToLocalRequest AffectQRCodeRequest, Identity user)
        {
            Etiquette etiquette = EtiquetteController.SelectByQrCode(AffectQRCodeRequest.QrCode).FirstOrDefault();

            int nbreOfRow = 0;

            if (etiquette != null)
            {
                AffectQRCodeRequest.IdEtiquette = etiquette.Id;

                Local local = SelectById(AffectQRCodeRequest.Id).FirstOrDefault();

                if (local != null)
                {
                    int idTypeEtiquette = local.IsSpace ? 3 : 2; // 2: local , 3: espace

                    List<(string, object)> sqlAndData = new List<(string, object)>();

                    sqlAndData.Add((SqlLocal.AffectQRCode, AffectQRCodeRequest));
                    sqlAndData.Add((SqlEtiquette.UseIt, new { UsedBy = user.UserName, QrCode = AffectQRCodeRequest.QrCode, IdEtiquetteType = idTypeEtiquette }));

                    nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);
                }
            }            

            return nbreOfRow > 0;
        }

        public static bool DesAffectQRCode(Local local, bool liberate, Identity user)
        {
            bool result = false;

            if (local != null)
            {
                List<(string, object)> sqlAndData = new List<(string, object)>();

                sqlAndData.Add((SqlLocal.DesaffectQRCode, new { local.Id }));

                if (liberate)
                {
                    sqlAndData.Add((SqlEtiquette.LibereIt, new { local.QrCode }));
                }

                int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

                result = nbreOfRow > 0;
            }

            return result;
        }

        public static bool Insert(AddLocalRequest AddLocalValues, Identity user)
        {
            AddLocalValues.UserCreation = user.UserName;
            AddLocalValues.IdTypeLocal = AddLocalValues.IdTypeLocal == null || AddLocalValues.IdTypeLocal == 0 ? 1 : AddLocalValues.IdTypeLocal;
            int nbreRow = SqlDataAccess.SaveData(SqlLocal.Insert, AddLocalValues, user);
            return nbreRow > 0;
        }

        public static long InsertSync(AddLocalSyncRequest AddLocalValues, Identity user)
        {
            //List<(string, object)> sqlAndData = new List<(string, object)>();

            //sqlAndData.Add((SqlLocal.InsertSync, AddLocalValues));

            //if ()
            //{
            //    sqlAndData.Add((SqlEtiquette.UseIt, new { UsedBy = AddLocalValues.UserCreation, QrCode = AddLocalValues.QrCode, IdEtiquetteType = AddLocalValues.IdEtiquette }));

            //}


            //int nbreRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            if (AddLocalValues.IdEtiquette == 0)
            {
                AddLocalValues.IdEtiquette = null;
                AddLocalValues.QrCode = null;
            }

            long localId = 0;

            //var result = SqlDataAccess.QueryMultipleUsingStoredProcedure(SqlLocal.InsertSyncStoredProcedure, new { Code = AddLocalValues.Code, Designation = AddLocalValues.Designation, CodeOrgane = AddLocalValues.CodeOrgane, IsSpace = AddLocalValues.IsSpace, UserCreation = AddLocalValues.UserCreation, IdEtiquette = AddLocalValues.IdEtiquette, UsedBy = AddLocalValues.UserCreation, QrCode = AddLocalValues.QrCode, IdEtiquetteType = AddLocalValues.IdEtiquette }).FirstOrDefault();
            var result = SqlDataAccess.QueryMultipleUsingStoredProcedure(SqlLocal.InsertSyncStoredProcedure, new { Code = AddLocalValues.Code, Designation = AddLocalValues.Designation, CodeOrgane = AddLocalValues.CodeOrgane, IsSpace = AddLocalValues.IsSpace, UserCreation = AddLocalValues.UserCreation, IdEtiquette = AddLocalValues.IdEtiquette, UsedBy = AddLocalValues.UserCreation, QrCode = AddLocalValues.QrCode, IdEtiquetteType = AddLocalValues.IdEtiquette });
            localId = result.Read<long>().FirstOrDefault();
            return localId;
        }

        public static bool Update(ModifyLocalRequest modifyLocalValues, Identity user)
        {
            modifyLocalValues.IdTypeLocal = modifyLocalValues.IdTypeLocal == null || modifyLocalValues.IdTypeLocal == 0 ? 1 : modifyLocalValues.IdTypeLocal;
            int nbreRow = SqlDataAccess.SaveData(SqlLocal.Update, modifyLocalValues, user);
            return nbreRow > 0;
        }

        public static bool Delete(long Id, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlLocal.Delete, new { Id }, user);
            return nbreRow > 0;
        }
    }
}