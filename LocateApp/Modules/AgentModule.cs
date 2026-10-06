using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Utilities;
using LocateApp.ViewModels;

namespace LocateApp.Modules
{
    /// <summary>Répertoire des agents de l'entreprise (table Agent) : liste, ajout, modification.</summary>
    public class AgentModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(AgentModule));

        public AgentModule() : base("/agent")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler(GetAgents));
            Get("/add/", _ => this.RunHandler(GetAddAgentForm));
            Get("/modify/{matricule}", _ => this.RunHandler<string>(GetModifyAgentForm, (string)_.matricule));

            Post("/add/", _ => this.RunHandler<AgentFormulaireRequest>(AddAgent));
            Post("/modify/", _ => this.RunHandler<AgentFormulaireRequest>(ModifyAgent));
        }

        private object GetAgents()
        {
            List<AgentRepertoire> agents = AgentController.SelectRepertoire();

            AgentRepertoireViewModel viewModel = new AgentRepertoireViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Agents = agents
            };

            object view = View["AgentRepertoireView", viewModel];
            int success = agents.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, agents, viewModel.Title, success);
        }

        private object GetAddAgentForm()
        {
            AgentFrmViewModel viewModel = new AgentFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                MatriculePropose = AgentController.ProchainMatricule()
            };

            object view = View["AgentFrmView", viewModel];

            return this.ResponseObject(view, null, viewModel.Title, 1);
        }

        private object GetModifyAgentForm(string matricule)
        {
            string court = matricule?.Trim();
            if (court != null && court.Length == 8 && court.EndsWith("00")) { court = court.Substring(0, 6); }
            Agent agent = AgentController.SelectByMatricule(court);

            if (agent == null)
            {
                return this.RedirectUrl("/agent/", $"Aucun agent n'a le matricule {matricule}.", Log.ERROR_CODE, "Modification d'un agent");
            }

            AgentFrmViewModel viewModel = new AgentFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Agent = agent,
                Compte = IdentityController.GetUser(court) == null ? null : court
            };

            object view = View["AgentFrmView", viewModel];

            return this.ResponseObject(view, agent, viewModel.Title, 1);
        }

        private object AddAgent(AgentFormulaireRequest agent)
        {
            const string titre = "Ajout d'un agent";
            string erreur = AgentController.Valider(agent, true);
            if (erreur != null)
            {
                return this.RedirectUrl("/agent/add/", erreur, Log.ERROR_CODE, titre);
            }

            bool result = AgentController.Inserer(agent, IdentityController.GetUser(this.CurrentUserName()));
            string remarquePhoto = result ? EnregistrerPhoto(agent.Matricule) : null;

            string nom = $"{agent.Nom} {agent.Postnom} {agent.Prenom}".Trim();
            string message = result ? $"L'agent {nom} a été ajouté avec le matricule {agent.Matricule}.{remarquePhoto}" : $"L'ajout de l'agent {nom} a échoué.";
            return this.RedirectUrl(result ? "/agent/" : "/agent/add/", message, result ? Log.SUCCESS_CODE : Log.ERROR_CODE, titre);
        }

        private object ModifyAgent(AgentFormulaireRequest agent)
        {
            const string titre = "Modification d'un agent";
            string erreur = AgentController.Valider(agent, false);
            if (erreur != null)
            {
                return this.RedirectUrl("/agent/modify/" + agent.Matricule, erreur, Log.ERROR_CODE, titre);
            }

            bool result = AgentController.Modifier(agent, IdentityController.GetUser(this.CurrentUserName()));
            string remarquePhoto = result ? EnregistrerPhoto(agent.Matricule) : null;

            string nom = $"{agent.Nom} {agent.Postnom} {agent.Prenom}".Trim();
            string message = result ? $"L'agent {nom} ({agent.Matricule}) a été modifié.{remarquePhoto}" : $"La modification de l'agent {nom} a échoué.";
            return this.RedirectUrl(result ? "/agent/" : "/agent/modify/" + agent.Matricule, message, result ? Log.SUCCESS_CODE : Log.ERROR_CODE, titre);
        }

        /// <summary>
        /// Photo envoyée avec le formulaire : recadrée en carré, réduite à 400 px et enregistrée en JPEG sous {matricule}00.jpg,
        /// le nom lu par la liste des agents, les responsables et le compte utilisateur. Renvoie une remarque si la photo est refusée.
        /// </summary>
        private string EnregistrerPhoto(string matricule)
        {
            var fichier = this.Request.Files.FirstOrDefault(f => f.Value != null && f.Value.Length > 0);
            if (fichier == null) { return null; }

            try
            {
                using (Image source = Image.FromStream(fichier.Value))
                {
                    int cote = Math.Min(source.Width, source.Height);
                    var cadre = new Rectangle((source.Width - cote) / 2, (source.Height - cote) / 3, cote, cote);
                    int taille = Math.Min(400, cote);

                    using (var photo = new Bitmap(taille, taille))
                    using (Graphics g = Graphics.FromImage(photo))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(Color.White);
                        g.DrawImage(source, new Rectangle(0, 0, taille, taille), cadre, GraphicsUnit.Pixel);

                        string chemin = Path.Combine(new DefaultRootPathProvider().GetRootPath(), "Content", "images", "photos", matricule + "00.jpg");
                        ImageCodecInfo jpeg = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        using (var parametres = new EncoderParameters(1))
                        {
                            parametres.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
                            photo.Save(chemin, jpeg, parametres);
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Error(IdentityController.GetUser(this.CurrentUserName()), $"Photo de l'agent {matricule} refusée : {e.Message}");
                return " La photo n'a pas pu être lue (format d'image non reconnu) : elle n'a pas été enregistrée.";
            }
        }
    }
}
