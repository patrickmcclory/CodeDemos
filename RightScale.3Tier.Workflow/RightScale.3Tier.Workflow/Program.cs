using System;
using System.Linq;
using System.Activities;
using System.Activities.Statements;
using RightScale.netClient;
using RightScale.netClient.Core;
using System.Collections.Generic;
using System.Collections;

namespace RightScale._3Tier.Workflow
{

    class Program
    {
        static void Main(string[] args)
        {
            string rsEmail = string.Empty;
            string rsAccountNo = string.Empty;
            string rsPassword = string.Empty;

            Console.WriteLine("Enter RightScale API Token (leave blank if you'd rather authenticate with Username, Account ID and Password");
            string rsAuthKey = Console.ReadLine();

            bool isAuthenticated = false;

            if (!string.IsNullOrWhiteSpace(rsAuthKey))
            {
                Console.WriteLine("  Attempting to authenticate with API Token");
                isAuthenticated = APIClient.Instance.Authenticate(rsAuthKey);
            }
            else
            {
                Console.WriteLine("Enter RightScale User Name:");
                rsEmail = Console.ReadLine();
                Console.WriteLine("Enter RightScale Password:");
                ConsoleKeyInfo key;
                rsPassword = string.Empty;
                do
                {
                    key = Console.ReadKey(true);

                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        rsPassword += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && rsPassword.Length > 0)
                        {
                            rsPassword = rsPassword.Substring(0, (rsPassword.Length - 1));
                            Console.Write("\b \b");
                        }
                    }
                }
                while (key.Key != ConsoleKey.Enter);
                Console.WriteLine();

                Console.WriteLine("Enter RightScale Account No:");
                rsAccountNo = Console.ReadLine();

                Console.WriteLine("  Attempting to authenticate with user name, password and account number");
                isAuthenticated = APIClient.Instance.Authenticate(rsEmail, rsPassword, rsAccountNo);
            }

            if (isAuthenticated)
            {
                Console.WriteLine("Successfully authenticated to the RightScale API");
            }

            Console.WriteLine("Ready to Proceed? Y/n");

            string readyResponse = Console.ReadLine();

            if (!readyResponse.ToLower().StartsWith("y"))
            {
                return;
            }

            Dictionary<string, object> inputs = new Dictionary<string, object>();
            inputs.Add("deploymentName", "Demo Deployment");
            inputs.Add("deploymentDescription", "Created via Windows Workflow Foundation and RightScale.netClient at " + DateTime.Now.ToString());
            inputs.Add("deploymentTagScope", "deployment");
            if(!string.IsNullOrWhiteSpace(rsAuthKey))
            {
                inputs.Add("rsOauthRefreshToken", rsAuthKey);
            }
            else if (!string.IsNullOrWhiteSpace(rsEmail) && !string.IsNullOrWhiteSpace(rsAccountNo) && !string.IsNullOrWhiteSpace(rsPassword))
            {
                inputs.Add("rsUsername", rsEmail);
                inputs.Add("rsPassword", rsPassword);
                inputs.Add("rsAccountNo", rsAccountNo);
            }
            
            Activity workflow1 = new Workflow1();

            WorkflowInvoker.Invoke(workflow1, inputs);
        }
    }
}
