using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Activities;
using RightScale.netClient.Core;
using RightScale.netClient;

namespace RightScale.netClient.ActivityLibrary
{
    public sealed class RunRightScript : Base.RSCodeActivity
    {
        [RequiredArgument]
        public InArgument<string> serverID { get; set; }

        [RequiredArgument]
        public InArgument<string> scriptOrIDString { get; set; }

        public InArgument<bool> ignoreLock { get; set; }

        public InArgument<List<Input>> inputs { get; set; }

        public OutArgument<string> taskID { get; set; }

        protected override void Execute(System.Activities.CodeActivityContext context)
        {
            if (base.authClient(context))
            {
                string rightScriptName = string.Empty;
                string rightScriptID = string.Empty;

                if (IsDigitsOnly(this.scriptOrIDString.Get(context)))
                {
                    rightScriptID = this.scriptOrIDString.Get(context);
                }
                else
                {
                    rightScriptName = this.scriptOrIDString.Get(context);
                }

                Server currentServer = Server.show(serverID.Get(context));
                Task executableRun = Instance.run_executable(currentServer.currentInstance.cloud.ID, currentServer.currentInstance.ID, rightScriptName, rightScriptID, inputs.Get(context), ignoreLock.Get(context));
                this.taskID.Set(context, executableRun.ID);
            }
        }

        static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

    }
}
