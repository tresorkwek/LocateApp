using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class ArticleController
    {
        public static List<Article> Select(string Designation = null, bool fetchAll=false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Designation == null ? SqlArticle.SelectAll : SqlArticle.SelectByName;
            }
            else
            {
                sql = Designation == null ? SqlArticle.SelectAllActive : SqlArticle.SelectActiveByName;
            }

            return SqlDataAccess.SelectData<Article>(sql, new { Designation });
        }

        public static List<Article> SelectById(long Id)
        {
            return SqlDataAccess.SelectData<Article>(SqlArticle.SelectById, new { Id });
        }

        public static List<Article> SelectByLocal(long IdLocal)
        {
            return SqlDataAccess.SelectData<Article>(SqlArticle.SelectByLocal, new { IdLocal });
        }

        public static List<Article> SelectByOrgane(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<Article>(SqlArticle.SelectByOrgane, new { CodeOrgane });
        }

        public static List<Article> SelectByEntite(string CodeOrgane)
        {
            return SqlDataAccess.SelectData<Article>(SqlArticle.SelectByEntite, new { CodeOrgane });
        }

        public static List<GetArticleRequest> SelectRequesById(long Id)
        {
            return SqlDataAccess.SelectData<GetArticleRequest>(SqlArticle.SelectById, new { Id });
        }
        public static string SelectNameById(long Id)
        {
            return SqlDataAccess.SelectData<string>(SqlArticle.SelectNameById, new { Id }).FirstOrDefault();
        }

        public static bool Insert(AddArticleRequest AddArticleValues, Identity user)
        {
            AddArticleValues.UserCreation = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlArticle.Insert, AddArticleValues, user);
            return nbreRow > 0;
        }

        public static bool Insert(AddArticleRequest AddArticleValues, Identity user, out long id)
        {
            id = 0;
            AddArticleValues.UserCreation = user.UserName;

            //int nbreRow = SqlDataAccess.SaveData(SqlArticle.Insert, AddArticleValues, user);
            //return nbreRow > 0;

            var result = SqlDataAccess.QueryMultipleUsingStoredProcedure(SqlArticle.Insert, AddArticleValues);

            if (result != null)
            {
                var resultMessageObject = result.Read().FirstOrDefault();
                id = resultMessageObject.Id;
            }

            return id > 0;
        }

        public static bool Update(ModifyArticleRequest ModifyArticleValues, Identity user)
        {
            ModifyArticleValues.UserCreation = user.UserName;
            int nbreRow = SqlDataAccess.SaveData(SqlArticle.Update, ModifyArticleValues, user);
            return nbreRow > 0;
        }

        public static bool Delete(long Id, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlArticle.Delete, new { Id }, user);
            return nbreRow > 0;
        }

    }
}