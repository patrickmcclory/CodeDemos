using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Messaging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;

namespace WindowsFormsApplication1
{
    public partial class QueueListener : Form
    {
        SubscriptionClient client;
        string subscriptionID;
        string connectionString;

        public QueueListener()
        {
            InitializeComponent();
            initializeAppProcess();
            
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void initializeAppProcess()
        {
            connectionString = Environment.GetEnvironmentVariable("phbDemoServiceBus", EnvironmentVariableTarget.Machine);
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            subscriptionID = Guid.NewGuid().ToString();
            if (!namespaceManager.SubscriptionExists("phbalerter", subscriptionID))
            {
                namespaceManager.CreateSubscription("phbalerter", subscriptionID);
            }

            client = SubscriptionClient.CreateFromConnectionString(connectionString, "phbalerter", subscriptionID);
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            Application.ApplicationExit += Application_ApplicationExit;
            bgWorker.RunWorkerAsync();
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (namespaceManager.SubscriptionExists("phbalerter", subscriptionID))
            {
                namespaceManager.DeleteSubscription("phbalerter", subscriptionID);
            }
        }

        void appendTextBox(string message)
        {
            tbOutput.Text += Environment.NewLine + DateTime.Now.ToString() + " " + message;
            tbOutput.DeselectAll();
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.Invoke((MethodInvoker)delegate() { appendTextBox(e.Result.ToString()); });
                if (e.Result.ToString().ToLower() == "phbalarm")
                {
                    //start program here
                    this.Invoke((MethodInvoker)delegate() { MessageBox.Show("this is where things should happen"); });
                }
            }
            bgWorker.RunWorkerAsync();
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = null;
            while (!e.Cancel && e.Result == null)
            {
                this.Invoke((MethodInvoker)delegate() { appendTextBox("Listening for messages.."); });
                BrokeredMessage message = client.Receive();
                try
                {
                    if (message != null)
                    {
                        e.Result = message.GetBody<string>();
                        break;
                    }
                    message.Complete();
                }
                catch
                {
                    message.Abandon();
                }
            }
        }
    }
}
