using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{
    public class BilletEnvoiRequest
    {
        public long Id { get; set; }
    }
    public class BilletEnvoiFromHopitalInsertRequest
    {
        public string IdPatient { get; set; }
        public long IdServiceHopital { get; set; }
        public bool IsUrgent { get; set; }
        public string Observation { get; set; }
        public bool IsOrdonance { get; set; }
    }

    public class BilletEnvoiBCCInsertRequest
    {
        public string Nip { get; set; }
        public string RefCodeHopital { get; set; }
        public string Observation { get; set; }
        public List<long> Specialites { get; set; } = new List<long>();
        public bool IsOrdonance { get; set; }
    }

    public class BilletEnvoiBCCModifyRequest
    {
        public long Id { get; set; }
        public string Nip { get; set; }
        public string IdHopital { get; set; }
        public string Observation { get; set; }
        public List<long> Specialites { get; set; } = new List<long>();
        public bool IsOrdonance { get; set; }
    }
}