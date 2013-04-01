using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightScale.netClient.Core;
using RightScale.netClient;
using System.Activities;

namespace RightScale._3Tier.Workflow.Base
{
    public abstract class RSNativeActivity<T> : CodeActivity<T>
    {
        public InArgument<string> rsOAuthToken { get; set; }

        public InArgument<string> rsUserName { get; set; }

        public InArgument<string> rsPassword { get; set; }

        public InArgument<string> rsAccountID { get; set; }

        protected bool authClient(CodeActivityContext context)
        {
            bool isAuthed = false;
            if (!string.IsNullOrWhiteSpace(this.rsOAuthToken.Get(context)))
            {
                isAuthed = APIClient.Instance.Authenticate(this.rsOAuthToken.Get(context));
            }
            else if (!string.IsNullOrWhiteSpace(this.rsUserName.Get(context)) && !string.IsNullOrWhiteSpace(this.rsPassword.Get(context)) && !string.IsNullOrWhiteSpace(this.rsAccountID.Get(context)))
            {
                isAuthed = APIClient.Instance.Authenticate(this.rsUserName.Get(context), this.rsPassword.Get(context), this.rsAccountID.Get(context));
            }
            else
            {
                throw new RightScaleAPIException("Cannot authenticate without either providing an OAuth Refresh token or a username/password/accountno combination");
            }
            return isAuthed;
        }

        protected abstract override T Execute(CodeActivityContext context);
    }
}
