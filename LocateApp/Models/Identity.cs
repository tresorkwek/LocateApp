using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using Newtonsoft.Json;
using System.Reflection;

namespace LocateApp.Models
{
    public class Identity
    {
        public Guid IdUser { get; set; }
        public string UserName { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        public string Sexe { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int IdProfil { get; set; }
        public bool IsExternalUser { get; set; }
        public bool IsLocked { get; set; }
        public int NbreTentatives { get; set; }
        public bool FirstConnexion { get; set; }
        public string IdInstitution { get; set; }
        public string CodeOrgane { get; set; }
        public bool FocalPoint { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime LastConnexion { get; set; }
        public string Genre => (Sexe == "M") ? "Masculin" : "Féminin";
        public MessageAlerte MessageAlerte { get; set; }
        public Profil Profil => GetProfil();
        public Organe Organe => OrganeController.SelectById(CodeOrgane);
        public Institution Institution => GetInstitution();
        public List<string> Claims => GetClaims();
        public List<GroupMenu> GroupMenu => GetGroupMenu();
        public string Photo => GetPhoto();
        public int AnneeInventaireEnCours => InventaireController.SelectAnneeEnCours();

        public string GetPassword()
        {
            return IdentityController.GetPassword(IdUser);
        }

        public bool HasClaim(int idMenu)
        {
            return Profil.Root || Claims.Contains(Utilities.Utilities.GenerateClaimName(MenuController.GetMenu(idMenu).FirstOrDefault()?.Nom, this));
        }

        private Profil GetProfil()
        {
            Profil profil = ProfilController.GetProfil(IdProfil).FirstOrDefault();

            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(Identity).GetProperties();
            propertyInfos.ToList().ForEach(myProperty => {

                if (profil.HomeUrl.Contains("{" + myProperty.Name + "_}"))
                {
                    string value = myProperty.GetValue(this) == null ? string.Empty : myProperty.GetValue(this).ToString();
                    profil.HomeUrl = profil.HomeUrl.Replace("{" + myProperty.Name + "_}", value);
                }

            });

            return profil;
        }

        private Institution GetInstitution()
        {
            Institution institution = new Institution()
            {
                Nom = ConfigurationManager.AppSettings["Company"],
                Sigle = ConfigurationManager.AppSettings["Acronyme"],
                Adresse = ConfigurationManager.AppSettings["CompanyAdress"]
            };

            if (!string.IsNullOrEmpty(IdInstitution))
            {
                institution = OrganeController.SelectInstitutionById(IdInstitution);
            }

            return institution;
        }

        private List<string> GetClaims()
        {
            List<string> claims = new List<string> { Profil.Nom };

            if (!Profil.Root)
            {
                claims = new List<string>();
                List<string> newClaims = ClaimControllers.GetClaimsForProfil(Profil.IdProfil, IsExternalUser);

                newClaims.ForEach(claim => {
                    //PropertyInfo[] propertyInfos;
                    //propertyInfos = typeof(Identity).GetProperties();
                    //propertyInfos.ToList().ForEach(myProperty => {                        

                    //    if (claim.Contains("{" + myProperty.Name + "_}"))
                    //    {
                    //        string myClaim = claim;
                    //        string value = myProperty.GetValue(this) == null ? string.Empty : myProperty.GetValue(this).ToString();
                    //        claim = myClaim.Replace("{" + myProperty.Name + "_}", value);
                    //    }

                    //});
                    //claims.Add(claim);
                    claims.Add(Utilities.Utilities.GenerateClaimName(claim, this));
                });

            }         

            return claims;
        }

        private bool IsPhotographed()
        {
           // string photoName = !IsExternalUser ? UserName.Trim() + "00" : UserName.Trim();
            string photoName =  UserName.Trim() + "00";

            string photoPath = Utilities.Utilities.GetPhotoAgentPath();

            string photo = photoPath + photoName + ".jpg";
            //return File.Exists(HttpContext.Current.Server.MapPath(photoPath + photoName + ".jpg"));

            //

           // return File.Exists(photo);
            return Utilities.Utilities.URLExists(photo);
        }

        private string GetPhoto()
        {
            string photoParDefaut = Sexe == "M" ? "photoVideHomme.png" : "photoVideFemme.png";
           // string photoName = !IsExternalUser ? UserName.Trim() + "00" : UserName.Trim();
            string photoName = UserName.Trim() + "00" ;

            return IsPhotographed() ? photoName + ".jpg" : photoParDefaut;
        }

        private List<MenuToShow> GetMenuToShows()
        {
            return MenuToShowController.GetMenuToShows(IdProfil);
        }

        private List<GroupMenu> GetGroupMenu()
        {
            List<GroupMenu> groupMenus = new List<GroupMenu>();
            List<GroupMenu> newGroupMenusList = new List<GroupMenu>();

            List<string> claims = ClaimControllers.GetClaimsForProfil(Profil.IdProfil, Profil.Externe);

            groupMenus = Profil.Root ? GroupMenuController.GetGroupMenu() : GroupMenuController.GetGroupMenuByProfil(Profil);

            groupMenus.ForEach(groupMenu =>
            {
                GroupMenu gMenu = groupMenu;
                gMenu.Menu.Clear();

                groupMenu.AllMenu.ForEach(menu =>
                {
                    if (menu.Visible && (claims.Any(c => c.Equals(menu.Nom)) || Profil.Root))
                    {
                        Menu myMenu = menu;
                        PropertyInfo[] propertyInfos;
                        propertyInfos = typeof(Identity).GetProperties();
                        propertyInfos.ToList().ForEach(myProperty => {

                            if (menu.Nom.Contains("{" + myProperty.Name + "_}"))
                            {
                                string oldMenuNom = menu.Nom;
                                string value = myProperty.GetValue(this) == null ? string.Empty : myProperty.GetValue(this).ToString();
                                string routeToReplace = myProperty.GetValue(this) == null ? "{" + myProperty.Name + "_}/" : "{" + myProperty.Name + "_}";

                                myMenu.Nom = menu.Nom.Replace("{" + myProperty.Name + "_}", value);
                                myMenu.Url = menu.Url.Replace(routeToReplace, value);
                            }

                        });
                        gMenu.Menu.Add(myMenu);
                    }
                        
                });
                newGroupMenusList.Add(gMenu);
            });

            return newGroupMenusList;
        }

    }
}