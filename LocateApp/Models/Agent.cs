using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LocateApp.Utilities;
using LocateApp.Controllers;
using System.Configuration;

namespace LocateApp.Models
{
    public class Agent 
    {
        public Guid SerialId { get; set; }
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        public int Sexe { get; set; }
        public DateTime DateNaissance { set; get; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string CodeOrgane { get; set; }
        public string Genre => (Sexe == 1) ? "Masculin" : "Féminin";
        public bool IsPhotographed => GetStatusOfPhotographing();
        public string Photo => GetPhoto();
        public CategorieAgent Categorie { get; set; }


        private bool GetStatusOfPhotographing(string matricule = null)
        {
            string nip = string.IsNullOrEmpty(matricule) ? Matricule.Trim() : matricule.Trim();
            matricule = nip;

            //bool IsLocal = bool.Parse(ConfigurationManager.AppSettings["Local"]);

            //return File.Exists(HttpContext.Current.Server.MapPath(photoPath + matricule + ".jpg"));

            string photoPath = Utilities.Utilities.GetPhotoAgentPath();
            string photo = photoPath + matricule + ".jpg";
                        
            //return IsLocal ? File.Exists(photo):  Utilities.Utilities.URLExists(photo);
            return  Utilities.Utilities.URLExists(photo);
        }

        public string GetPhoto()
        {
            string photoParDefaut = Sexe == 1 ? "photoVideHomme.png" : "photoVideFemme.png";
            return IsPhotographed ? Matricule.Trim() + ".jpg" : photoParDefaut;
        }
    
    }
}