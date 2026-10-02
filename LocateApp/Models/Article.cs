using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Article
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Designation { get; set; }
        public string IdPosteBudget { get; set; }
        public string UniteMesure { get; set; }
        public decimal CoutUnitaire { get; set; }
        public long? IdCategorie { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public long? IdFournisseur { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public long NbreImmo { get; set; }
        public bool IsPhotographed => GetStatusOfPhotographing();
        public string Photo => GetPhoto();
        public string DesignationToShow => Designation + " " + Marque + " " + Modele;

        public Categorie GetCategorie()
        {
            return IdCategorie == null ? null : CategorieController.SelectById((long)IdCategorie).FirstOrDefault();
        }

        public Fournisseur GetFournisseur()
        {
            return IdFournisseur == null ? null : FournisseurController.SelectById((long)IdFournisseur).FirstOrDefault();
        }

        private bool GetStatusOfPhotographing()
        {
            string photoPath = "~/Content/images/articles/";
            return File.Exists(HttpContext.Current.Server.MapPath(photoPath + Id + ".jpg"));
        }

        private string GetPhoto()
        {
            return IsPhotographed ? Id + ".jpg" : "defaultarticle.jpg";
        }

        public long GetQuantiteImmo()
        {
            return ImmoController.SelectQuantiteByArticle(Id);
        }

        public long GetQuantiteImmoInventorie(long? idLocal = null)
        {
            return idLocal == null ? InventaireController.SelectQuantiteByArticle(Id) : InventaireController.SelectQuantiteByArticleAndLocal(Id, (long)idLocal);
        }

    }

}