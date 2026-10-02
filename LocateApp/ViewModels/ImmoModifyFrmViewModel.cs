using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.Configuration;

namespace LocateApp.ViewModels
{
    public class ImmoModifyFrmViewModel : ViewModel
    {
        public List<Immo> ImmoAutonome { get; set; }
        public Immo Immo { get; set; }
        public string Link { get; set; }
        public bool IsModification { get; set; }
        public List<Article> Articles { get; set; }
        public List<Organe> Organes { get; set; }
        public List<Organe> Organigramme { get; set; }
        public List<Agent> Occupants { get; set; }
        public string InputStyle => GetInputStyle();
        public string SelectStyle => GetSelectStyle();
        public string CheckBoxStyle => GetCheckBoxStyle();

        public ImmoModifyFrmViewModel(string userName, MessageAlerte messageAlerte = null): base(userName, messageAlerte)
        {
            Link = "/immo/modify/";
            Articles = ArticleController.Select();
            Organes = OrganeController.Select(Identity.IdInstitution);
            Organigramme = OrganeController.Organigramme(Identity.IdInstitution);
        }

        public List<Local> GetLocaux()
        {
            Local local = Immo.GetLocal();

            return local == null || local?.CodeOrgane == null ? new List<Local>() : LocalController.SelectAllByCodeOrgane(local.CodeOrgane);
        }

        public string GetNomOrgage(string idOrgane)
        {
            return idOrgane == null ? null : OrganeController.Select(idOrgane).FirstOrDefault().Nom;
        }

        public List<Observations> GetObservations()
        {
            return ObservationsController.SelectByEtat(Immo.LastEtat);
        }

        private string GetInputStyle()
        {
            return IsModification ? "padding-top:10px;padding-bottom:0px;" : "";
        }

        private string GetSelectStyle()
        {
            return IsModification ? "padding-top:20px;padding-bottom:0px;" : "";
        }

        private string GetCheckBoxStyle()
        {
            return IsModification ? "padding-top:25px;padding-bottom:16px;" : "";
        }

    }
}