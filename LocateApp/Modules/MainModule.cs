using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using LocateApp.ViewModels;
using Nancy.Security;
using LocateApp.Utilities;
using LocateApp.Models;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;

namespace LocateApp.Modules
{
    public class MainModule:NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(MainModule));
        public MainModule() : base("/")
        {
            this.RequiresAuthentication();

            Get("", _ => this.RunHandler(ShowDashboard));
            Get("/profile/", _ => this.RunHandler(GetProfile));
            Get("/profile/modify/{matricule}", _ => this.RunHandler<string>(GetModificationProfile, (string)_.matricule));
        }

        private object ShowDashboard()
        {                     

            Identity identity = IdentityController.GetUser(this.CurrentUserName());
            DashboardViewModel dashboard = new DashboardViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };            
            
            Log.Info(identity, dashboard.Title);

            return View["DashboardView", dashboard];
        }

        private object GetProfile()
        {
            //List<Agent> famillyAgent = AgentController.SelectAgent(this.CurrentUserName());
            //PatientFamilleViewModel viewModelOfPatient = new PatientFamilleViewModel(famillyAgent, this.CurrentUserName(), this.ShowAlert())
            //{
            //    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
            //    CurrentClaim = this.GetClaimString(),
            //    Link = "/billetenvoifamilial/create/",
            //    ModifyLink = "/profile/modify/",
            //    AccordLink = "/profile/accord/",
            //    IsMyProfil = true
            //};

            //object view = View["PatientFamilleView", viewModelOfPatient];
            //int success = famillyAgent.Count > 0 ? 1 : 0;

            //return this.ResponseObject(view, famillyAgent, viewModelOfPatient.Title, success);


            Identity user = IdentityController.GetUserIdentity(this.CurrentUserName());

            IdentityModifyFrmViewModel viewModelOfUser = new IdentityModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Identity = user
            };

            object view = View["ProfileView", viewModelOfUser];
            int success = user == null ? 0 : 1;

            return this.ResponseObject(view, user, viewModelOfUser.Title, success);
        }

        private object GetModificationProfile(string matricule)
        {

            if (!Utilities.ForString.IsNip(matricule)) return HttpStatusCode.BadRequest;
            if (matricule.Substring(0, 6) != this.CurrentUserName()) return HttpStatusCode.Forbidden;

            List<Agent> listOfPatient = AgentController.SelectAgent(matricule);

            if (listOfPatient.Count == 0) return HttpStatusCode.Forbidden;

            Agent patient = listOfPatient.FirstOrDefault();

           // if (patient.Statut[0] == "1")
            //{
           //     string message = "Vous ne pouvez pas modifier les informations de ce patient";
           //     string redirectUrl = $"/profile/";

           //     return this.RedirectUrl(redirectUrl, message, 0);
           // }

            PatientModifyInfoViewModel viewModelOfPatientForm = new PatientModifyInfoViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                Patient = patient,
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Link = "/profile/modify/"
            };


            object view = View["PatientModifyInfoFormView", viewModelOfPatientForm];
            int success = listOfPatient.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfPatient, viewModelOfPatientForm.Title, success);
        }

    }
}