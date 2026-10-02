using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class EtiquetteFormatListViewModel : ViewModel
    {
        public List<EtiquetteFormat> EtiquetteFormats { get; set; }
        public string Link { get; set; }

        public EtiquetteFormatListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/etiquette/format/modify/";
        }
    }
}