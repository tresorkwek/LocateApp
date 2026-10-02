using Nancy;
using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp
{
    public class AppUserMapper : IUserMapper
    {
        public ClaimsPrincipal GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var user = IdentityController.GetIdentity(identifier).FirstOrDefault();

            if (user == null)
            {
                user = IdentityController.TransformAgentToUser(AgentController.SelectPatientBySerialId(identifier.ToString()).FirstOrDefault());
            }

            return user == null
                ? null
                : new ClaimsPrincipal(
                    new GenericPrincipal(new GenericIdentity(user.UserName),
                                         user.Claims.ToArray()));
        }
                
    }
}