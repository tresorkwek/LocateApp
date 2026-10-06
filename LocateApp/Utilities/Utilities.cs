using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using LocateApp.Models;
using iTextSharp.text.pdf.parser;
using Nancy;

namespace LocateApp.Utilities
{
    public static class Utilities
    {
        public static bool CheckClaimStatus(Identity identity, string claimString)
        {
            // Sans identité (requête sans session, ex. jeton d'API) : aucun droit, plutôt qu'une erreur au rendu de la page
            return identity?.Claims != null && identity.Claims.Any(c => c.Equals(claimString) || c.Equals("Root"));
        }

        public static string GenerateClaimName(string claim, Identity identity)
        {
            claim = claim ?? string.Empty;

            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(Identity).GetProperties();
            propertyInfos.ToList().ForEach(myProperty =>
            {

                if (claim.Contains("{" + myProperty.Name + "_}"))
                {
                    string myClaim = claim;
                    string value = myProperty.GetValue(identity) == null ? string.Empty : myProperty.GetValue(identity).ToString();
                    claim = myClaim.Replace("{" + myProperty.Name + "_}", value);
                }
            });

            return claim;
        }

        public static void ToTextWriter(IEnumerable<dynamic> results, TextWriter tw)
        {
            var csv = new CsvHelper.CsvWriter(tw, CultureInfo.InvariantCulture);
            csv.WriteRecords(results);

            csv.Flush();
        }

        public static string FormatNumber(decimal number, int NbreDecimal = 2)
        {
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

            nfi.NumberGroupSeparator = " ";
            nfi.NumberDecimalSeparator = ",";
            nfi.NumberDecimalDigits = NbreDecimal;

            return number.ToString("n",nfi);
        }

        public static string GetObjectProperties<T>(T obj)
        {
            // Appelé depuis les blocs catch de SqlDataAccess : ne doit jamais lever d'exception,
            // sinon l'erreur SQL d'origine n'est jamais journalisée (paramètres null = requête sans paramètre).
            if (obj == null)
            {
                return "NULL";
            }

            try
            {
                var type = obj.GetType();
                string result = "{ ";

                var propreties = type.GetProperties();

                foreach (var prop in propreties)
                {
                    if (prop.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    object valeur;
                    try { valeur = prop.GetValue(obj); } catch (Exception e) { valeur = $"<{e.GetType().Name}>"; }
                    result += string.Format("\n{0}: {1}; ", prop.Name, valeur ?? "NULL");
                }

                if (result.Length > 2)
                {
                    result = result.Substring(0, result.Length - 2);
                }
                result += "\n}";

                return result;
            }
            catch (Exception e)
            {
                return $"<{e.GetType().Name} lors de la lecture des propriétés>";
            }
        }

        public static string GetPhotoAgentPath()
        {
            // Chemin absolu depuis la racine de l'application (et non depuis l'URL de la page courante,
            // sinon /utilisateur/ donnait /utilisateur/Content/images/photos/ -> 404).
            string local = VirtualPathUtility.ToAbsolute("~/Content/images/photos/");
            //string local = HttpContext.Current.Server.MapPath("~/Content/images/photos/");
            // Photos hébergées ailleurs (clé PhotoAgentUrl, ex. serveur RH) quand Local = false ; sinon dossier de l'application.
            string distant = ConfigurationManager.AppSettings["PhotoAgentUrl"];

            bool.TryParse(ConfigurationManager.AppSettings["Local"], out bool IsLocal);

            return IsLocal || string.IsNullOrWhiteSpace(distant) ? local : distant;
        }

        public static bool URLExists(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            // Chemin interne à l'application (ex. /Content/images/photos/70000100.jpg) :
            // on vérifie directement le fichier sur le disque, sans requête HTTP vers soi-même.
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.IsFile)
            {
                try
                {
                    return File.Exists(HttpContext.Current.Server.MapPath(url));
                }
                catch
                {
                    return false;
                }
            }

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "HEAD";
            bool result;

            try
            {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                result = response.StatusCode == System.Net.HttpStatusCode.OK;

            }
            catch
            {
                result = false;
            }

            return result;
        }

        //public static bool URLExists(string url)
        //{
        //    Uri destURL = new Uri(url);
        //    System.Net.WebRequest webRequest = System.Net.WebRequest.CreateHttp(destURL);
        //    webRequest.Method = "HEAD";
        //    bool result;

        //    try
        //    {
        //        System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)webRequest.GetResponse();

        //        result = response.StatusCode == System.Net.HttpStatusCode.OK;

        //    }
        //    catch
        //    {
        //        result = false;
        //    }

        //    return result;
        //}

    }
}