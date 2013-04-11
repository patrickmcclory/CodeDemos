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
    public sealed class UpdateInputs : Base.RSCodeActivity
    {
        public InArgument<string> serverID { get; set; }

        public InArgument<List<Input>> inputs { get; set; }
        
        public OutArgument<bool> isUpdated { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            isUpdated.Set(context, false);
            if (base.authClient(context))
            {
                Instance nextInstance = Server.show(this.serverID.Get(context)).nextInstance;
                bool updated = Input.multi_update_instance(nextInstance.cloud.ID, nextInstance.ID, inputs.Get(context));
                isUpdated.Set(context, updated);
            }
        }
    }
}
