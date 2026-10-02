using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Cookies;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.Security.Principal;
using Newtonsoft.Json;
using LocateApp.ViewModels;
using Nancy.Session;
using Nancy.Extensions;
using System.Configuration;

namespace LocateApp.Utilities
{
    public static class ModuleExtension
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ModuleExtension));
        private static readonly string AppName = ConfigurationManager.AppSettings["Logiciel"];

        public static object RunHandler<T>(this NancyModule module, Func<T, object> handler)
        {

            T dto;
            try
            {
                dto = module.BindAndValidate<T>();
            }
            catch (ModelBindingException e)
            {
                Log.Error(IdentityController.GetUser(module.CurrentUserName()), e.Message);
                return HttpStatusCode.BadRequest;
            }

            if (module.Context.CurrentUser != null)
            {
                // if (!module.CheckClaims()) return module.Response.AsRedirect("/");
                if (!module.CheckClaims()) return HttpStatusCode.Forbidden;
            }

            return handler(dto);

        }

        public static object RunHandlerWithAuthentication<T>(this NancyModule module, Func<T, object> handler)
        {
            if (module.Context.CurrentUser == null)
                // return module.Response.AsRedirect("~/auth/login");
                return HttpStatusCode.Unauthorized;

            return module.RunHandler<T>(handler);
        }

        public static object RunHandler(this NancyModule module, Func<object> handler)
        {

            if (module.Context.CurrentUser != null)
            {

                if (!module.CheckClaims())
                {
                    string message = $"L'utilisateur {module.CurrentUserName()} ne dispose pas du droit {module.GetClaimString()}";
                    Log.Error(IdentityController.GetUser(module.CurrentUserName()), message);

                    if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html") && !module.Context.Request.IsAjaxRequest())
                    {

                        string redirectUrl = IdentityController.GetUser(module.CurrentUserName()).Profil.HomeUrl;
                        message = ConfigurationManager.AppSettings[$"{(int)HttpStatusCode.Forbidden}"];
                        string messageTitle = $"Privilège de {module.Context.CurrentUser.Identity.Name}";
                        int sucess = 0;

                        return module.RedirectUrl(redirectUrl, message, sucess, messageTitle);

                    }
                    else
                    {
                        return HttpStatusCode.Forbidden;
                    }
                }

            }

            return handler();

        }

        public static object RunHandlerWithAuthentication(this NancyModule module, Func<object> handler)
        {
            if (module.Context.CurrentUser == null)
                // return module.Response.AsRedirect("~/auth/login");
                return HttpStatusCode.Unauthorized;

            return module.RunHandler(handler);
        }

        #region Avec Arguments
        public static object RunHandler<T>(this NancyModule module, Func<T, object> handler, dynamic arg)
        {

            if (module.Context.CurrentUser != null)
            {
                if (!module.CheckClaims())
                {
                    string message = $"L'utilisateur {module.CurrentUserName()} ne dispose pas du droit {module.GetClaimString()}";
                    Log.Error(IdentityController.GetUser(module.CurrentUserName()), message);

                    return HttpStatusCode.Forbidden;
                }

            }

            return handler(arg);

        }

        public static object RunHandlerWithAuthentication<T>(this NancyModule module, Func<T, object> handler, dynamic arg)
        {
            if (module.Context.CurrentUser == null)
                // return module.Response.AsRedirect("~/auth/login");
                return HttpStatusCode.Unauthorized;

            return module.RunHandler<T>(handler, (T)arg);
        }

        public static object RunHandler<T, U>(this NancyModule module, Func<T, U, object> handler, dynamic firstArg, dynamic secondArg)
        {

            if (module.Context.CurrentUser != null)
            {
                // if (!module.CheckClaims()) return module.Response.AsRedirect("/");
                if (!module.CheckClaims()) return HttpStatusCode.Forbidden;
            }

            return handler(firstArg, secondArg);

        }

        public static object RunHandler(this NancyModule module, Func<dynamic[], object> handler, params dynamic[] args)
        {

            if (module.Context.CurrentUser != null)
            {
                // if (!module.CheckClaims()) return module.Response.AsRedirect("/");
                if (!module.CheckClaims()) return HttpStatusCode.Forbidden;
            }

            return handler(args);

        }

        public static object RunHandlerWithAuthentication<T, U>(this NancyModule module, Func<T, U, object> handler, dynamic firstArg, dynamic secondArg)
        {
            if (module.Context.CurrentUser == null)
                // return module.Response.AsRedirect("~/auth/login");
                return HttpStatusCode.Unauthorized;

            return module.RunHandler<T,U>(handler, (T)firstArg, (U)secondArg);
        }

        #endregion

        public static bool CheckClaim(this NancyModule module)
        {
            string claimString = module.GetClaimString();
            string rootsString = "Root";
            

            List<string> claims = new List<string> { claimString, rootsString };
            Predicate<Claim> requiredClaims = x => claims.Contains(x.Value);

            return module.Context.CurrentUser.HasClaim(requiredClaims);
        }

        public static bool CheckClaims(this NancyModule module)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html") || module.Context.Request.IsAjaxRequest())
                return module.CheckClaim();

            return module.CheckClaimJWT();
        }

        public static bool CheckClaimJWT(this NancyModule module)
        {

            string claimString = module.GetClaimString();
            string rootsString = "Root";

            List<string> claims = new List<string> { claimString, rootsString };

            var requestHeaders = module.Request.Headers;
            if (requestHeaders == null)
                return false;

            if (String.IsNullOrEmpty(requestHeaders.Authorization.ToString()))
                return false;
            if (String.IsNullOrWhiteSpace(requestHeaders.Authorization.ToString()))
                return false;

            var stream = requestHeaders.Authorization.ToString().Substring(7).Trim();

            if (String.IsNullOrEmpty(stream))
                return false;
            if (String.IsNullOrWhiteSpace(stream))
                return false;

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(stream);
            var token = jsonToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

            var userName = token.Claims.First(claim => claim.Type == "sub").Value;

            if (String.IsNullOrEmpty(userName))
                return false;
            if (String.IsNullOrWhiteSpace(userName))
                return false;

            var userRoleClaimsList = token.Claims.Where(claim => claim.Type == "Role").Select(claim => claim.Value).ToList();
            if (userRoleClaimsList == null)
                return false;
            if (userRoleClaimsList.Count < 1)
                return false;

            var userMatchingRoleClaims = userRoleClaimsList.Where(c => claims.Contains(c)).ToList();

            if (userMatchingRoleClaims == null)
                return false;

            if (userMatchingRoleClaims.Count == 0)
                return false;

            return true;
        }

        public static string GetClaimString(this NancyModule module)
        {
            string methode = module.Context.Request.Method.First().ToString().ToUpper() + module.Context.Request.Method.Substring(1).ToLower();
            string url = module.Context.Request.Url.Path;

            DynamicDictionary parametres = (DynamicDictionary)module.Context.Parameters;


            List<string> urlList = url.Split('/').ToList();
            List<dynamic> paramValues = parametres.Values.ToList().Select(item => item.ToString()).ToList();
            List<string> paramKeys = parametres.Keys.ToList();

            //string claimString = methode + string.Join("", urlList.Where(item => !string.IsNullOrEmpty(item)).Select(u => paramValues.Contains(u) ? paramKeys[paramValues.IndexOf(u)].First().ToString().ToUpper() + paramKeys[paramValues.IndexOf(u)].Substring(1).ToLower() : u.First().ToString().ToUpper() + u.Substring(1).ToLower()).ToList());

            string claimString = methode 
                    + string.Join("", urlList.Where(item => !string.IsNullOrEmpty(item))
                    .Select(u => paramValues.Contains(u) 
                    ? (paramKeys[paramValues.IndexOf(u)].Substring(paramKeys[paramValues.IndexOf(u)].Length - 1) == "_" 
                    ? u.ToString() 
                    : paramKeys[paramValues.IndexOf(u)].First().ToString().ToUpper() + paramKeys[paramValues.IndexOf(u)].Substring(1).ToLower()) 
                    : u.First().ToString().ToUpper() + u.Substring(1).ToLower()).ToList());

            return claimString;
        }

        public static object ResponseObject(this NancyModule module, object webView, object jsonView)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html") && !module.Context.Request.IsAjaxRequest())
            {
                return webView;
            }

            return jsonView;
        }

        public static object ResponseObject(this NancyModule module, object webView, object jsonView, string message, long success)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html") && !module.Context.Request.IsAjaxRequest())
            {
                return webView;
            }

            return module.Response.AsJson(new
            {
                Success = success,
                message = message,
                Content = jsonView
            });

        }

        public static object LogIn(this NancyModule module, Identity identity, DateTime? expiry = null, string message = null, int typeMessage = 4)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html"))
            {
                MessageAlerte messageAlerte = new MessageAlerte()
                {
                    Alerte = message,
                    TypeTitle = typeMessage,
                    AlerteTitle = null
                };

                // module.Session["Alerte"] = messageAlerte;

                if (module.Context.Request.IsAjaxRequest())
                {

                    _ = module.LoginWithoutRedirect(identity.IdUser, expiry);

                    return module.Response.AsJson
                    (
                        new
                        {
                            Success = typeMessage != 0 ? 1 : 0,
                            Message = message,
                            Content = identity
                        }
                    );

                }
                else
                {
                    string homeUrl = identity.Profil.HomeUrl;
                    return module.LoginAndRedirect(identity.IdUser, expiry, homeUrl);
                }
                
            }

            var jwt = JwtBearerManager.GetUserJwt(identity);
            var tokenObject = JsonConvert.DeserializeObject<Token>(jwt);

            return module.Response.AsJson
                (
                    new
                    {
                        Success = typeMessage != 0 ? 1 : 0,
                        Message = message,
                        Token = tokenObject,
                        Content = identity
                    }
                );

        }

        public static object LogOut(this NancyModule module)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html"))
            {
                if (module.Context.Request.IsAjaxRequest())
                {
                    _ = module.LogoutWithoutRedirect();
                }
                else
                {
                    return module.LogoutAndRedirect("~/auth/login");
                }
                
                    
            }

            //var requestJwtCookie = new NancyCookie(module.Context.Request.Cookies.SingleOrDefault(c => c.Key.ToString() == "jwt").Key, module.Context.Request.Cookies.SingleOrDefault(c => c.Key.ToString() == "jwt").Value, true);
            //module.Context.Response.Cookies.Remove(requestJwtCookie);

            return module.Response.AsJson
                (
                    new
                    {
                        Success = 1,
                        Message = "Merci d'avoir utiliser "+AppName,
                        Content = ""
                    }
                );

        }

        public static object RedirectUrl(this NancyModule module, string redirectUrl, string message, long success = 1, string messageTitle = null)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html") && !module.Context.Request.IsAjaxRequest())
            {
                if (!string.IsNullOrEmpty(message))
                {
                    MessageAlerte messageAlerte = new MessageAlerte()
                    {
                        Alerte = message,
                        TypeTitle = success,
                        AlerteTitle = messageTitle
                    };

                    module.Session["Alerte"] = messageAlerte;
                }
                            
                return module.Response.AsRedirect(redirectUrl);
            }
        
            return module.Response.AsJson
                (
                    new
                    {
                        Success = success,
                        Message = message,
                        Content = ""
                    }
                );

        }

        public static MessageAlerte ShowAlert(this NancyModule module)
        {
            MessageAlerte messageAlerteSession = (MessageAlerte)module.Session["Alerte"];
            MessageAlerte messageAlerte = null;

            if (messageAlerteSession != null)
            {
                messageAlerte = new MessageAlerte()
                {
                    Alerte = messageAlerteSession.Alerte,
                    AlerteTitle = messageAlerteSession.AlerteTitle,
                    TypeTitle = messageAlerteSession.TypeTitle
                };
            }            

            module.Session["Alerte"] = null;

            return messageAlerte;
        }

        public static string CurrentUserName(this NancyModule module)
        {
            if (module.Context.Request.Headers.Accept.ToList().Select(e => e.Item1).Contains("text/html"))
            {
               return module.Context.CurrentUser?.Identity?.Name;
            }

            var requestHeaders = module.Request.Headers;
            if (requestHeaders == null)
                return null;

            if (String.IsNullOrEmpty(requestHeaders.Authorization.ToString()))
                return null;
            if (String.IsNullOrWhiteSpace(requestHeaders.Authorization.ToString()))
                return null;

            var stream = requestHeaders.Authorization.ToString().Substring(7).Trim();

            if (String.IsNullOrEmpty(stream))
                return null;
            if (String.IsNullOrWhiteSpace(stream))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(stream);
            var token = jsonToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

            var userName = token.Claims.First(claim => claim.Type == "sub").Value;

            if (String.IsNullOrEmpty(userName))
                return null;
            if (String.IsNullOrWhiteSpace(userName))
                return null;
                 
            return userName;
        }
    }
}