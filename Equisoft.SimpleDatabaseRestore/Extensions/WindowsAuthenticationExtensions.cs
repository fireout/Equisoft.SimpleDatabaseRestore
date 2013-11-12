using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using Nancy;
using Nancy.Security;

namespace Equisoft.SimpleDatabaseRestore.Extensions
{
    public static class WindowsAuthenticationExtensions
    {
        private class WindowsUserIdentity : IUserIdentity
        {
            private readonly string _userName;

            public WindowsUserIdentity(string userName)
            {
                _userName = userName;
            }

            #region IUserIdentity

            IEnumerable<string> IUserIdentity.Claims
            {
                get { throw new NotImplementedException(); }
            }

            string IUserIdentity.UserName
            {
                get { return _userName; }
            }

            #endregion
        }

        #region Methods

        /// <summary>
        ///     Forces the NancyModule to require a user to be Windows authenticated. Non-authenticated
        ///     users will be sent HTTP 401 Unauthorized.
        /// </summary>
        /// <param name="module"></param>
        public static void RequiresWindowsAuthentication(this NancyModule module)
        {
            if (HttpContext.Current == null)
                throw new InvalidOperationException(
                    "An HttpContext is required. Ensure that this application is running under IIS.");

            module.Before.AddItemToEndOfPipeline(
                new PipelineItem<Func<NancyContext, Response>>(
                    "RequiresWindowsAuthentication",
                    context =>
                        {
                            IPrincipal principal = GetPrincipal();

                            if (principal == null || !principal.Identity.IsAuthenticated)
                            {
                                return HttpStatusCode.Unauthorized;
                            }

                            context.CurrentUser = new WindowsUserIdentity(principal.Identity.Name);

                            return null;
                        }));
        }

        private static IPrincipal GetPrincipal()
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.User;
            }

            return new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        #endregion
    }
}