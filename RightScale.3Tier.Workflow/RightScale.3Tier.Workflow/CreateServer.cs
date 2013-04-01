using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using RightScale.netClient.Core;
using RightScale.netClient;

namespace RightScale._3Tier.Workflow
{
    public sealed class CreateServer : Base.RSNativeActivity<string>
    {
        public InArgument<string> ServerTemplateID { get; set; }

        public InArgument<string> ServerName { get; set; }

        public InArgument<string> DeploymentID { get; set; }

        public InArgument<string> CloudID { get; set; }

        public OutArgument<string> ServerID { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            string retVal = string.Empty;

            if (base.authClient(context))
            {
                retVal = Server.create_deployment(this.DeploymentID.Get(context), this.CloudID.Get(context), this.ServerTemplateID.Get(context), this.ServerName.Get(context));
                this.ServerID.Set(context, retVal);
            }

            return retVal;
        }
    }
}
