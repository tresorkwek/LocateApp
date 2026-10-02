using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.Utilities;

namespace LocateApp.Controllers
{
    public static class OrganeVersionController
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(OrganeVersionController));

        public static List<OrganeVersion> Select(int? Id = null)
        {
            string sql = Id == null ? SqlOrganeVersion.SelectAll : SqlOrganeVersion.SelectById;
            return SqlDataAccess.SelectData<OrganeVersion>(sql, new { Id }) ?? new List<OrganeVersion>();
        }

        public static OrganeVersion SelectDefault()
        {
            return SqlDataAccess.SelectData<OrganeVersion>(SqlOrganeVersion.SelectDefault)?.FirstOrDefault();
        }

        /// <summary>
        /// Crée une version, recopie éventuellement les organes d'une version source (codes re-préfixés par le
        /// nouveau numéro) et la définit comme courante si demandé, le tout dans une transaction.
        /// Retourne l'identifiant créé, 0 en cas d'échec.
        /// </summary>
        public static int Insert(InsertOrganeVersionRequest values, Identity user, out int nbreOrganesCopies)
        {
            nbreOrganesCopies = 0;
            int id = 0;

            using (IDbConnection connection = SqlDataAccess.GetConnexion())
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        id = connection.QuerySingle<int>(SqlOrganeVersion.Insert, new
                        {
                            Defaut = false,
                            Libelle = string.IsNullOrWhiteSpace(values.Libelle) ? null : values.Libelle.Trim(),
                            Commentaire = string.IsNullOrWhiteSpace(values.Commentaire) ? null : values.Commentaire.Trim(),
                            UserCreation = user?.UserName
                        }, transaction);

                        if (values.CopierDepuis.HasValue && values.CopierDepuis.Value != id)
                        {
                            string prefix = id.ToString("00");
                            nbreOrganesCopies = connection.Execute(SqlOrganeVersion.CopierOrganes, new { Prefix = prefix, Version = id, Source = values.CopierDepuis.Value }, transaction);
                        }

                        // On ne bascule que sur une version qui contient des organes.
                        if (values.Defaut && nbreOrganesCopies > 0)
                        {
                            connection.Execute(SqlOrganeVersion.ResetAllDefault, transaction: transaction);
                            connection.Execute(SqlOrganeVersion.SetDefault, new { Id = id }, transaction);
                        }

                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    id = 0;
                    Log.Error(user, $"Création de la version d'organigramme impossible : {e.Message}");
                }
            }

            return id;
        }

        /// <summary>Met à jour le libellé et le commentaire ; bascule la version courante si demandé et possible.</summary>
        public static bool Update(ModifyOrganeVersionRequest values, Identity user)
        {
            OrganeVersion version = Select(values.Id).FirstOrDefault();
            if (version == null) return false;

            List<(string, object)> batch = new List<(string, object)>
            {
                (SqlOrganeVersion.Update, (object)new { values.Id, Libelle = string.IsNullOrWhiteSpace(values.Libelle) ? null : values.Libelle.Trim(), Commentaire = string.IsNullOrWhiteSpace(values.Commentaire) ? null : values.Commentaire.Trim() })
            };

            if (values.Defaut && !version.Defaut && version.NbOrgane > 0)
            {
                batch.Add((SqlOrganeVersion.ResetAllDefault, (object)new { }));
                batch.Add((SqlOrganeVersion.SetDefault, (object)new { values.Id }));
            }

            return SqlDataAccess.SaveDataWithTransaction(batch, user) > 0;
        }

        /// <summary>Supprime une version vide et non courante.</summary>
        public static bool Delete(int id, Identity user)
        {
            return SqlDataAccess.SaveData(SqlOrganeVersion.Delete, new { Id = id }, user) == 1;
        }
    }
}
