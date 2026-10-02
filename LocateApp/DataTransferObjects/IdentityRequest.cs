using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.DataTransferObjects
{

    [ExcludeFromCodeCoverage]
    public class GetIdentityRequest
    {
        public Guid IdUser { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ImportIdentityRequest
    {
        public string Matricule { get; set; }
        public string IdProfil { get; set; }
    }


    [ExcludeFromCodeCoverage]
    public class IdentityInsertRequest
    {
        public Guid? IdUser { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        public string Sexe { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int IdProfil { get; set; }
        public bool IsExternalUser { get; set; }
        public bool FirstConnexion { get; set; }
        public string IdInstitution { get; set; }
        public string CodeOrgane { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class IdentityUpdateRequest
    {
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
        public bool FocalPoint { get; set; }
        public string IdInstitution { get; set; }
        public string CodeOrgane { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GetMoviaIdentityRequest
    {
        public Guid IdUser { get; set; }
        public string UserName { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        public int Sexe { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int IdProfil { get; set; }
        public bool IsHospitalUser { get; set; }
        public bool IsLocked { get; set; }
        public int NbreTentatives { get; set; }
        public bool FirstConnexion { get; set; }
        public string IdInstitution { get; set; }
        public bool FocalPoint { get; set; }
        public string NumOrdMed { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime LastConnexion { get; set; }
        public string Genre { get; set; }
        public MessageAlerte MessageAlerte { get; set; }
        public Profil Profil { get; set; }
        public Institution Institution { get; set; }
        public List<string> Claims { get; set; }
        public List<GroupMenu> GroupMenu { get; set; }
        public string Photo { get; set; }
    }

}