using LocateApp.Controllers;
using LocateApp.Models;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Modules
{
    public class ErrorModule : NancyModule
    {
        public ErrorModule() : base("/Error")
        {
            Get("", _ => Error());
            Get("/{code}", _ => Error(_.code));
        }

        private object Error(int code = 520)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel(code, this.CurrentUserName());

            return View["ErrorView", errorViewModel];
        }
    }
}