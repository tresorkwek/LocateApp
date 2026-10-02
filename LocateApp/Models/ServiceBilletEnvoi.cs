using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.Models
{
    public class ServiceBilletEnvoi
    {
        public long IdService { get; set; }
        public long IdBilletEnvoi { get; set; }
        public string Denomination { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public bool IsUrgent { get; set; }
        public string Observation { get; set; }
        public DateTime DateExpiration { get; set; }
        public bool IsUsed => GetServiceState();

        private bool GetServiceState()
        {
            return ServiceBilletEnvoiController.GetPassagePatientByBilletEnvoiAndService(IdBilletEnvoi, IdService).Count > 0;
        }

    }
}