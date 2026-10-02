using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class MenuToShow
    {
        public int IdMenuGroup { get; set; }
        public string NomGroup { get; set; }
        public string LibelleGroup { get; set; }
        public string  Icone  { get; set; }
        public int OrdreGroup { get; set; }
        public bool VisibleGroup { get; set; }
        public int IdSection { get; set; }
        public string Section { get; set; }
        public int IdMenu { get; set; }
        public string NomMenu { get; set; }
        public string MenuLibelle { get; set; }
        public string Commentaire { get; set; }
        public string Titre { get; set; }
        public string Url { get; set; }
        public int OrdreMenu { get; set; }
        public bool VisibleMenu { get; set; }
        public int IdModule { get; set; }
        public string NomModule { get; set; }
        public int IdAction { get; set; }
        public string NomAction { get; set; }
        public int IdProfil { get; set; }
        public string NomProfil { get; set; }
        public string TableName { get; set; }
        public string Champ { get; set; }
        public string Valeur { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AddMenuRequest
    {
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public string LibelleGroup { get; set; }
        public string Commentaire { get; set; }
        public string Titre { get; set; }
        public string Url { get; set; }
        public int IdGroupMenu { get; set; }
        public int IdModule { get; set; }
        public int IdAction { get; set; }
        public int Ordre { get; set; }
        public bool Visible { get; set; }
        public bool DefaultMenu { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyMenuRequest
    {
        public int IdMenu { get; set; }
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public string Commentaire { get; set; }
        public string Titre { get; set; }
        public string Url { get; set; }
        public int IdGroupMenu { get; set; }
        public long IdModule { get; set; }
        public int IdAction { get; set; }
        public int Ordre { get; set; }
        public bool Visible { get; set; }
        public bool DefaultMenu { get; set; }
    }
}