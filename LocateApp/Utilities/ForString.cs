using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace LocateApp.Utilities
{        
    public static class ForString
    {
        public static bool IsId(string matricule)
        {
            return Regex.IsMatch(matricule, @"^[0-9]+$") || matricule.Contains("GOV0");
        }

        public static bool IsMatricule(string matricule)
        {
            return IsId(matricule) && matricule.Length == 6;
        }

        public static bool IsNip(string matricule)
        {
            return IsId(matricule) && matricule.Length == 8;
        }

        public static bool CompareCompositeString(string firstString, string secondString, char separator = ' ')
        {
            bool response = true;

            List<string> listFirstString = firstString?.Trim()?.Split(separator)?.ToList();
            listFirstString = listFirstString?.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            List<string> listSecondString = secondString?.Trim()?.Split(separator)?.ToList();
            listSecondString = listSecondString?.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (listFirstString?.Count > 1 && listFirstString?.Count == listSecondString?.Count)
            {

                for (int i = 0; i < listFirstString.Count; i++)
                {
                    if (!string.Equals(listFirstString[i]?.Trim(), listSecondString[i]?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        response = false;
                        break;
                    }
                }                
            }
            else            
            {
                response = string.Equals(firstString?.Trim(), secondString?.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            return response;
        }

        public static DateTime StringToDate(string dateString)
        {
            CultureInfo provider = new CultureInfo("fr-FR");
            string format = "dd/MM/yyyy";
            return DateTime.ParseExact(dateString, format, provider);
        }

        public static string ShowCorrectionBetween(string nom, string nomAfficheable)
        {
            string nomForTooltip = string.IsNullOrEmpty(nom) ? "... non renseigné" : nom.ToUpper();

            //string.Compare(nom?.Trim(), nomAfficheable?.Trim(), true) == 0 
            //ForString.CompareCompositeString(Nom?.Trim(), MoviaNom?.Trim())
            //string.Equals(Nom?.Trim(), MoviaNom?.Trim(), StringComparison.OrdinalIgnoreCase)

            //return string.Equals(nom?.Trim(), nomAfficheable?.Trim(), StringComparison.OrdinalIgnoreCase) ? nomAfficheable : $"<a class=\"tooltipped red-text\" data-position=\"bottom\" data-delay=\"50\" data-tooltip=\"{nomForTooltip}\">{nomAfficheable}</a>";
            //return (string.Compare(nom?.Trim(), nomAfficheable?.Trim(), true) <= 0) ? nomAfficheable : $"<a class=\"tooltipped red-text\" data-position=\"bottom\" data-delay=\"50\" data-tooltip=\"{nomForTooltip}\">{nomAfficheable}</a>";
            return CompareCompositeString(nom?.Trim(), nomAfficheable?.Trim()) ? nomAfficheable : $"<a class=\"tooltipped red-text\" data-position=\"bottom\" data-delay=\"50\" data-tooltip=\"{nomForTooltip}\">{nomAfficheable}</a>";
        }

        public static string ShowInput(string value, string name, bool isModification = false, string type = "text", string label = null, List<DataClient> dataClient = null)  //a.localeCompare(b, 'fr', { sensitivity: 'base' })
        {
            string inputStyle = "border-bottom:1px solid #E7E7E7;border-left:1px solid #E7E7E7;border-right:1px solid #E7E7E7;border-top:1px solid #E7E7E7;text-align:center;height:35px; marging: 0px 0px 0px 0px;";
            string selectStyle = "border:1px solid #CCC;-moz-border-radius: 50px 20px 20px 50px;-webkit-border-radius: 50px 20px 20px 50px;padding:2px;"; //  #CCC         
            string returnValue;

            switch (type)
            {
                case "date":
                    //<input placeholder="" id="mask1" name="DateNaissanceBrute" value="@dateNaissance" class="masked" type="text" data-inputmask="'alias': 'date'">
                    returnValue = isModification ? "<input type=\"text\" id=\"mask1\" name=\"" + name + "\" style=\"" + inputStyle + "\" value=\"" + value + "\" class=\"masked\" data-inputmask=\"'alias': 'date'\" onkeyup=\"if( $(this).val().localeCompare('" + value + "', 'fr', { sensitivity: 'base' }) != 0)$(this).css('border-color', 'orangered'); if($(this).val().localeCompare('" + value + "', 'fr', { sensitivity: 'base' }) == 0)$(this).css('border-color', '#E7E7E7');\" />" : label==null? $"{value} &nbsp;" : $"{label} &nbsp;";

                    break;
                
                case "select":

                    if (isModification)
                    {
                        string selectByDefault;

                        returnValue = $"<select name=\"{name}\" class=\"browser-default\" style=\"{selectStyle}\" onchange=\"if($(this).val() != '{value}')$(this).css('border-color', 'orangered'); if($(this).val() == '{value}')$(this).css('border-color', '#E7E7E7');\">";

                        foreach (var select in dataClient)
                        {                        
                           selectByDefault = (select.Value == value) ? "selected" : "";
                           returnValue += $"<option value=\"{select.Value}\" {selectByDefault}>{select.Label}</option>";                            
                        }

                        returnValue += "</select>";
                    }
                    else
                    {
                        returnValue = label;
                    }

                    break;

               default:
                   
                    returnValue = isModification ? "<input type=\"text\" name=\"" + name + "\" style=\"" + inputStyle + "\" value=\"" + value + "\" onkeyup=\"if( $(this).val().localeCompare('" + value + "', 'fr', { sensitivity: 'base' }) != 0)$(this).css('border-color', 'orangered'); if($(this).val().localeCompare('" + value + "', 'fr', { sensitivity: 'base' }) == 0)$(this).css('border-color', '#E7E7E7');\" />" : label == null ? $"{value} &nbsp;" : $"{label} &nbsp;";
                    break;
            }      

            return returnValue; 
        }


    }
}