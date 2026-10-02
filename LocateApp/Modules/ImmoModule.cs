using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.ModelBinding;
using Nancy.Security;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using LocateApp.Models;
using System.IO;
using LocateApp.Services;
using System.Threading.Tasks;

namespace LocateApp.Modules
{
    public class ImmoModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ImmoModule));

        public ImmoModule() : base("/immo")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler(GetArticle)); //ok
            Get("/article/{idArticle}", _ => this.RunHandler<long>(GetImmoByArticle, (long)_.idArticle)); //ok
            Get("/organe/list/", _ => this.RunHandler<string>(GetImmoViaOrganigramme, null));//ok
            Get("/organe/list/{idInstitution_}/", _ => this.RunHandler<string>(GetImmoViaOrganigramme, (string)_.idInstitution_)); //ok
            Get("/organe/select/{codeOrgane}/", _ => this.RunHandler<string>(GetImmoByOrgane, (string)_.codeOrgane)); //ok
            Get("/organe/select/{codeOrgane}/article/{idArticle}", _ => this.RunHandler<string, long>(GetImmoByOrganeAndArticle, (string)_.codeOrgane, (long)_.idArticle));

            Get("/entite/list/", _ => this.RunHandler<string>(GetImmoViaOrganigrammeEntite, null)); //ok
            Get("/entite/list/{idInstitution_}/", _ => this.RunHandler<string>(GetImmoViaOrganigrammeEntite, (string)_.idInstitution_)); //ok
            Get("/entite/select/{codeOrgane}/", _ => this.RunHandler<string>(GetImmoByEntite, (string)_.codeOrgane));
            Get("/entite/select/", _ => this.RunHandler<string>(GetImmoByEntite, null));

            Get("/responsable/list/", _ => this.RunHandler<string>(GetImmoViaOrganigrammeByResponsable, null));
            Get("/responsable/list/{idInstitution_}/", _ => this.RunHandler<string>(GetImmoViaOrganigrammeByResponsable, (string)_.idInstitution_)); //ok
            Get("/responsable/organe/{codeOrgane}/", _ => this.RunHandler<string>(GetResponsableImmoByOrgane, (string)_.codeOrgane));
            Get("/responsable/litigieux/", _ => this.RunHandler(GetResponsableImmoLitigieux));
            Get("/responsable/bien/{matricule}/", _ => this.RunHandler<string>(GetImmoByResponsable, (string)_.matricule));
            Get("/responsable/bienlitigieux/{matricule}/", _ => this.RunHandler<string>(GetImmoLitigieuxByResponsable, (string)_.matricule));

            Get("/select/", _ => this.RunHandler<string>(GetImmo, null));
            Get("/select/{code}", _ => this.RunHandler<string>(GetImmo, (string)_.code));
            Get("/id/{id}", _ => this.RunHandler<long>(GetImmoById, (long)_.id));
            Get("/qrcode/{qrCode}", _ => this.RunHandler<Guid>(GetImmoByQRCode, (Guid)_.qrCode));
            Get("/etiquette/{idEtiquette}", _ => this.RunHandler<long>(GetImmoByIdEtiquette, (long)_.idEtiquette));
            Get("/local/{idLocal}", _ => this.RunHandler<long>(GetImmoByLocal, (long)_.idLocal));
            Get("/local/{idLocal}/article/{idArticle}", _ => this.RunHandler<long,long>(GetImmoByLocalAndArticle, (long)_.idLocal, (long)_.idArticle));
            Get("/principal/{idLocal}", _ => this.RunHandler<long>(GetImmoPrincipalByLocal, (long)_.idLocal));
            Get("/sanslocal/", _ => this.RunHandler(GetImmoSansLocal));
            Get("/add/{idLocal}", _ => this.RunHandler<long>(GetAddImmoForm, (long)_.idLocal));
            Get("/modify/{id}", _ => this.RunHandler<long>(GetModifyImmoForm, (long)_.id));
            Get("/resetqrcode/{idImmo}", _ => this.RunHandler<long, bool>(DesaffectQRCodeToImmo, (long)_.idImmo, true));
            Get("/liveqrcode/{idImmo}", _ => this.RunHandler<long, bool>(DesaffectQRCodeToImmo, (long)_.idImmo, false));
          //  Get("/declassement/{idImmo}", _ => this.RunHandler<long>(Declassement, (long)_.idImmo));
            Get("/nonvu/", _ => this.RunHandler(GetImmoNonVu));
            Get("/transit/", _ => this.RunHandler(GetImmoTransit));
            Get("/declasser/", _ => this.RunHandler(GetImmoDeclasser));

            Post("/nonvu/{id}", _ => this.RunHandler<long>(NonVu, (long)_.id));
            Post("/declasser/{id}", _ => this.RunHandler<long>(Declasser, (long)_.id));
            Post("/cession/{id}", _ => this.RunHandler<long>(Cession, (long)_.id));
            Post("/local/cession/", _ => this.RunHandler(CessionLocal));
            Post("/select/", _ => this.RunHandler<SearchImmoRequest>(SearchImmo));
            Post("/qrcode/", _ => this.RunHandler<AffectQRCodeToImmoRequest>(AffectQRCodeToImmo));
            Post("/changelocal/", _ => this.RunHandler<ChangeLocalOfImmoRequest>(ChangeLocal));
            Post("/misenservice/", _ => this.RunHandler<ChangeLocalOfImmoRequest>(MisEnService));
            Post("/expedier/", _ => this.RunHandler<ExpedierImmoRequest>(Expedier));


            Post("/add/", _ => this.RunHandler<InsertImmoRequest>(InsertImmo));
            Post("/add/photo", _ => this.RunHandler<InsertPhotoImmoFoTabletRequest>(InsertPhotoImmo));
            Post("/modify/", _ => this.RunHandler<ModifyImmoRequest>(UpdateImmo));
            

            Post("/sync/add/", _ => this.RunHandler<InsertImmoSyncRequest>(InsertImmoSync));
            Post("/sync/modify/", _ => this.RunHandler<ModifyImmoSyncRequest>(UpdateImmoSync));
        }

        private object SearchImmo(SearchImmoRequest searchRequest)
        {
            string redirectUrl = $"/immo/select/{searchRequest.Code}";

            return this.RedirectUrl(redirectUrl, string.Empty, 0);
        }

        private object GetArticle()
        {
            List<Article> listOfArticle = ArticleController.Select();

            ArticleListViewModel viewModelOfArticle = new ArticleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Articles = listOfArticle,
                Link = "article/"
        };

            object view = View["ImmoArticleListView", viewModelOfArticle];
            int success = listOfArticle.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfArticle, viewModelOfArticle.Title, success);
        }

        private object GetImmoViaOrganigramme(string codeOrgane = null)
        {
            List<Organe> organigramme = OrganeController.Organigramme(codeOrgane);

            if (organigramme == null)
            {
                Identity identity = IdentityController.GetUser(this.CurrentUserName());
                string redirectUrl = identity.Profil.HomeUrl;
                string messageTitle = "Liste des immos";
                int sucess = Log.ERROR_CODE;
                string message = "L'organe que vous avez renseigné n'existe pas !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme,
                Link = "/immo/organe/select/"
            };

            object view = View["ImmoOrganigrammeView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }

        private object GetImmoViaOrganigrammeEntite(string codeOrgane = null)
        {
            List<Organe> organigramme = OrganeController.OrganigrammeEntite(codeOrgane);

            if (organigramme == null)
            {
                Identity identity = IdentityController.GetUser(this.CurrentUserName());
                string redirectUrl = identity.Profil.HomeUrl;
                string messageTitle = "Liste des immos";
                int sucess = Log.ERROR_CODE;
                string message = "L'organe que vous avez renseigné n'existe pas !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme,
                Link = "/immo/entite/select/"
            };

            object view = View["ImmoOrganigrammeView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }

        private object GetImmoViaOrganigrammeByResponsable(string codeOrgane = null)
        {
            List<Organe> organigramme = OrganeController.Organigramme(codeOrgane);

            if (organigramme == null)
            {
                Identity identity = IdentityController.GetUser(this.CurrentUserName());
                string redirectUrl = identity.Profil.HomeUrl;
                string messageTitle = "Liste des immos";
                int sucess = Log.ERROR_CODE;
                string message = "L'organe que vous avez renseigné n'existe pas !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme,
                Link = "/immo/responsable/organe/"
            };

            object view = View["ImmoOrganigrammeByResponsableView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }


        private object GetImmoByArticle(long idArticle)
        {
            List<Immo> listOfImmo = ImmoController.SelectByArticle(idArticle);

            ViewModel viewModel;
            string viewToShow;

            if (listOfImmo.Count == 1)
            {
                Agent agent = null;
                Organe organeAgent = null;
                Immo immo = listOfImmo.FirstOrDefault();

                if (immo.Responsable != null)
                {
                    agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();                   

                    organeAgent = OrganeController.SelectById(agent.CodeOrgane);
                }

                viewModel = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immo = immo,
                    Responsable = agent,
                    OrganeResponsable = organeAgent
                };

                viewToShow = "ImmoView";
            }
            else
            {
                viewModel = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immos = listOfImmo
                };

                viewToShow = "ImmoListView";
            }

            object view = View[viewToShow, viewModel];
            int success = listOfImmo.Count > 0 ? 1 : 0;


            return this.ResponseObject(view, listOfImmo, viewModel.Title, success);
        }

        private object GetImmoByResponsable(string matricule)
        {
            List<Immo> listOfImmo = ImmoController.SelectByResponsable(matricule);

            ViewModel viewModel;
            string viewToShow;

            if(listOfImmo.Count == 1)
            {
                Agent agent = null;
                Organe organeAgent = null;
                Immo immo = listOfImmo.FirstOrDefault();

                if (immo.Responsable != null)
                {
                    agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();                    

                    organeAgent = OrganeController.SelectById(agent.CodeOrgane);
                }

                viewModel = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immo = immo,
                    Responsable = agent,
                    OrganeResponsable = organeAgent
                };

                viewToShow = "ImmoView";
            }
            else
            {
                viewModel = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immos = listOfImmo
                };

                viewToShow = "ImmoListView";
            }

            object view = View[viewToShow, viewModel];
            int success = listOfImmo.Count > 0 ? 1 : 0;

            
            return this.ResponseObject(view, listOfImmo, viewModel.Title, success);
        }

        private object GetImmoLitigieuxByResponsable(string matricule)
        {
            List<Immo> listOfImmo = ImmoController.SelectLitigeByResponsable(matricule);

            ViewModel viewModel;
            string viewToShow;

            if (listOfImmo.Count == 1)
            {
                Agent agent = null;
                Organe organeAgent = null;
                Immo immo = listOfImmo.FirstOrDefault();

                if (immo.Responsable != null)
                {
                    agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();                   

                    organeAgent = OrganeController.SelectById(agent.CodeOrgane);
                }

                viewModel = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immo = immo,
                    Responsable = agent,
                    OrganeResponsable = organeAgent
                };

                viewToShow = "ImmoView";
            }
            else
            {
                viewModel = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immos = listOfImmo
                };

                viewToShow = "ImmoListView";
            }

            object view = View[viewToShow, viewModel];
            int success = listOfImmo.Count > 0 ? 1 : 0;


            return this.ResponseObject(view, listOfImmo, viewModel.Title, success);
        }

        private object GetImmo(string code = null)
        {
            List<Immo> listOfImmo = ImmoController.Select(code);

            ViewModel viewModel;
            string viewToShow;

            if (listOfImmo.Count == 1)
            {
                Agent agent = null;
                Organe organeAgent = null;
                Immo immo = listOfImmo.FirstOrDefault();

                if (immo.Responsable != null)
                {
                    agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();                    

                    organeAgent = OrganeController.SelectById(agent.CodeOrgane);
                }

                viewModel = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immo = immo,
                    Responsable = agent,
                    OrganeResponsable = organeAgent
                };

                viewToShow = "ImmoView";
            }
            else
            {
                viewModel = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immos = listOfImmo
                };

                viewToShow = "ImmoListView";
            }

            object view = View[viewToShow, viewModel];
            int success = listOfImmo.Count > 0 ? 1 : 0;


            return this.ResponseObject(view, listOfImmo, viewModel.Title, success);
        }

        private object GetImmoByOrgane(string idOrgane)
        {
            return AfficherImmoOrgane(idOrgane, null);
        }

        /// <summary>Biens d'un article dans un organe (pendant de /immo/local/{idLocal}/article/{idArticle}).</summary>
        private object GetImmoByOrganeAndArticle(string idOrgane, long idArticle)
        {
            return AfficherImmoOrgane(idOrgane, idArticle);
        }

        private object AfficherImmoOrgane(string idOrgane, long? idArticle)
        {
            List<Immo> listOfImmo = ImmoController.SelectByOrgane(idOrgane);
            if (idArticle != null) { listOfImmo = listOfImmo.Where(i => i.IdArticle == idArticle).ToList(); }

            ViewModel viewModel;
            string viewToShow;

            if (listOfImmo.Count == 1)
            {
                Agent agent = null;
                Organe organeAgent = null;
                Immo immo = listOfImmo.FirstOrDefault();

                if (immo.Responsable != null)
                {
                    agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();                   

                    organeAgent = OrganeController.SelectById(agent.CodeOrgane);
                }

                viewModel = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immo = immo,
                    Responsable = agent,
                    OrganeResponsable = organeAgent
                };

                viewToShow = "ImmoView";
            }
            else
            {
                viewModel = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immos = listOfImmo,
                    Organe = OrganeController.SelectById(idOrgane),
                    Article = idArticle == null ? null : ArticleController.SelectById((long)idArticle).FirstOrDefault(),
                    LienParArticle = $"/article/organe/{idOrgane}",
                    LienDetails = $"/immo/organe/select/{idOrgane}/"
                };

                viewToShow = "ImmoListView";
            }

            object view = View[viewToShow, viewModel];
            int success = listOfImmo.Count > 0 ? 1 : 0;


            return this.ResponseObject(view, listOfImmo, viewModel.Title, success);
        }

        private object GetImmoByEntite(string idOrgane)
        {
            List<Immo> listOfImmo = ImmoController.SelectByEntite(idOrgane);

            ViewModel viewModel;
            string viewToShow;

            if (listOfImmo.Count == 1)
            {
                Agent agent = null;
                Organe organeAgent = null;
                Immo immo = listOfImmo.FirstOrDefault();

                if (immo.Responsable != null)
                {
                    agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();                                       

                    organeAgent = OrganeController.SelectById(agent.CodeOrgane);
                }

                viewModel = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immo = immo,
                    Responsable = agent,
                    OrganeResponsable = organeAgent
                };

                viewToShow = "ImmoView";
            }
            else
            {
                viewModel = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
                {
                    MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                    CurrentClaim = this.GetClaimString(),
                    Immos = listOfImmo
                };

                viewToShow = "ImmoListView";
            }

            object view = View[viewToShow, viewModel];
            int success = listOfImmo.Count > 0 ? 1 : 0;


            return this.ResponseObject(view, listOfImmo, viewModel.Title, success);
        }

        private object GetResponsableImmoByOrgane(string idOrgane)
        {
            List<Agent> listOfResponsable = AgentController.SelectResponsableByOrgane(idOrgane);

            AgentListViewModel viewModelOfAgent = new AgentListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                Agents = listOfResponsable,
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Link = "/immo/responsable/bien/"
            };

            object view = View["AgentListView", viewModelOfAgent];
            int success = listOfResponsable.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfResponsable, viewModelOfAgent.Title, success);
        }

        private object GetResponsableImmoLitigieux()
        {
            List<Agent> listOfResponsable = AgentController.SelectResponsableLitigieux();

            AgentListViewModel viewModelOfAgent = new AgentListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                Agents = listOfResponsable,
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Link = "/immo/responsable/bienlitigieux/"
            };

            object view = View["AgentListLitigieuxView", viewModelOfAgent];
            int success = listOfResponsable.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfResponsable, viewModelOfAgent.Title, success);
        }

        private async Task<object> GetImmoByIdViaWS(long id)
        {
            Immo immo = ImmoController.SelectById(id);
            Agent agent = null;

            if(immo.Responsable != null)
            {
                Token token = await MoviaAgentApiService.LoginToMoviaAsync();
                if (token != null)
                {
                    agent = MoviaAgentApiService.GetAgentInfoFromMoviaAsync(immo.Responsable, token);
                }
            }

            ImmoViewModel viewModelOfImmo = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immo = immo,
                Responsable = agent
            };

            object view = View["ImmoView", viewModelOfImmo];
            int success = immo != null ? 1 : 0;

            return this.ResponseObject(view, immo, viewModelOfImmo.Title, success);
        }

        private object GetImmoById(long id)
        {
            Immo immo = ImmoController.SelectById(id);
            Agent agent = null;
            Organe organeAgent = null;

            if (!string.IsNullOrEmpty(immo.Responsable))
            {
                agent = AgentController.SelectAgent(immo.Responsable).FirstOrDefault();

                

                organeAgent = OrganeController.SelectById(agent.CodeOrgane);
            }

            ImmoViewModel viewModelOfImmo = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immo = immo,
                Responsable = agent,
                OrganeResponsable = organeAgent
            };

            object view = View["ImmoView", viewModelOfImmo];
            int success = immo != null ? 1 : 0;

            return this.ResponseObject(view, immo, viewModelOfImmo.Title, success);
        }

        private object GetImmoByQRCode(Guid qrCode)//GetImmoRequest
        {
           // List<Immo> listOfImmo = ImmoController.SelectByQrCode(qrCode);
            List<GetImmoRequest> listOfImmo = ImmoController.SelectByQrCodeWithDetails(qrCode);

            ImmoViewModel viewModelOfImmo = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immo = listOfImmo.FirstOrDefault()
            };

            object view = View["ImmoView", viewModelOfImmo];
            int success = listOfImmo != null ? 1 : 0;

            return this.ResponseObject(view, listOfImmo, viewModelOfImmo.Title, success);
        }

        private object GetImmoByIdEtiquette(long idEtiquette)
        {
            Immo immo = ImmoController.SelectByIdEtiquette(idEtiquette);

            ImmoViewModel viewModelOfImmo = new ImmoViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immo = immo
            };

            object view = View["ImmoView", viewModelOfImmo];
            int success = immo != null ? 1 : 0;

            return this.ResponseObject(view, immo, viewModelOfImmo.Title, success);
        }

        private object GetImmoByLocal(long idLocal)
        {
            List<Immo> immos = ImmoController.SelectByLocal(idLocal);

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos,
                Local = LocalController.SelectById(idLocal).FirstOrDefault(),
                ActionLink = $"/immo/add/{idLocal}"
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);
        }

        private object GetImmoNonVu()
        {
            string redirectUrl = $"/";
            string messageTitle = "Bien non vu";
            int sucess = 0;
            string message;

            string codeOrganeAffectation = IdentityController.GetUser(this.CurrentUserName()).IdInstitution;

            if (string.IsNullOrEmpty(codeOrganeAffectation))
            {
                message = $"Vous devez avoir un organe d'affectation pour afficher les biens non vu !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(codeOrganeAffectation);

            Local local = LocalController.SelectNonVu(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Votre organe d'affectation n'a pas de local non vu";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }


            List<Immo> immos = ImmoController.SelectByLocal(local.Id);

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos,
                Local = LocalController.SelectById(local.Id).FirstOrDefault(),
                ActionLink = $"/immo/add/{local.Id}"
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);
        }

        private object GetImmoTransit()
        {
            string redirectUrl = $"/";
            string messageTitle = "Bien en transit";
            int sucess = 0;
            string message;

            string codeOrganeAffectation = IdentityController.GetUser(this.CurrentUserName()).IdInstitution;

            if (string.IsNullOrEmpty(codeOrganeAffectation))
            {
                message = $"Vous devez avoir un organe d'affectation pour afficher les biens en transit !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(codeOrganeAffectation);

            Local local = LocalController.SelectTransit(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Votre organe d'affectation n'a pas de local de transit";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            List<Immo> immos = ImmoController.SelectByLocal(local.Id);

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos,
                Local = LocalController.SelectById(local.Id).FirstOrDefault(),
                ActionLink = $"/immo/add/{local.Id}"
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);
        }


        private object GetImmoDeclasser()
        {
            string redirectUrl = $"/";
            string messageTitle = "Bien(s) Déclassé(s)";
            int sucess = 0;
            string message;

            string codeOrganeAffectation = IdentityController.GetUser(this.CurrentUserName()).IdInstitution;

            if (string.IsNullOrEmpty(codeOrganeAffectation))
            {
                message = $"Vous devez avoir un organe d'affectation pour afficher les biens déclassés !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(codeOrganeAffectation);

            Local local = LocalController.SelectDeclasser(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Votre organe d'affectation n'a pas de local déclassé";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }


            List<Immo> immos = ImmoController.SelectByLocal(local.Id);

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos,
                Local = LocalController.SelectById(local.Id).FirstOrDefault(),
                ActionLink = $"/immo/add/{local.Id}"
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);
        }

        private object GetImmoByLocalAndArticle(long idLocal, long idArticle)
        {
            List<Immo> immos = ImmoController.SelectByLocalAndArticle(idLocal, idArticle);

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos,
                Local = LocalController.SelectById(idLocal).FirstOrDefault(),
                ActionLink = $"/immo/add/{idLocal}"
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);
        }

        private object GetImmoPrincipalByLocal(long idLocal)
        {
            List<Immo> immos = ImmoController.SelectImmoPrincipalByLocal(idLocal);

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos,
                Local = LocalController.SelectById(idLocal).FirstOrDefault(),
                ActionLink = $"/immo/add/{idLocal}"
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);
        }

        private object GetImmoSansLocal()
        {
            List<Immo> immos = ImmoController.SelectSansLocal();

            ImmoListViewModel viewModelOfImmo = new ImmoListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Immos = immos
            };

            object view = View["ImmoListView", viewModelOfImmo];
            int success = immos != null ? 1 : 0;

            return this.ResponseObject(view, immos, viewModelOfImmo.Title, success);

        }

        private object AffectQRCodeToImmo(AffectQRCodeToImmoRequest affectQRCodeRequest)
        {
            string redirectUrl = $"/immo/id/{affectQRCodeRequest.Id}";
            string messageTitle = "Affectation du QRCode à un bien";
            int sucess = 0;
            string message;         

            Etiquette etiquette = EtiquetteController.SelectPrintedByQrCode(affectQRCodeRequest.QrCode).FirstOrDefault();

            if(etiquette == null)
            {
                 redirectUrl = $"/immo/id/{affectQRCodeRequest.Id}";
                 messageTitle = "Affectation du QRCode à un bien";

                 message = $"Vous tentez d'affecter un QRCode non imprimé par l'Hotel des Monnaies, Locate l'a rejeté !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }
            else if(etiquette.IsUsed)
            {
                redirectUrl = $"/immo/id/{affectQRCodeRequest.Id}";
                messageTitle = "Affectation du QRCode à un bien";

                message = $"Vous tentez d'affecter un QRCode déjà utilisé, Locate l'a rejeté !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }



            Immo immo = ImmoController.SelectById(affectQRCodeRequest.Id);
            bool result = ImmoController.AffectQRCode(affectQRCodeRequest, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} affecté au QRCode avec succès !" : $"L'affection du QRCode à l'équiment {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }



        private object DesaffectQRCodeToImmo(long idImmo, bool liberate)
        {
            string redirectUrl = $"/immo/id/{idImmo}";
            string messageTitle = "Désaffectation du QRCode à un bien";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(idImmo);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }
            else if (immo.QrCode == null)
            {               
                message = $"Le bien {immo.Designation} n'a pas de QRcode";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            bool result = ImmoController.DesAffectQRCode(immo, liberate, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} désaffecté du QRCode avec succès !" : $"La désaffection du QRCode à l'équiment {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }
        
        private object ChangeLocal(ChangeLocalOfImmoRequest changeLocalRequest)
        {
            
            string redirectUrl = $"/immo/id/{changeLocalRequest.IdImmo}";
            string messageTitle = "Délocaliser un bien";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(changeLocalRequest.IdImmo);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Local local = LocalController.SelectByQrCode(changeLocalRequest.QrCodeLocal).FirstOrDefault();

            if (local == null)
            {
                message = $"Le local d'affectation n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            ChangeLocalImmoRequest ChangeLocalImmoValues = new ChangeLocalImmoRequest()
            {
                Id = immo.Id,
                IdLocal = local.Id
            };

            bool result = ImmoController.ChangeLocal(ChangeLocalImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} envoyé au local {local.Designation} avec succès !" : $"La délocalisation du bien {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object MisEnService(ChangeLocalOfImmoRequest MisEnServiceRequest)
        {

            string redirectUrl = $"/immo/id/{MisEnServiceRequest.IdImmo}";
            string messageTitle = "Délocaliser un bien";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(MisEnServiceRequest.IdImmo);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Local local = LocalController.SelectByQrCode(MisEnServiceRequest.QrCodeLocal).FirstOrDefault();

            if (local == null)
            {
                message = $"Le local d'affectation n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            MisEnServiceImmoRequest MisEnServiceImmoValues = new MisEnServiceImmoRequest()
            {
                Id = immo.Id,
                IdLocal = local.Id
            };

            bool result = ImmoController.MisEnService(MisEnServiceImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} envoyé au local {local.Designation} avec succès !" : $"La délocalisation du bien {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object NonVu(long id)
        {

            string redirectUrl = $"/immo/id/{id}";
            string messageTitle = "Déclarer un bien non vu";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(id);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            string codeOrganeAffectation = IdentityController.GetUser(this.CurrentUserName()).IdInstitution;

            if (string.IsNullOrEmpty(codeOrganeAffectation))
            {
                message = $"Vous devez avoir un organe d'affectation pour désigner un bien comme non vu !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(codeOrganeAffectation);

            Local local = LocalController.SelectNonVu(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Cet organe n'a pas de local non vu";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            ChangeLocalImmoRequest ChangeLocalImmoValues = new ChangeLocalImmoRequest()
            {
                Id = immo.Id,
                IdLocal = local.Id
            };

            bool result = ImmoController.ChangeLocal(ChangeLocalImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} désigné comme non nu !" : $"La désignation du bien {immo.Designation} comme non vu a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object CessionLocal()
        {
            string redirectUrl = $"/immo/local/";
            string messageTitle = "Bien(s) Cédé(s)";
            int sucess = 0;
            string message;

            string codeOrganeAffectation = IdentityController.GetUser(this.CurrentUserName()).IdInstitution;

            if (string.IsNullOrEmpty(codeOrganeAffectation))
            {
                message = $"Vous devez avoir un organe d'affectation pour céder les biens déclassés !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(codeOrganeAffectation);

            Local local = LocalController.SelectDeclasser(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Votre organe d'affectation n'a pas de local des biens déclassés";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            DeclassementLocalImmoRequest DeclassementLocalImmoValues = new DeclassementLocalImmoRequest()
            {
                IdLocal = local.Id
            };

            bool result = ImmoController.CederLocal(DeclassementLocalImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"Les bien du local {local.Designation} cédés avec succès !" : $"La cession des biens du local {local.Designation} a échouée !";

            redirectUrl += local.Id;


            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
           
        }

        private object Cession(long idImmo)
        {
            string redirectUrl = $"/immo/id/{idImmo}";
            string messageTitle = "Cession du bien";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(idImmo);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }
            else if (immo.QrCode == null)
            {
                message = $"Le bien {immo.Designation} n'a pas de QRcode";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            DeclassementImmoRequest declassementImmoValues = new DeclassementImmoRequest()
            {
                IdImmo = immo.Id
            };

            bool result = ImmoController.Ceder(declassementImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} cédé avec succès !" : $"La cession du bien {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object Declasser(long id)
        {

            string redirectUrl = $"/immo/id/{id}";
            string messageTitle = "Déclasser un bien ";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(id);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            string codeOrganeAffectation = IdentityController.GetUser(this.CurrentUserName()).IdInstitution;

            if (string.IsNullOrEmpty(codeOrganeAffectation))
            {
                message = $"Vous devez avoir un organe d'affectation pour déclasser un bien !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(codeOrganeAffectation);

            Local local = LocalController.SelectDeclasser(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Cet organe n'a pas de local pour bien à Déclasser";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            ChangeLocalDeclassementImmoRequest ChangeLocalDeclassementImmoValues = new ChangeLocalDeclassementImmoRequest()
            {
                Id = immo.Id,
                IdLocal = local.Id
            };


            bool result = ImmoController.Declassement(ChangeLocalDeclassementImmoValues, IdentityController.GetUser(this.CurrentUserName()));


            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} déclassé !" : $"Le déclassement du bien {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object Expedier(ExpedierImmoRequest expedierImmoRequest)
        {

            string redirectUrl = $"/immo/id/{expedierImmoRequest.IdImmo}";
            string messageTitle = "Expédition d'un bien";
            int sucess = 0;
            string message;

            Immo immo = ImmoController.SelectById(expedierImmoRequest.IdImmo);

            if (immo == null)
            {
                redirectUrl = $"/immo/";
                message = $"Le bien selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            if (string.IsNullOrEmpty(expedierImmoRequest.CodeOrgane))
            {
                message = $"L'organe de destination n'existe pas !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            Organe organe = OrganeController.SelectById(expedierImmoRequest.CodeOrgane);

            Local local = LocalController.SelectTransit(organe.IdStructure).FirstOrDefault();

            if (local == null)
            {
                message = $"Cet organe n'a pas de local de transit";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            ChangeLocalImmoRequest ChangeLocalImmoValues = new ChangeLocalImmoRequest()
            {
                Id = immo.Id,
                IdLocal = local.Id
            };

            bool result = ImmoController.ChangeLocal(ChangeLocalImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{immo.Designation} expédié à {organe.NomStructure} avec succès !" : $"L'expédition du bien {immo.Designation} à {organe.NomStructure} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }



        private object GetAddImmoForm(long idLocal)
        {
            Local local = LocalController.SelectById(idLocal).FirstOrDefault();

            ImmoAddFrmViewModel viewModelOfImmo = new ImmoAddFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Local = local,
                Occupants = AgentController.SelectAgentByOrgane(local.CodeOrgane),
                ImmoAutonome = ImmoController.SelectImmoPrincipalByLocal(idLocal)
        };

            object view = View["ImmoAddFrmView", viewModelOfImmo];
            int success = 1;

            return this.ResponseObject(view, viewModelOfImmo, viewModelOfImmo.Title, success);
        }

        private object UpdateImmo(ModifyImmoRequest modifyImmoRequest)
        {
            bool correctResponsable = true;
            bool result = false;

            if (!string.IsNullOrEmpty(modifyImmoRequest.Responsable))
            {
                List<Agent> agents = AgentController.SelectAgent(modifyImmoRequest.Responsable);
                if (agents.Count == 1)
                {
                    modifyImmoRequest.Responsable = agents[0].Matricule.Substring(0, 6);
                }
                else
                {
                    correctResponsable = false;
                }
            }
            else
            {
                modifyImmoRequest.Responsable = null;
            }


            if (correctResponsable)
            {
                result = ImmoController.UpdateImmo(modifyImmoRequest, IdentityController.GetUser(this.CurrentUserName()));

                if (result)
                {

                    if (!string.IsNullOrEmpty(modifyImmoRequest.IdPhotoToDelete))
                    {
                        List<string> idPhotoToDelete = modifyImmoRequest.IdPhotoToDelete.Split(',').ToList();

                        for (int i = 0; i < idPhotoToDelete.Count; i++)
                        {
                            //suprimer les photos dans le repertoire 

                            DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();
                            var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", idPhotoToDelete[i] + ".jpg");

                            var uri = new Uri(filename, UriKind.Absolute);
                            File.Delete(uri.LocalPath);
                        }
                    }

                    if (this.Request.Files.FirstOrDefault() != null)
                    {
                        List<string> constats = modifyImmoRequest.Constat.Split(',').ToList();
                        int i = 0;
                        string nomPhoto;
                        Guid id;

                        InsertPhotoImmoRequest InsertPhotoImmoValues;

                        foreach (var file in this.Request.Files.ToList())
                        {
                            id = Guid.NewGuid();

                            InsertPhotoImmoValues = new InsertPhotoImmoRequest()
                            {
                                Id = id,
                                IdImmo = modifyImmoRequest.Id,
                                Constat = constats[i]
                            };

                            if (ImmoController.InsertPhoto(InsertPhotoImmoValues, IdentityController.GetUser(this.CurrentUserName())))
                            {
                                nomPhoto = id.ToString();

                                DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                                var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", nomPhoto + ".jpg");

                                using (var fileStream = new FileStream(filename, FileMode.Create))
                                {
                                    file.Value.CopyTo(fileStream);
                                }
                            }

                            i++;
                        }
                    }

                }
            }
                        
            string redirectUrl = $"/immo/id/{modifyImmoRequest.Id}";
            string messageTitle = "Modification d'un équipement";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;

            string message;

            if (correctResponsable)
            {
                Immo immo = ImmoController.SelectById(modifyImmoRequest.Id);
                message = result ? $"{immo.Designation} modifié avec succès !" : $"La modification de l'équiment {immo.Designation} a échouée !";
            }
            else
            {
                message = $"Le matricule du responsable {modifyImmoRequest.Responsable} n'est pas correcte !";
            }

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object UpdateImmoSync(ModifyImmoSyncRequest modifyImmoRequest)
        {
            bool correctResponsable = true;
            bool result = false;

            if (!string.IsNullOrEmpty(modifyImmoRequest.Responsable))
            {
                List<Agent> agents = AgentController.SelectAgent(modifyImmoRequest.Responsable);
                if (agents.Count == 1)
                {
                    modifyImmoRequest.Responsable = agents[0].Matricule.Substring(0, 6);
                }
                else
                {
                    correctResponsable = false;
                }
            }
            else
            {
                modifyImmoRequest.Responsable = null;
            }


            if (correctResponsable)
            {
                result = ImmoController.UpdateImmoSync(modifyImmoRequest, IdentityController.GetUser(this.CurrentUserName()));

                if (result)
                {

                    if (!string.IsNullOrEmpty(modifyImmoRequest.IdPhotoToDelete))
                    {
                        List<string> idPhotoToDelete = modifyImmoRequest.IdPhotoToDelete.Split(',').ToList();

                        for (int i = 0; i < idPhotoToDelete.Count; i++)
                        {
                            //suprimer les photos dans le repertoire 

                            DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();
                            var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", idPhotoToDelete[i] + ".jpg");

                            var uri = new Uri(filename, UriKind.Absolute);
                            File.Delete(uri.LocalPath);
                        }
                    }

                    if (this.Request.Files.FirstOrDefault() != null)
                    {
                        List<string> constats = modifyImmoRequest.Constat.Split(',').ToList();
                        int i = 0;
                        string nomPhoto;
                        Guid id;

                        InsertPhotoImmoRequest InsertPhotoImmoValues;

                        foreach (var file in this.Request.Files.ToList())
                        {
                            id = Guid.NewGuid();

                            InsertPhotoImmoValues = new InsertPhotoImmoRequest()
                            {
                                Id = id,
                                IdImmo = modifyImmoRequest.Id,
                                Constat = constats[i]
                            };

                            if (ImmoController.InsertPhoto(InsertPhotoImmoValues, IdentityController.GetUser(this.CurrentUserName())))
                            {
                                nomPhoto = id.ToString();

                                DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                                var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", nomPhoto + ".jpg");

                                using (var fileStream = new FileStream(filename, FileMode.Create))
                                {
                                    file.Value.CopyTo(fileStream);
                                }
                            }

                            i++;
                        }
                    }

                }
            }

            string redirectUrl = $"/immo/id/{modifyImmoRequest.Id}";
            string messageTitle = "Modification d'un équipement";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;

            string message;

            if (correctResponsable)
            {
                Immo immo = ImmoController.SelectById(modifyImmoRequest.Id);
                message = result ? $"{immo.Designation} modifié avec succès !" : $"La modification de l'équiment {immo.Designation} a échouée !";
            }
            else
            {
                message = $"Le matricule du responsable {modifyImmoRequest.Responsable} n'est pas correcte !";
            }

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object InsertImmo(InsertImmoRequest InsertImmoValues)
        {

            bool result = ImmoController.InsertImmo(InsertImmoValues, IdentityController.GetUser(this.CurrentUserName()), out long idImmo);

            Article article = ArticleController.SelectById(InsertImmoValues.IdArticle).FirstOrDefault();

            if (result && this.Request.Files.FirstOrDefault() != null)
            {
                List<string> constats = InsertImmoValues.Constat?.Split(',')?.ToList();

              
                int i = 0;
                string nomPhoto;
                Guid id;

                InsertPhotoImmoRequest InsertPhotoImmoValues;

                foreach (var file in this.Request.Files.ToList())
                {
                    id = Guid.NewGuid();

                    InsertPhotoImmoValues = new InsertPhotoImmoRequest()
                    {
                        Id = id,
                        IdImmo = idImmo,
                        Constat = constats != null && constats.Count > 0 ? constats[i] : null
                    };

                    if (ImmoController.InsertPhoto(InsertPhotoImmoValues, IdentityController.GetUser(this.CurrentUserName())))
                    {
                        nomPhoto = id.ToString();

                        DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                        var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", nomPhoto + ".jpg");

                        using (var fileStream = new FileStream(filename, FileMode.Create))
                        {
                            file.Value.CopyTo(fileStream);
                        }
                    }

                    i++;
                }
            }

            string redirectUrl = result ? $"/immo/id/{idImmo}" : $"/immo/local/{InsertImmoValues.IdLocal}";
            string messageTitle = "Création d'un équipement";
            int sucess = result ? 1 : Log.ERROR_CODE;
            string message = result ? $"{article.Designation} crée avec succès !" : $"La création de l'équiment {article.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object InsertImmoSync(InsertImmoSyncRequest InsertImmoValues)
        {

            string redirectUrl = $"/immo/local/{InsertImmoValues.IdLocal}";
            string messageTitle = "Synchronisation d'un bien";
            int sucess = 0;
            string message;

            if(InsertImmoValues.IdEtiquette == 0)
            {
                InsertImmoValues.IdEtiquette = null;
                InsertImmoValues.QrCode = null;
            }


            if(InsertImmoValues.QrCode != null)
            {
                Etiquette etiquette = EtiquetteController.SelectPrintedByQrCode(InsertImmoValues.QrCode).FirstOrDefault();

                if (etiquette == null)
                {
                    message = $"Vous tentez d'affecter un QRCode non imprimé par l'Hotel des Monnaies, Locate l'a rejeté !";
                    return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
                }
                else if (etiquette.IsUsed)
                {
                    message = $"Vous tentez d'affecter un QRCode déjà utilisé, Locate l'a rejeté !";
                    return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
                }
            }           

            bool result = ImmoController.InsertImmoSync(InsertImmoValues, IdentityController.GetUser(this.CurrentUserName()), out long idImmo);

            Article article = ArticleController.SelectById(InsertImmoValues.IdArticle).FirstOrDefault();

            if (result && this.Request.Files.FirstOrDefault() != null)
            {
                List<string> constats = InsertImmoValues.Constat?.Split(',')?.ToList();


                int i = 0;
                string nomPhoto;
                Guid id;

                InsertPhotoImmoRequest InsertPhotoImmoValues;

                foreach (var file in this.Request.Files.ToList())
                {
                    id = Guid.NewGuid();

                    InsertPhotoImmoValues = new InsertPhotoImmoRequest()
                    {
                        Id = id,
                        IdImmo = idImmo,
                        Constat = constats != null && constats.Count > 0 ? constats[i] : null
                    };

                    if (ImmoController.InsertPhoto(InsertPhotoImmoValues, IdentityController.GetUser(this.CurrentUserName())))
                    {
                        nomPhoto = id.ToString();

                        DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                        var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", nomPhoto + ".jpg");

                        using (var fileStream = new FileStream(filename, FileMode.Create))
                        {
                            file.Value.CopyTo(fileStream);
                        }
                    }

                    i++;
                }
            }

            redirectUrl = result ? $"/immo/id/{idImmo}" : $"/immo/local/{InsertImmoValues.IdLocal}";
            messageTitle = "Création d'un équipement";
            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{article.Designation} crée avec succès !" : $"La création de l'équiment {article.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object InsertPhotoImmo(InsertPhotoImmoFoTabletRequest InsertImmoPhotoValues)
        {

            bool result = false;

            if (Request.Files.FirstOrDefault() != null)
            {
                List<string> constats = InsertImmoPhotoValues.Constat?.Split(',')?.ToList();
                int i = 0;
                string nomPhoto;
                Guid id;

                InsertPhotoImmoRequest InsertPhotoImmoValues;

                foreach (var file in this.Request.Files.ToList())
                {
                    id = Guid.NewGuid();

                    InsertPhotoImmoValues = new InsertPhotoImmoRequest()
                    {
                        Id = id,
                        IdImmo = long.Parse(InsertImmoPhotoValues.IdImmo),
                        Constat = constats[i]
                    };

                    if (result = ImmoController.InsertPhoto(InsertPhotoImmoValues, IdentityController.GetUser(this.CurrentUserName())))
                    {
                        nomPhoto = id.ToString();

                        DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();

                        var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/equipements", nomPhoto + ".jpg");

                        using (var fileStream = new FileStream(filename, FileMode.Create))
                        {
                            file.Value.CopyTo(fileStream);
                        }
                    }

                    i++;
                }
            }

            Immo immo = ImmoController.SelectById(long.Parse(InsertImmoPhotoValues.IdImmo));

            string redirectUrl =  $"/immo/id/{immo.Id}";
            string messageTitle = "Photographie d'un bien";
            int sucess = result ? 1 : Log.ERROR_CODE;
            string message = result ? $"{immo.Designation} photographié avec succès !" : $"La photograhie du bien {immo.Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyImmoForm(long id)
        {
            Immo immo = ImmoController.SelectById(id);

            Local local = immo?.GetLocal();

            ImmoModifyFrmViewModel viewModelOfImmo = new ImmoModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                IsModification = true,
                Immo = immo,
                Occupants = local?.CodeOrgane == null ? new List<Agent>() : AgentController.SelectAgentByOrgane(local.CodeOrgane),
                ImmoAutonome = ImmoController.SelectImmoPrincipalByLocal(local.Id)
            };

            object view = View["ImmoModifyFrmView", viewModelOfImmo];
            int success = 1;

            return this.ResponseObject(view, viewModelOfImmo, viewModelOfImmo.Title, success);
        }

    }
}