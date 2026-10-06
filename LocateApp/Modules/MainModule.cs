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

    }
}