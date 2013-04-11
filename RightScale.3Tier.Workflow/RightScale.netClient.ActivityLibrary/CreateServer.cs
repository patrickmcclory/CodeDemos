using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using RightScale.netClient.Core;
using RightScale.netClient;

namespace RightScale.netClient.ActivityLibrary
{
    public sealed class CreateServer : Base.ServerBasedCreateActivity
    {
        [RequiredArgument]
        public InArgument<Int32> numberOfServers { get; set; }

        public OutArgument<List<string>> serverIDs { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string retVal = string.Empty;

            List<string> serverIDlist = new System.Collections.Generic.List<string>();

            if (this.numberOfServers.Get(context) < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfServers must be greater than 0 to provision servers through this process.  Please check your inputs and try again.");
            }
            
            if (base.authClient(context))
            {
                if (this.numberOfServers.Get(context) > 1)
                {
                    for (int i = 0; i < this.numberOfServers.Get(context); i++)
                    {
                        string desc = this.description.Get(context);
                        if (string.IsNullOrWhiteSpace(desc))
                        {
                            desc = getDefaultDescription();
                        }
                        string srvName = this.name.Get(context) + " [" + (i + 1).ToString() + " of " + this.numberOfServers.Get(context) + "]";
                        string srvID = Server.create(this.cloudID.Get(context), this.deploymentID.Get(context), this.serverTemplateID.Get(context), srvName, desc, this.cloudID.Get(context), this.description.Get(context), this.imageID.Get(context), this.inputs.Get(context), this.instanceTypeID.Get(context), this.kernelImageID.Get(context), this.multiCloudImageID.Get(context), this.ramdiskImageID.Get(context), this.securityGroupIDs.Get(context), this.sshKeyID.Get(context), this.userData.Get(context), this.optimized.Get(context));
                        serverIDlist.Add(srvID);
                    }
                }
                else
                {
                    string desc = this.description.Get(context);
                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        desc = getDefaultDescription();
                    }
                    string srvID = Server.create(this.cloudID.Get(context), this.deploymentID.Get(context), this.serverTemplateID.Get(context), this.name.Get(context), desc, this.cloudID.Get(context), this.description.Get(context), this.imageID.Get(context), this.inputs.Get(context), this.instanceTypeID.Get(context), this.kernelImageID.Get(context), this.multiCloudImageID.Get(context), this.ramdiskImageID.Get(context), this.securityGroupIDs.Get(context), this.sshKeyID.Get(context), this.userData.Get(context), this.optimized.Get(context));
                    serverIDlist.Add(srvID);
                }
            }
            this.serverIDs.Set(context, serverIDlist);
        }

        private string getDefaultDescription()
        {
            return "Server created by Windows 3 Tier Workflow project based on RightScale.netClient library at " + DateTime.Now.ToString();
        }
    }
}
