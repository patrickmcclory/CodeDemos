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

            Console.WriteLine();
            Console.WriteLine("Accept Defaults? (Y/n)");
            ConsoleKeyInfo defaultResponse = Console.ReadKey();
            if (defaultResponse.KeyChar.ToString().ToLower().StartsWith("y") || defaultResponse.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                Console.WriteLine("Accepting defaults for this deployment.");
                Console.WriteLine();
            }
            else
            {
                buildDeploymentVars();
                buildLBInputs();
                buildWindowsInputs();
                buildSQLInputs();
                buildIISInputs();
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


            Dictionary<string, object> inputs = new Dictionary<string, object>();

            List<Input> deploymentLevelInputs = new List<Input>();
            deploymentLevelInputs.AddRange(getLBInputs());
            deploymentLevelInputs.AddRange(getWindowsInputs());
            deploymentLevelInputs.AddRange(getSQLInputs());
            deploymentLevelInputs.AddRange(getIISInputs());

            inputs.Add("deploymentLevelInputs", deploymentLevelInputs);
            inputs.Add("databaseDNSNames", databaseDNSNames());

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

            inputs.Add("serverLoadBalancerID", loadBalancerServerTemplateID);
            inputs.Add("serverSQLServerID", sqlDBServerTemplateID);
            inputs.Add("serverIISServerID", iisServerTemplateID);

            inputs.Add("serverLoadBalancerCount", loadBalancerCount);
            inputs.Add("serverSQLServerCount", sqlDBServerCount);
            inputs.Add("serverIISServerMinCount", iisServerMinCount);
            inputs.Add("serverIISServerMaxCount", iisServerMaxCount);

            AlertSpecificParam asp = new AlertSpecificParam("voterTagPredicate", "81");
            Bound b = new Bound(iisServerMinCount, iisServerMaxCount);
            Pacing p = new Pacing(iisResizeUpBy, iisResizeDownBy, iisResizeCalmTime);
            ElasticityParam ep = new ElasticityParam(asp, b, p, new List<ScheduleEntry>());

            inputs.Add("iisElasticityParams", new List<ElasticityParam>() { ep });

            inputs.Add("serverCloudID", cloudID);

            Activity workflow1 = new Workflow1();

            WorkflowInvoker.Invoke(workflow1, inputs);
        }

        private static string rsEmail = string.Empty;
        private static string rsAccountNo = string.Empty;
        private static string rsPassword = string.Empty;
        private static string rsAuthKey = string.Empty;

        private static string iisResizeUpBy = "1";
        private static string iisResizeDownBy = "1";
        private static string iisResizeCalmTime = "15";

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

        private static string cloudID = "2178";
        private static string loadBalancerServerTemplateID = "292788001";
        private static string sqlDBServerTemplateID = "291393001";
        private static string iisServerTemplateID = "292787001"; //This is a head revision and needs to be modified once it's tested and published
        private static int loadBalancerCount = 2;
        private static int sqlDBServerCount = 2;
        private static int iisServerMinCount = 1;
        private static int iisServerMaxCount = 5;

        static void buildDeploymentVars()
        {
            string inputTemp = string.Empty;
            Console.WriteLine("Getting variables for server layout");
            Console.WriteLine();
            Console.Write("ID of Cloud to deploy to (2178 - Azure US West): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                cloudID = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input Load Balancer ServerTemplate ID (" + loadBalancerServerTemplateID + "): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                loadBalancerServerTemplateID = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input Load Balancer count (2): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                loadBalancerCount = int.Parse(inputTemp);
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input SQL DB ServerTemplate ID (" + sqlDBServerTemplateID + "): ");
            inputTemp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDBServerTemplateID = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input SQL DB Count (2): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDBServerCount = int.Parse(inputTemp);
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input IIS ServerTemplate ID (" + iisServerTemplateID + "): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisServerTemplateID = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input IIS Server min count (1): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisServerMinCount = int.Parse(inputTemp);
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Input IIS Server max count (5): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisServerMaxCount = int.Parse(inputTemp);
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Resize up by (1): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisResizeUpBy = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Resize down by (1): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisResizeDownBy = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write("Resize calm time (15): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisResizeCalmTime = inputTemp;
            }
        }

        private static string lbPools = "mileagestats";
        private static string lbHealthCheckURI = "/";
        private static string lbStatsPassword = "P@ssword1";
        private static string lbStatsUser = "statsUser";
        private static string lbStatsUri = "/haproxy-mileagestats";
        private static string rightscaleTimezone = "UTC";

        static void buildLBInputs()
        {
            string inputTemp = string.Empty;
            Console.WriteLine("Getting Deployment-level variables for Load Balancer Inputs");
            Console.WriteLine();
            Console.Write(@"lb\pools input value (" + lbPools + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                lbPools = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"lb\health_check_uri input value (" + lbHealthCheckURI + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                lbHealthCheckURI = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"lb\stats_password input value (" + lbStatsPassword + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                lbStatsPassword = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"lb\stats_uri input value (" + lbStatsUri + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                lbStatsUri = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"rightscale\timezone input value (" + rightscaleTimezone + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                rightscaleTimezone = inputTemp;
            }
        }

        static List<Input> getLBInputs()
        {
            List<Input> retVal = new List<Input>();
            retVal.Add(new Input(@"lb\pools", formatInput(lbPools)));
            retVal.Add(new Input(@"lb\health_check_uri", formatInput(lbHealthCheckURI)));
            retVal.Add(new Input(@"lb\stats_password", formatInput(lbStatsPassword)));
            retVal.Add(new Input(@"lb\stats_user", formatInput(lbStatsUser)));
            retVal.Add(new Input(@"lb\stats_uri", formatInput(lbStatsUri)));
            return retVal;
        }

        private static string sqlBackupMethod = "Remote Storage";
        private static string sqlDataVolumeSize = "10";
        private static string sqlDBLineageName = "windowsWorkflowFoundation3Tier";
        private static string sqlLogsVolumeSize = "10";
        private static string sqlDNSUser = "cred:DME_USER";
        private static string sqlDNSPassword = "cred:DME_USER_PASSWORD";
        private static string sqlDNSService = "DNS Made Easy";
        private static string sqlRemoteStorageAccountID = "cred:azureStorage_devTest_AccountName";
        private static string sqlRemoteStorageAccountProvider = "Windows Azure Storage";
        private static string sqlRemoteStorageAccountSecret = "cred:azureStorage_devTest_AccountKey";
        private static string sqlRemoteStorageContainer = "media";
        private static string sqlBackupFileName = "mileagestatsdata_sql2012.bak";
        private static string sqlDatabaseName = "MileageStatsData";
        private static string sqlServerMode = "Standalone";

        static void buildSQLInputs()
        {
            string inputTemp = string.Empty;
            Console.WriteLine("Getting variables for Deployment-level SQL Server Inputs");
            Console.WriteLine();
            Console.Write(@"BACKUP_METHOD input value (" + sqlBackupMethod + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlBackupMethod = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"DATA_VOLUME_SIZE input value (" + sqlDataVolumeSize + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDataVolumeSize = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"DB_LINEAGE_NAME input value (" + sqlDBLineageName + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDBLineageName = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"LOGS_VOLUME_SIZE input value (" + sqlLogsVolumeSize + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlLogsVolumeSize = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"DNS_USER input value (" + sqlDNSUser + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDNSUser = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"DNS_PASSWORD input value (" + sqlDNSPassword + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDNSPassword = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"DNS_SERVICE input value (" + sqlDNSService + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDNSService = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_ACCOUNT_ID input value (" + sqlRemoteStorageAccountID + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlRemoteStorageAccountID = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_ACCOUNT_PROVIDER input value (" + sqlRemoteStorageAccountProvider + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlRemoteStorageAccountProvider = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_ACCOUNT_SECRET input value (" + sqlRemoteStorageAccountSecret + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlRemoteStorageAccountSecret = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_CONTAINER input value (" + sqlRemoteStorageContainer + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlRemoteStorageContainer = inputTemp;
            }
            inputTemp = string.Empty;
            Console.Write(@"DB_NAME input value (" + sqlDatabaseName + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                sqlDatabaseName = inputTemp;
            }
            inputTemp = string.Empty;
        }

        static List<Input> getSQLInputs()
        {
            List<Input> retVal = new List<Input>();

            retVal.Add(new Input("BACKUP_METHOD", formatInput(sqlBackupMethod)));
            retVal.Add(new Input("DATA_VOLUME_SIZE", formatInput(sqlDataVolumeSize)));
            retVal.Add(new Input("DB_LINEAGE_NAME", formatInput(sqlDBLineageName)));
            retVal.Add(new Input("LOGS_VOLUME_SIZE", formatInput(sqlLogsVolumeSize)));
            retVal.Add(new Input("DNS_USER", formatInput(sqlDNSUser)));
            retVal.Add(new Input("DNS_PASSWORD", formatInput(sqlDNSPassword)));
            retVal.Add(new Input("DNS_SERVICE", formatInput(sqlDNSService)));
            retVal.Add(new Input("REMOTE_STORAGE_ACCOUNT_ID", formatInput(sqlRemoteStorageAccountID)));
            retVal.Add(new Input("REMOTE_STORAGE_ACCOUNT_PROVIDER", formatInput(sqlRemoteStorageAccountProvider)));
            retVal.Add(new Input("REMOTE_STORAGE_ACCOUNT_SECRET", formatInput(sqlRemoteStorageAccountSecret)));
            retVal.Add(new Input("REMOTE_STORAGE_CONTAINER", formatInput(sqlRemoteStorageContainer)));
            retVal.Add(new Input("BACKUP_FILE_NAME", formatInput(sqlBackupFileName)));
            retVal.Add(new Input("DB_NAME", formatInput(sqlDatabaseName)));
            retVal.Add(new Input("SERVER_MODE", formatInput(sqlServerMode)));

            return retVal;
        }

        private static string windowsAdminPassword = "P@ssword1";
        private static string windowsTimezoneInfo = "GMT Standard Time";

        static void buildWindowsInputs()
        {
            string inputTemp = string.Empty;
            Console.WriteLine("Getting variables for Deployment-level generic Windows Inputs");
            Console.WriteLine();
            Console.Write(@"ADMIN_PASSWORD input value (" + windowsAdminPassword + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                windowsAdminPassword = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"SYS_WINDOWS_TZINFO input value (" + windowsTimezoneInfo + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                windowsTimezoneInfo = inputTemp;
            }
        }

        static List<Input> getWindowsInputs()
        {
            List<Input> retVal = new List<Input>();

            retVal.Add(new Input("ADMIN_PASSWORD", formatInput(windowsAdminPassword)));
            retVal.Add(new Input("SYS_WINDOWS_TZINFO", formatInput(windowsTimezoneInfo)));

            return retVal;
        }

        private static string iisLBVHostName = "mileagestats";
        private static string iisZipFileName = "Build_20130318050444.zip";
        private static string iisRemoteStorageAccountIDApp = "cred:azureStorage_devTest_AccountName";
        private static string iisRemoteStorageAccountSecretApp = "cred:azureStorage_devTest_AccountKey";
        private static string iisRemoteStorageAccountProviderApp = "Windows Azure Storage";
        private static string iisRemoteStorageContainerApp = "media";

        static void buildIISInputs()
        {
            string inputTemp = string.Empty;
            Console.WriteLine("Getting variables for Deployment-level IIS Inputs");
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_ACCOUNT_ID_APP input value (" + iisRemoteStorageAccountIDApp + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisRemoteStorageAccountIDApp = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_ACCOUNT_SECRET_APP input value (" + iisRemoteStorageAccountSecretApp + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisRemoteStorageAccountSecretApp = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_ACCOUNT_PROVIDER_APP input value (" + iisRemoteStorageAccountProviderApp + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisRemoteStorageAccountProviderApp = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"REMOTE_STORAGE_CONTAINER_APP input value (" + iisRemoteStorageContainerApp + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisRemoteStorageContainerApp = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"ZIP_FILE_NAME input value (" + iisZipFileName + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisZipFileName = inputTemp;
            }
            inputTemp = string.Empty;
            Console.WriteLine();
            Console.Write(@"LB_VHOST_NAME input value (" + iisLBVHostName + " ): ");
            inputTemp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputTemp))
            {
                iisRemoteStorageAccountIDApp = inputTemp;
            }
            inputTemp = string.Empty;
        }

        static List<Input> getIISInputs()
        {
            List<Input> retVal = new List<Input>();

            retVal.Add(new Input("REMOTE_STORAGE_ACCOUNT_ID_APP", formatInput(iisRemoteStorageAccountIDApp)));
            retVal.Add(new Input("REMOTE_STORAGE_ACCOUNT_SECRET_APP", formatInput(iisRemoteStorageAccountSecretApp)));
            retVal.Add(new Input("REMOTE_STORAGE_ACCOUNT_PROVIDER_APP", formatInput(iisRemoteStorageAccountProviderApp)));
            retVal.Add(new Input("REMOTE_STORAGE_CONTAINER_APP", formatInput(iisRemoteStorageContainerApp)));
            retVal.Add(new Input("ZIP_FILE_NAME", formatInput(iisZipFileName)));
            retVal.Add(new Input("LB_VHOST_NAME", formatInput(iisLBVHostName)));

            return retVal;
        }

        static List<KeyValuePair<string, string>> databaseDNSNames()
        {
            List<KeyValuePair<string, string>> retVal = new List<KeyValuePair<string, string>>();
            retVal.Add(new KeyValuePair<string, string>("winworkflowdb1.cloudlord.com", "10244681"));
            retVal.Add(new KeyValuePair<string, string>("winworkflowdb2.cloudlord.com", "10244682"));
            return retVal;
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
