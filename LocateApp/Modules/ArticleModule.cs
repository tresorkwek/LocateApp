using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.ModelBinding;
using Nancy.Security;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using LocateApp.Models;
using System.IO;

namespace LocateApp.Modules
{
    public class ArticleModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ArticleModule));

        public ArticleModule() : base("/article")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetArticle,null));
            Get("/select/{designation}", _ => this.RunHandler<string>(GetArticle, (string)_.designation));
            Get("/id/{id}", _ => this.RunHandler<long>(GetArticleById, (long)_.id));
            Get("/local/{idLocal}", _ => this.RunHandler<long>(GetArticleByLocal, (long)_.idLocal));
            Get("/organe/{codeOrgane}", _ => this.RunHandler<string>(GetArticleByOrgane, (string)_.codeOrgane));
            Get("/entite/{codeOrgane}", _ => this.RunHandler<string>(GetArticleByEntite, (string)_.codeOrgane));
            Get("/add/", _ => this.RunHandler(GetArticleAddForm));
            Get("/modify/{id}", _ => this.RunHandler<long>(GetModifyArticleForm, (long)_.id));
            Get("/delete/{id}", _ => this.RunHandler<long>(DeleteArticle, (long)_.id));

            Post("/add/", _ => this.RunHandler<AddArticleRequest>(AddArticle));
            Post("/modify/", _ => this.RunHandler<ModifyArticleRequest>(UpdateArticle));
        }

        private object GetArticle(string designation = null)
        {
            List<Article> listOfArticle = ArticleController.Select(designation);

            ArticleListViewModel viewModelOfArticle = new ArticleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Articles = listOfArticle
            };

            object view = View["ArticleListView", viewModelOfArticle];
            int success = listOfArticle.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfArticle, viewModelOfArticle.Title, success);
        }
        private object GetArticleById(long id)
        {
            Article article = ArticleController.SelectById(id).FirstOrDefault();

            ArticleViewModel viewModelOfArticle = new ArticleViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Article = article,
                Achats = AchatController.SelectAchatsByArticle(id)
            };

            object view = View["ArticleView", viewModelOfArticle];
            int success = article != null ? 1 : 0;

            return this.ResponseObject(view, article, viewModelOfArticle.Title, success);
        }

        private object GetArticleByLocal(long idLocal)
        {
            List<Article> listOfArticle = ArticleController.SelectByLocal(idLocal);

            ArticleListViewModel viewModelOfArticle = new ArticleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Articles = listOfArticle,
                Local = LocalController.SelectById(idLocal).FirstOrDefault(),
                Link = $"/immo/local/{idLocal}/article/"
            };

            object view = View["ArticleLocalListView", viewModelOfArticle];
            int success = listOfArticle != null ? 1 : 0;

            return this.ResponseObject(view, listOfArticle, viewModelOfArticle.Title, success);
        }

        /// <summary>Biens d'un organe groupés par article (pendant de /article/local/{idLocal}).</summary>
        private object GetArticleByOrgane(string codeOrgane)
        {
            return AfficherArticlesOrgane(codeOrgane, false);
        }

        /// <summary>Biens d'une entité (structure et organes rattachés) groupés par article.</summary>
        private object GetArticleByEntite(string codeOrgane)
        {
            return AfficherArticlesOrgane(codeOrgane, true);
        }

        private object AfficherArticlesOrgane(string codeOrgane, bool entite)
        {
            List<Article> listOfArticle = entite ? ArticleController.SelectByEntite(codeOrgane) : ArticleController.SelectByOrgane(codeOrgane);
            string portee = entite ? "entite" : "organe";

            // Inventoriés de l'inventaire en cours, par article, parmi les biens actifs de l'organe (ou de l'entité)
            int anneeEnCours = InventaireController.SelectAnneeEnCours();
            Dictionary<long, long> inventories = (entite ? ImmoController.SelectByEntite(codeOrgane) : ImmoController.SelectByOrgane(codeOrgane))
                .Where(i => i.IdArticle != null && anneeEnCours > 0 && i.LastAnneeComptable == anneeEnCours && !string.IsNullOrEmpty(i.UserVu) && !string.IsNullOrEmpty(i.Inventorieur))
                .GroupBy(i => (long)i.IdArticle)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            ArticleListViewModel viewModelOfArticle = new ArticleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Articles = listOfArticle,
                Organe = OrganeController.SelectById(codeOrgane),
                ParEntite = entite,
                InventoriesParArticle = inventories,
                Link = $"/immo/{portee}/select/{codeOrgane}/article/",
                LienParArticle = $"/article/{portee}/{codeOrgane}",
                LienDetails = $"/immo/{portee}/select/{codeOrgane}/"
            };

            object view = View["ArticleLocalListView", viewModelOfArticle];
            int success = listOfArticle != null ? 1 : 0;

            return this.ResponseObject(view, listOfArticle, viewModelOfArticle.Title, success);
        }

        private object GetArticleAddForm()
        {
            ArticleAddFrmViewModel viewModel = new ArticleAddFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["ArticleAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object AddArticle(AddArticleRequest insertArticleValues)
        {
            bool result = ArticleController.Insert(insertArticleValues, IdentityController.GetUser(this.CurrentUserName()),out long idImmo);

            if (result && this.Request.Files.FirstOrDefault() != null)
            {
                string nomPhoto;

                foreach (var file in this.Request.Files.ToList())
                {
                    nomPhoto = idImmo.ToString();

                    DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                    var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/articles", nomPhoto + ".jpg");

                    using (var fileStream = new FileStream(filename, FileMode.Create))
                    {
                        file.Value.CopyTo(fileStream);
                    }                    

                }
            }


            string message = result ? $"Article {insertArticleValues.Designation} inséré avec succès !" : $"L'insertion de l'article {insertArticleValues.Designation} échouée !";
            string redirectUrl = $"/article/";
            string messageTitle = "Ajout d'un article";
            int sucess = result ? Log.OK_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyArticleForm(long idArticle)
        {
            Article article = ArticleController.SelectById(idArticle).FirstOrDefault();

            ArticleModifyFrmViewModel viewModelOfArticle = new ArticleModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Article = article
            };

            object view = View["ArticleModifyFrmView", viewModelOfArticle];
            int success = article != null ? 1 : 0;
            string message = viewModelOfArticle.Title;

            return this.ResponseObject(view, article, message, success);
        }

        private object UpdateArticle(ModifyArticleRequest modifyImmoRequest)
        {

            bool result = ArticleController.Update(modifyImmoRequest, IdentityController.GetUser(this.CurrentUserName()));

            if (result)
            {

                if (this.Request.Files.FirstOrDefault() != null)
                {
                    int i = 0;
                    string nomPhoto = modifyImmoRequest.Id + "";

                    foreach (var file in this.Request.Files.ToList())
                    {
                        DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                        var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/articles", nomPhoto + ".jpg");

                        using (var fileStream = new FileStream(filename, FileMode.Create))
                        {
                            file.Value.CopyTo(fileStream);
                        }                        

                        i++;
                    }
                }

            }

            string redirectUrl = $"/article/id/{modifyImmoRequest.Id}";
            string messageTitle = "Modification d'un article";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result ? $"{modifyImmoRequest.Designation} modifié avec succès !" : $"La modification de l'article {modifyImmoRequest.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object DeleteArticle(long idArticle)
        {
            Article article = ArticleController.SelectById(idArticle).FirstOrDefault();

            string message = "L'article que vous avez renseigné n'existe pas !";
            bool result = false;

            if (article != null)
            {
                result = ArticleController.Delete(article.Id, IdentityController.GetUser(this.CurrentUserName()));
                message = result ? $"{article.Designation} supprimé avec succès !" : $"La supression de l'article {article.Designation} a échouée !";
            }

            string redirectUrl = $"/article/";
            int sucess = result ? 1: 0;
            string messageTitle = "Supression d'un article";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}