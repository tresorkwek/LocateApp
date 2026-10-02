using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.ViewModels;
using Nancy;
using LocateApp.Controllers;
using Nancy.Extensions;
using Nancy.Authentication.Forms;
using LocateApp.Utilities;
using LocateApp.Models;
using System.Configuration;

namespace LocateApp.Modules
{
    public class LoginModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(LoginModule));

        private static readonly String AppName = ConfigurationManager.AppSettings["Logiciel"];

        public LoginModule() : base("/auth")
        {
            Get("/login", _ => View["AuthLogInView", new LoginViewModel()]);
            Get("/logout", _ => this.LogOut());
            Post("/login", _ => this.RunHandler<LoginSendRequest>(Connexion));
        }
           
        private object Connexion(LoginSendRequest loginValues)
        {

            Identity identity = IdentityController.GetUser(loginValues.UserName);
            string message;
            int success = Log.OK_CODE;
            

            if (identity != null && identity.IsLocked)
            {
                message = "Votre compte est vérouillé";

                LoginViewModel loginViewModel = new LoginViewModel()
                {
                    Alerte = message,
                    AlerteTitle = "Login",
                    TypeTitle = Log.WARNING_CODE,
                    Identity = identity
                };

                Log.Warning(identity, message);

                var view = View["AuthLockScreenView", loginViewModel];

                return this.ResponseObject(view, identity, loginViewModel.Alerte, loginViewModel.TypeTitle);
            }


            if (identity != null && identity.IsExternalUser && !string.IsNullOrEmpty(loginValues.ConfirmPassword))
            {
                bool samePassword = loginValues.Password == loginValues.ConfirmPassword;
                message = null;

                try
                {
                    identity = IdentityController.ValidateApplicationUser(loginValues.UserName, loginValues.ExPassword);
                }
                catch (Exception e)
                {
                    message = e.Message;
                }


                if (!string.IsNullOrEmpty(message) || identity == null || !samePassword)
                {
                    message = !string.IsNullOrEmpty(message) ? message : identity == null ? "L'ancien mot de passe est incorrecte !" : "les 2 nouveaux mots de passe sont différents";

                    LoginViewModel loginViewModel = new LoginViewModel()
                    {
                        Alerte = message,
                        TypeTitle = Log.ERROR_CODE,
                        Identity = IdentityController.GetUser(loginValues.UserName)
                    };

                    Log.Error(identity, message);

                    var view = View["AuthConfirmPwView", loginViewModel];                    

                    return this.ResponseObject(view,IdentityController.GetUser(loginValues.UserName), loginViewModel.Alerte, loginViewModel.TypeTitle);

                }

                bool result = IdentityController.ChangePassword(identity.IdUser, loginValues.ConfirmPassword, identity);

                if (result)
                {
                    message = "Mot de passe modifié avec succès !" ;
                    success = Log.SUCCESS_CODE;

                    Log.Info(identity, message);
                }
                else
                {
                    message = "Modification du mot de passe échoué !";
                    success = Log.ERROR_CODE;

                    Log.Error(identity, message);
                }


                identity = IdentityController.GetIdentity(identity.IdUser).FirstOrDefault();
            }
            else
            {
                try
                {
                    identity = IdentityController.ValidateApplicationUser(loginValues.UserName, loginValues.Password);
                }
                catch (Exception e)
                {
                    LoginViewModel loginViewModel = new LoginViewModel()
                    {
                        Alerte = e.Message,
                        TypeTitle = Log.ERROR_CODE
                    };

                    Log.Error(identity, e.Message);

                    var view = View["AuthLogInView", loginViewModel];

                    identity = null;

                    return this.ResponseObject(view, identity, loginViewModel.Alerte, loginViewModel.TypeTitle);
                }

            }

            //TODO vérifier s'il y a l'adresse Mac en cas de connexion via mobile

            DateTime ? expiry = null;
            if (loginValues.RememberMe == 1)
            {
                expiry = DateTime.Now.AddDays(7);
            }

            if (identity.IsExternalUser && identity.FirstConnexion)
            {
                message = $"L'utilisateur {identity.UserName} tente de créer un mot de passe";
                Log.Info(identity, message);

                message = "Créer votre mot de passe";

                LoginViewModel loginViewModel = new LoginViewModel()
                {
                    Alerte = message,
                    TypeTitle = Log.OK_CODE,
                    Icone = "vpn_key",
                    Identity = identity
                };

                var view = View["AuthConfirmPwView", loginViewModel];
                success = Log.SUCCESS_CODE;

                return this.ResponseObject(view, identity, loginViewModel.Alerte, success);

            }

            message = "Bienvenue à "+ AppName;
            success = Log.INFO_CODE;

            return this.LogIn(identity, expiry, message, success);
        }

    }
}