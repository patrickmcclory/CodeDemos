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
            bool isAuthenticated = authenticateRS();

            while (!isAuthenticated)
            {
                Console.WriteLine("Failed to authenticate to RightScale API - try again? (Y/n)");
                string retry = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(retry) || retry.ToLower().StartsWith("y"))
                {
                    isAuthenticated = authenticateRS();
                }
                else
                {
                    Console.WriteLine("Exiting process.  Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
            }

            if (isAuthenticated)
            {
                Console.WriteLine("Successfully authenticated to the RightScale API");
            }
            else
            {
                Console.WriteLine("Cannot authenticate to RS API - Press any key to continue...");
                Console.ReadKey();
                return;
            }

            string manifestLocation = @"Manifests/Windows3Tier.manifest.json";

            Console.WriteLine();
            Console.WriteLine("Use default manifest (" + manifestLocation + ") (Y/n)");

            ConsoleKeyInfo defaultResponse = Console.ReadKey();
            if (defaultResponse.KeyChar.ToString().ToLower().StartsWith("y") || defaultResponse.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                Console.WriteLine("Reading Default Manifest.");
                Console.WriteLine();
            }
            else
            {
                bool fileExists = false;
                while (!fileExists)
                {
                    Console.WriteLine();
                    Console.WriteLine("Input manifest path:");
                    manifestLocation = Console.ReadLine();
                    if (System.IO.File.Exists(manifestLocation))
                    {
                        fileExists = true;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("File " + manifestLocation + " does not exist or is inaccessible--enter another path? (Y/n)");
                        ConsoleKeyInfo retryResponse = Console.ReadKey();
                        if (retryResponse.KeyChar.ToString().ToLower() != "y" && retryResponse.Key != ConsoleKey.Enter)
                        {
                            Console.WriteLine("Exiting per request... press any key...");
                            Console.ReadKey();
                            return;
                        }
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Ready to Proceed? (Y/n)");

            ConsoleKeyInfo readyResponse = Console.ReadKey();

            if (readyResponse.KeyChar.ToString().ToLower() != "y" && readyResponse.Key != ConsoleKey.Enter)
            {
                Console.WriteLine("Exiting per request... press any key...");
                Console.ReadKey();
                return;
            }

            string manifestContents = string.Empty;

            using (System.IO.StreamReader sr = new System.IO.StreamReader(manifestLocation))
            {
                manifestContents = sr.ReadToEnd();
            }

            Newtonsoft.Json.JsonSerializerSettings jsettings = new Newtonsoft.Json.JsonSerializerSettings();
            jsettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            jsettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;

            Dictionary<string, object> inputs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(manifestContents, jsettings);

            if (!string.IsNullOrWhiteSpace(rsAuthKey))
            {
                inputs.Add("rsOauthRefreshToken", rsAuthKey);
            }
            else if (!string.IsNullOrWhiteSpace(rsEmail) && !string.IsNullOrWhiteSpace(rsAccountNo) && !string.IsNullOrWhiteSpace(rsPassword))
            {
                inputs.Add("rsUsername", rsEmail);
                inputs.Add("rsPassword", rsPassword);
                inputs.Add("rsAccountNo", rsAccountNo);
            }

            //add a default deployment description if it doesn't exist
            if (!inputs.ContainsKey("deploymentDescription"))
            {
                inputs.Add("deploymentDescription", "Created via Windows Workflow Foundation and RightScale.netClient at " + DateTime.Now.ToString());
            }

            Activity workflow1 = new Windows3TierWorkflow();

            WorkflowInvoker.Invoke(workflow1, inputs);
        }

        private static string rsEmail = string.Empty;
        private static string rsAccountNo = string.Empty;
        private static string rsPassword = string.Empty;
        private static string rsAuthKey = string.Empty;

        static bool authenticateRS()
        {
            Console.WriteLine("Enter RightScale API Token (leave blank if you'd rather authenticate with Username, Account ID and Password");
            rsAuthKey = Console.ReadLine();

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
            return isAuthenticated;
        }
        
        private static string formatInput(string inputVal)
        {
            if (inputVal.StartsWith("cred:") || inputVal.StartsWith("text:") || inputVal.StartsWith("env:") || inputVal.StartsWith("key:"))
            {
                return inputVal;
            }
            else
            {
                return "text:" + inputVal;
            }
        }
    }
}
