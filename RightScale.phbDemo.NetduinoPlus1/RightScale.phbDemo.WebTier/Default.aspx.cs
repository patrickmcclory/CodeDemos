using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ServiceBus;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;

namespace RightScale.phbDemo.WebTier
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString == null || string.IsNullOrWhiteSpace(Request.QueryString.Get("message")))
            {
                Response.Write("nope");
                Response.End();
            }
            else
            {
                string connectionString = Environment.GetEnvironmentVariable("phbDemoServiceBus", EnvironmentVariableTarget.Machine);
                TopicClient client = TopicClient.CreateFromConnectionString(connectionString, "phbalerter");
                client.Send(new BrokeredMessage(Request.QueryString.Get("message")));
                Response.Write("yep");
                Response.End();
            }
        }
    }
}