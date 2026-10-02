using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LocateApp.Utilities;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Immo
    {
        public long Id { get; set; }
        public string CodeADM { get; set; }
        public string CodeImmo { get; set; }
        public string Designation => GetNameImmo();
        public long? IdArticle { get; set; }
        public long? IdLocal { get; set; }
        public DateTime DateCreation { set; get; }
        public string UserCreation { get; set; }
        public DateTime DateMisEnService { set; get; }
        public string UserMisEnService { get; set; }
        public DateTime DateDeclassement { set; get; }
        public string UserDeclassement { get; set; }
        public Guid? QrCode { get; set; }
        public long? IdEtiquette { get; set; }
        public bool IsActive { get; set; }
        public string LastEtat { get; set; }
        public int IdLastObservation { get; set; }
        public string LastObservation { get; set; }
        public int LastAnneeComptable { get; set; } 
        public string Responsable { get; set; }
        public string Inventorieur { get; set; }
        public DateTime DateInventaire { get; set; }
        public bool ImmoExist { get; set; }
        public string Observation { get; set; }
        public string UserVu { get; set; }
        public DateTime DateVu { get; set; }
        public string UserCession { get; set; }
        public DateTime? DateCession { get; set; }
        public long? IdImmoParent { get; set; }
        public string DesignationImmoParent => GetNameImmoParent();
        public long IdImmoPrincipal { get; set; }
        public long? IdReceptionLigne { get; set; }
        public decimal? PrixAcquisition { get; set; }
        public DateTime? DateAcquisition { get; set; }
        public string DeviseAcquisition { get; set; }
        public decimal? TauxAcquisition { get; set; }
        public string DesignationImmoPrincipal => GetNameImmoPrincipal();
        public string IconLastEtat => GetIcon();
        public string IconColor => GetIconColor();
        public string LastEtatString => GetEtat();
        public List<ImmoPhotos> ImmoPhotos => GetImmoPhotos();
        public string Photo => GetPhoto();
        /// <summary>Inventorié dans l'inventaire en cours : bien identifié (UserVu) ayant sa ligne InventaireDetails de l'année.</summary>
        public bool Inventorier => !string.IsNullOrEmpty(UserVu) && GetInventaire() != null;

        public string GetNameImmo()
        {
            return ImmoController.SelectNameById(Id);
        }
        public string GetNameImmoParent()
        {
            return IdImmoParent == null ? null : ImmoController.SelectNameById((long)IdImmoParent);
        }

        public string GetNameImmoPrincipal()
        {
            return ImmoController.SelectNameById(IdImmoPrincipal);
        }

        public Article GetArticle()
        {
            return IdArticle == null ? null : ArticleController.SelectById((long)IdArticle).FirstOrDefault();
        }

        public Local GetLocal()
        {
            return IdLocal == null ? null : LocalController.SelectById((long)IdLocal).FirstOrDefault();
        }

        private string GetIconColor()
        {
            string color;

            switch (LastEtat)
            {
                case "B":
                    color = "green";//Bon
                    break;
                case "M":
                    color = "orange"; //Mauvais
                    break;
                default:
                    color = "red"; // non vu
                    break;
            }

            return color;
        }

        private string GetIcon()
        {
            string icon ;

            switch (LastEtat)
            {
                case "B":
                    icon = "thumb_up";//Bon
                    break;
                case "M":
                    icon = "thumb_down"; //Mauvais
                    break;
                default:
                    icon = "visibility_off"; // non vu
                    break;
            }

            return icon;
        }
        private string GetEtat()
        {
            string result;

            switch (LastEtat)
            {
                case "M":
                    result = "Mauvais";
                    break;
                case "B":
                    result = "Bon";
                    break;
                default:
                    result = "Non vu";
                    break;
            }

            return result;
        }

        public List<ImmoPhotos> GetImmoPhotos()
        {
            List<ImmoPhotos> immoPhotos = ImmoController.SelectPhotosById(Id);

            List<ImmoPhotos> photos = new List<ImmoPhotos>();
            bool isPhotographed;

            if (immoPhotos.Count > 0)
            {
                string photoPath = "~/Content/images/equipements/";

                foreach (var photo in immoPhotos)
                {
                    isPhotographed = File.Exists(HttpContext.Current.Server.MapPath(photoPath + photo.Id + ".jpg"));

                    if (isPhotographed)
                    {
                        photos.Add(photo);
                    }

                }

            }
            
            return photos;
        }

        public bool IsPhotographed()
        {
            return GetImmoPhotos().Count > 0;
        }

        public string GetPhoto()
        {
            string photoParDefaut = "defaultImmo.jpg"; //TODO mettre la bonne photo
            List<ImmoPhotos> immoPhotos = GetImmoPhotos();

            return immoPhotos.Count > 0 ? immoPhotos[0].Id + ".jpg" : photoParDefaut;
        }             

        public bool IsItCorrect()
        {
            bool response = false;

            if (IdArticle != null && IdLocal != null && QrCode != null && IsPhotographed())
            {
                response = true; // Ce que c'est bien rempli
            }
                           
            return response;
        }

        public InventaireDetails GetInventaire()
        {
            int anneeEnCours = InventaireController.SelectAnneeEnCours();

            return anneeEnCours != 0 && LastAnneeComptable == anneeEnCours ? InventaireController.SelectDetailsByImmo(Id) : null;
        }

    }
}