﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using RightScale.netClient.Core;
using RightScale.netClient;


namespace RightScale.netClient.ActivityLibrary 
{
    public sealed class EnableServerArray : Base.RSCodeActivity
    {

        [RequiredArgument]
        public InArgument<string> serverArrayID { get; set; }

        public OutArgument<bool> isEnabled { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if (base.authClient(context))
            {
                bool retVal = ServerArray.setEnabled(this.serverArrayID.Get(context));
                this.isEnabled.Set(context, retVal);
            }
        }
    }
}
