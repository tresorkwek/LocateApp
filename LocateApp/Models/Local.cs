using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Local
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Designation { get; set; }
        public string CodeOrgane { get; set; }
        public Guid? QrCode { get; set; }
        public long? IdEtiquette { get; set; }
        public bool IsSpace { get; set; }
        public bool IsActive { get; set; }
        public int IdTypeLocal { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public long QuantiteImmo => GetQuantiteImmo();
        public long QuantiteImmoIdentifier => GetQuantiteImmoIdentifie();
        public long QuantiteImmoInventorier => GetQuantiteImmoInventorie();

        public Organe GetOrgane()
        {
            return OrganeController.Select(CodeOrgane).FirstOrDefault();
        }
        private long GetQuantiteImmo()
        {
            return ImmoController.SelectQuantiteByLocal(Id);
        }

        private long GetQuantiteImmoIdentifie()
        {
            return ImmoController.SelectQuantiteImmoIdentifieByLocal(Id);
        }

        private long GetQuantiteImmoInventorie()
        {
            return InventaireController.SelectQuantiteByLocal(Id);
        }

    }
}