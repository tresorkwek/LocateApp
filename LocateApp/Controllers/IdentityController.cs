using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.Utilities;
using System.DirectoryServices;

namespace LocateApp.Controllers
{
    public static class IdentityController
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(IdentityController));

        public static List<Identity> GetIdentity(Guid? idUser = null)
        {
            GetIdentityRequest userValue = null;
            string sql = SqlUtilisateur.SelectAll;

            if (idUser != null)
            {
                userValue = new GetIdentityRequest() { IdUser = (Guid)idUser };
                sql = SqlUtilisateur.SelectById;
            }

            return SqlDataAccess.SelectData<Identity>(sql, userValue);
        }

        public static List<Identity> GetDisableIdentity()
        {
            return SqlDataAccess.SelectData<Identity>(SqlUtilisateur.SelectAllDesable);
        }

        public static Identity GetUserIdentity(string userName)
        {
            return SqlDataAccess.SelectData<Identity>(SqlUtilisateur.SelectAllById, new { UserName = userName }).FirstOrDefault();
        }

        public static bool ChangeIdentityStatus(string userName, bool desable, Identity identity)
        {
            bool response = false;
            try
            {
                string sql = desable ? SqlUtilisateur.Desactiver : SqlUtilisateur.Activer;

                int nbreOfRow = SqlDataAccess.SaveData(sql, new { UserName = userName }, identity);

                if (nbreOfRow > 0)
                {
                    response = true;
                    string action = desable ? "Désactivation" : "Activation";

                    string message = $"{action} de l'utilisateur {userName}";
                    Log.Info(identity, message);
                }

            }
            catch (Exception e)
            {
                string action = desable ? "désactivé" : "activé";

                Log.Error(identity, e.Message);
                //throw new Exception($"Oup ! L'utilisateur n'a pas pu être {action}, veillez contacter l'administrateur");
            }

            return response;

        }

        public static bool ImportIdentities(ImportIdentityRequest importIdentityRequest, Identity user, out List<Identity> identities)
        {

            List<(string, IdentityInsertRequest)> sqlAndDataImport = new List<(string, IdentityInsertRequest)>();
            identities = new List<Identity>();

            List<string> listeMatriculeIdProfil = importIdentityRequest.IdProfil.Split(',').ToList();

            foreach (var matriculeIdProfil in listeMatriculeIdProfil)
            {
                string[] values = matriculeIdProfil.Split('_');

                IdentityInsertRequest userValues = new IdentityInsertRequest()
                {
                    UserName = values[0].Substring(0, 6),
                    IdProfil = int.Parse(values[1])
                };

                Agent agent = AgentController.SelectAgent(userValues.UserName).FirstOrDefault();

                string codeOrgageUser;

                //if (!string.IsNullOrEmpty(agent.CodeService))
                //{
                //    codeOrgageUser = agent.CodeService;
                //}
                //else
               // {
                 //   codeOrgageUser = !string.IsNullOrEmpty(agent.CodeSousDirection) ? agent.CodeSousDirection : agent.CodeDirection;
               // }

                string sexe = agent.Sexe == 1 ? "M" : "F";

                userValues.IdUser = agent.SerialId;
                userValues.Password = null;
                userValues.Nom = agent.Nom?.Trim() ;
                userValues.Postnom = agent.Postnom?.Trim();
                userValues.Prenom = agent.Prenom?.Trim();
                userValues.Sexe = sexe;
                userValues.Telephone = agent.Telephone?.Trim();
                userValues.Email = agent.Email?.Trim();
                userValues.IsExternalUser = false;
                userValues.FirstConnexion = false;
                userValues.IdInstitution = null;
                userValues.CodeOrgane = agent.CodeOrgane + "00";

                sqlAndDataImport.Add((SqlUtilisateur.Insert, userValues));

                identities.Add(TransformAgentToUser(agent, userValues.IdProfil));
            }

            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndDataImport, user);

            return nbreOfRow > 0;
        }

        public static bool InsertIdentity(IdentityInsertRequest userValues, Identity identity)
        { 
            bool response = false;

            if (userValues.IsExternalUser)
            {
                userValues.IdUser = Guid.NewGuid();
                userValues.Password = new PasswordHasher().HashPassword(ConfigurationManager.AppSettings["DefaultPassword"]);

                int nbreOfRow = SqlDataAccess.SaveData(SqlUtilisateur.Insert, userValues, identity);

                if (nbreOfRow > 0)
                {
                    response = true;
                    string message = $"Le compte {identity.UserName} a inseré un nouvel utilisateur ayant pour Username: {userValues.UserName}";
                    Log.Info(identity, message);
                }
            }
            else
            {
                string domaine = ConfigurationManager.AppSettings["DomaineName"];
                string message = $"Le {identity.UserName} est un utilisateur {domaine}, il est impossible de l'insérer comme un utilisateur externe";
                Log.Error(identity, message);
            }

            return response;
                  
        }                

        public static bool ModifyIdentity(IdentityUpdateRequest userValues, Identity identity)
        {
            Identity userToModify = GetUser(userValues.UserName);

            string sql = userToModify.IsExternalUser ? SqlUtilisateur.Update : SqlUtilisateur.UpdateBcc;

            bool response = false;
            try
            {
                int nbreOfRow = SqlDataAccess.SaveData(sql, userValues, identity);

                if (nbreOfRow > 0)
                {
                    response = true;
                    string message = $"Modification de l'utilisateur {userValues.UserName}";
                    Log.Info(identity, message);
                }

                return response;
            }
            catch (Exception e)
            {
                Log.Error(identity, e.Message);
                throw new Exception("Oup ! L'utilisateur n'a pas pu être modifié, veillez contacter l'administrateur");
            }
            
        }

        
        public static Identity ValidateApplicationUser(string userName, string password)
        {
            string message;
            Identity user = GetUser(userName); 
            
            if(user == null)
            {
                message = $"L'utilisateur {userName} n'existe pas !";
                Log.Error(user, message);
                throw new Exception(message);
            }


            if (user.IsExternalUser)
            {
                PasswordVerificationResult result = new PasswordHasher().VerifyHashedPassword(user.GetPassword(), password);

                if (result != PasswordVerificationResult.Success)
                {
                    _ = SqlDataAccess.SaveData(SqlUtilisateur.UpdateTentative, new { user.IdUser }, user);

                    if (int.TryParse(ConfigurationManager.AppSettings["NbreTentative"], out int nbreTentatives))
                    {
                        if (user.NbreTentatives + 1 == nbreTentatives)
                        {
                            _ = SqlDataAccess.SaveData(SqlUtilisateur.Lock, new { user.UserName }, user);
                        }
                    }

                    message = "Mot de passe incorrecte !";
                    Log.Error(user, message);
                    throw new Exception(message); 
                }

            }
            else
            {
                //Agent BCCC Vérifiez now dans AD 

                try
                {
                    if (!IsAuthenticated(userName, password))
                    {
                        message = "Compte ou mot de passe incorrecte !";
                        throw new Exception(message); // connextion AD échoué 
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message); // gère le message de AD
                }
            }

            if (user.IsExternalUser)
            {
                _ = SqlDataAccess.SaveData(SqlUtilisateur.UpdateResetTentative, new { user.IdUser }, user);
            }

            _ = SqlDataAccess.SaveData(SqlUtilisateur.LastConnexion, new { user.IdUser }, user);

            message = $"Utilisateur {user.UserName} trouvé avec succès ! ";
            Log.Info(user, message);

            return user;
        }

        private static bool IsAuthenticated(string username, string password)
        {
            try
            {
                string domaineName = ConfigurationManager.AppSettings["DomaineName"];

                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domaineName, username, password, AuthenticationTypes.Secure);
                DirectorySearcher search = new DirectorySearcher(entry)
                {
                    Filter = "(objectClass=user)",
                    SearchScope = SearchScope.Subtree
                };
                SearchResult result = search.FindOne();
                return result != null;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static Guid? ValidateUser(string userName, string password)
        {
            Identity user = ValidateApplicationUser(userName, password);            

            return user.IdUser;
        }

        public static Identity GetUser(string userName)
        {
            Identity user = SqlDataAccess.SelectData<Identity>(SqlUtilisateur.SelectByUserName, new { UserName = userName }).FirstOrDefault();

            //if (user == null)
            //{
            //    user = TransformAgentToUser(PatientController.SelectPatient(true, userName).FirstOrDefault());
            //}

            return user;
        }

        public static Identity GetFocalPoint()
        {
            Identity user = SqlDataAccess.SelectData<Identity>(SqlUtilisateur.SelectFocalPoint).FirstOrDefault();

            return user;
        }

        public static bool ChangePassword(Guid? idUser, string password,Identity user)
        {
            string hashPassword = new PasswordHasher().HashPassword(password);
            return SqlDataAccess.SaveData(SqlUtilisateur.UpdatePassword, new { Password = hashPassword, IdUser = idUser }, user) > 0;
        }

        public static int ResetPassword(Guid idUser, Identity user)
        {
            string hashPassword = new PasswordHasher().HashPassword(ConfigurationManager.AppSettings["DefaultPassword"]);
            return SqlDataAccess.SaveData(SqlUtilisateur.ResetPassword, new { Password = hashPassword, IdUser = idUser }, user);
        }

        public static string GetPassword(Guid? idUser)
        {
            return SqlDataAccess.SelectData<string>(SqlUtilisateur.SelectPassword, new { IdUser = idUser }).FirstOrDefault();
        }

        public static Identity TransformAgentToUser(Agent agent, int? idProfil = null)
        {
            string sexe = agent.Sexe == 1 ? "M" : "F";

            Identity user = new Identity()
            {
                IdUser = agent.SerialId,
                UserName = agent.Matricule.Substring(0, 6),
                Nom = agent.Nom?.Trim(),
                Postnom = agent.Postnom?.Trim(),
                Prenom = agent.Prenom?.Trim(),
                Sexe = sexe,
                Telephone = agent.Telephone?.Trim(),
                Email = agent.Email?.Trim(),
                IdProfil = (int)(idProfil == null ? ProfilController.GetDefaultProfil().IdProfil : idProfil)
            };

            return user;
        }

    }
}