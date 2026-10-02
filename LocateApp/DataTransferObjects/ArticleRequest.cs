using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.IO;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AddArticleRequest
    {
        public string Code { get; set; }
        public string Designation { get; set; }
        public string IdPosteBudget { get; set; }
        public string UniteMesure { get; set; }
        public decimal CoutUnitaire { get; set; }
        public long IdCategorie { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public long? IdFournisseur { get; set; }
        public string UserCreation { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GetArticleRequest : Article
    {
        public Fournisseur Fournisseur => GetFournisseur();
        public Categorie Categorie => GetCategorie();

    }

    [ExcludeFromCodeCoverage]
    public class ModifyArticleRequest
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Designation { get; set; }
        public string IdPosteBudget { get; set; }
        public string UniteMesure { get; set; }
        public decimal CoutUnitaire { get; set; }
        public long IdCategorie { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public long? IdFournisseur { get; set; }
        public bool IsActive { get; set; }
        public string UserCreation { get; set; }
    }

}