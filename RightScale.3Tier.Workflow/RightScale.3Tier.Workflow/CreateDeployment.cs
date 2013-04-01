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
    public sealed class CreateDeployment : Base.RSNativeActivity<string>
    {
        [RequiredArgument]
        public InArgument<string> DeploymentName { get; set; }

        public InArgument<string> DeploymentDescription { get; set; }

        public InArgument<string> DeploymentTagScope { get; set; }

        public OutArgument<string> DeploymentID { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            string retVal = string.Empty;

            if (base.authClient(context))
            {
                retVal = Deployment.create(this.DeploymentName.Get(context), this.DeploymentDescription.Get(context), this.DeploymentTagScope.Get(context));
                this.DeploymentID.Set(context, retVal);
            }

            return retVal;
        }

    }
}
