using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Xml;
using System.Diagnostics;

namespace UCCX_API_Service
{
    class APIHandler
    {
        public APIData apiData { get; set; }
        public CredentialManager cm { get; set; }
        public EventLog eventLog { get; set; }
        public int eventNum { get; set; }
        public void Init()
        {
            //Console.WriteLine("########################################################################################");
            //Console.WriteLine("###################### WORKFORCE MANAGEMENT QUEUE UPDATE API TOOL ######################");
            //Console.WriteLine("########################################################################################\n\n");
            //UpdateConsoleStep("Entering Init State...");
            //this.cm = new CredentialManager();
            //this.apiData = new APIData(cm);
        }
        public void Refresh(ref int eventId)
        {
            eventNum = eventId;
            eventLog.WriteEntry($"Refreshing Config Parameters.", EventLogEntryType.Information, ++eventNum);
            cm = new CredentialManager();
            eventLog.WriteEntry($"Refreshing API Data.", EventLogEntryType.Information, ++eventNum);
            if(apiData != null)
            {
                int tempLog = eventNum;
                apiData.Refresh(cm, eventLog, ref tempLog);
                eventNum = tempLog;
            }
            else
            {
                apiData = new APIData(cm);
            }
            eventLog.WriteEntry($"Environment: {cm.Env}\nRoot URL: {cm.RootURL}\nUsername: {cm.Username}\nPassword: {cm.Password.Substring(0, cm.Password.Length / 5)}\nExcel File: {cm.ExcelFile}", EventLogEntryType.Information, ++eventNum);
            eventId = eventNum;
        }
        public void SetEventLog(EventLog eLog)
        {
            eventLog = eLog;
        }
        public void ExcelQueueUpdate(ExcelData excelData, ref int eventId)
        {
            //eventNum = eventId;
            //UpdateConsoleStep("Updating Agent Queues via UCCX API...");
            int numAgentsProcessed = 0;
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
            //UpdateConsoleStep("\t>Agents Processed: " + numAgentsProcessed.ToString() + "/" + excelData.excelAgents.Count.ToString());
            foreach (ExcelAgent excelAgent in excelData.excelAgents)
            {
                cm.BeginLog();
                // Determine Agent URL via apiData.ResourcesData
                string agentUserId = apiData.ResourcesData.Resource.Where(p => p.FirstName + " " + p.LastName == excelAgent.agentName).First().UserID;
                string agentUrl = $"{cm.RootURL}/resource/{agentUserId}";
                //// DEBUG -- Prints the built Agent URL for verification ###############################################
                //Console.WriteLine(agentUrl);
                ////#####################################################################################################


                // Request Agent API Info to modify for updated skillMap
                eventLog.WriteEntry("Sending GET Request for Agent Resource Data (" + agentUserId + ").", EventLogEntryType.Information, ++eventId);
                WebRequest apiRequest = WebRequest.Create(agentUrl);
                string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(cm.Username + ":" + cm.Password));
                apiRequest.Headers.Add("Authorization", "Basic " + encoded);
                HttpWebResponse apiResponse = (HttpWebResponse)apiRequest.GetResponse();
                string xmlOutput;
                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(apiResponse.GetResponseStream()))
                        xmlOutput = sr.ReadToEnd();
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(xmlOutput);
                    // Isolate Old skillMap Node
                    XmlNode node = xml.SelectSingleNode("/resource/skillMap");

                    // Determine Agent's desired Queue
                    ExcelSkill newQueue = excelData.excelSkills.Where(p => p.Name == excelAgent.Queue).First();
                    // Build Skill Map XML to replace current using Agent's desired Queue
                    XmlDocument xmlSkillMap = new XmlDocument();
                    xmlSkillMap.LoadXml(BuildSkillMap(newQueue));

                    // Create new XmlNode object to replace old skillMap with
                    XmlNode newNode = xmlSkillMap.SelectSingleNode("/skillMap");
                    //// DEBUG -- Prints the updated Skill Map XML ##########################################################
                    //Console.WriteLine("########################### NEW SKILL MAP ###########################\n" + node.OuterXml + "\n\n");
                    ////#####################################################################################################

                    // Replace skillMap Node with new skillMap Node
                    node.InnerXml = newNode.InnerXml;
                    //// DEBUG -- Removes all skillMap InnerXml Nodes, comment in to reset user skillMaps for testing #######
                    //node.InnerXml = "";
                    ////#####################################################################################################
                    
                    
                    try
                    {
                        // Call Method to make PUT Request to API to update Agent skillMap
                        cm.LogMessage($"Attempting to update {excelAgent.agentName} ({agentUserId}) to new Queue: {excelAgent.Queue}\n\tAgent refURL: {agentUrl}");
                        HttpWebResponse requestResponse = UpdateAgentResource(xml.OuterXml, agentUrl);
                        cm.LogMessage($"Status Code Returned: {requestResponse.StatusCode} -- {requestResponse.StatusDescription}\n");
                        //// DEBUG -- Prints HttpWebResponse from PUT Request ###################################################
                        //Console.WriteLine($"{requestResponse.StatusCode}: {requestResponse.StatusDescription}");
                        ////#####################################################################################################                    
                    }
                    catch (Exception e)
                    {
                        eventLog.WriteEntry("Invalid HttpWebResponse.StatusCode while attempting to UPDATE Agent Resource Data (" + agentUserId + ").\n" + e.Message.ToString(), EventLogEntryType.Warning, ++eventId);
                        cm.LogMessage("-->Invalid HttpWebResponse.StatusCode while attempting to UPDATE Agent Resource Data (" + agentUserId + ")");
                        cm.LogMessage("ERROR DESCRIPTIONS: " + e.Message.ToString());
                        // HANDLE ERROR/BAD RESPONSE
                    }
                    numAgentsProcessed += 1;
                    //Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    //UpdateConsoleStep("\t>Agents Processed: " + numAgentsProcessed.ToString() + "/" + excelData.excelAgents.Count.ToString());
                }
                else
                {
                    eventLog.WriteEntry("Invalid HttpWebResponse.StatusCode while attempting to RETRIEVE Agent Resource Data (" + agentUserId + ").\n" + apiResponse.StatusCode.ToString() + ": " + apiResponse.StatusDescription, EventLogEntryType.Warning, ++eventId);
                    cm.LogMessage("-->Invalid HttpWebResponse.StatusCode while attempting to RETRIEVE Agent Resource Data (" + agentUserId + ")");
                    cm.LogMessage("ERROR DESCRIPTIONS: " + apiResponse.StatusCode.ToString() + ": " + apiResponse.StatusDescription);
                }
                cm.EndLog();
            }
            //eventId = eventNum;
            //UpdateConsoleStep("");
            //Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            //currentLineCursor = Console.CursorTop;
            //Console.Write(new string(' ', Console.WindowWidth));
            //Console.SetCursorPosition(0, Console.CursorTop);
            //UpdateConsoleStep("Process Finished...");
            //Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
            //UpdateConsoleStep("\t>Updated " + excelData.excelAgents.Count.ToString() + " Agent Queues.\n\n");
        }

        // Gets all skills associated with the input agent's Queue
        private string BuildSkillMap(ExcelSkill newQueue)
        {
            // Template skillMap XML --> Uses Replace in order to build new skillMap string
            string templateSkill = "<skillCompetency><competencelevel>COMPETENCY_LEVEL</competencelevel><skillNameUriPair name=\"SKILL_NAME\"><refURL>REF_URL</refURL></skillNameUriPair></skillCompetency>";
            //// DEBUG -- Prints the Queue Name and all associated Skills/Competency Levels #########################
            //Console.WriteLine(newQueue.Name);
            //foreach(KeyValuePair<string, int> kvp in newQueue.SkillsAdded)
            //{
            //    //Console.WriteLine("   >Adding " + kvp.Key + ": " + kvp.Value);
            //    //Console.WriteLine(templateSkill.Replace("COMPETENCY_LEVEL", kvp.Value.ToString()).Replace("SKILL_NAME", kvp.Key).Replace("REF_URL",addSkill.Self) + "\n\n");
            //}
            ////#####################################################################################################
            string skillMap = "";
            foreach (KeyValuePair<string, int> kvp in newQueue.SkillsAdded)
            {
                // Determine Skill refUrl by querying APIData
                Skill addSkill = apiData.SkillsData.Skill.Where(p => p.SkillName == kvp.Key).First();

                //Append skillMap string with new info
                skillMap += templateSkill.Replace("COMPETENCY_LEVEL", kvp.Value.ToString()).Replace("SKILL_NAME", kvp.Key).Replace("REF_URL", addSkill.Self);
            }
            // Add XML Outer Node onto new skillMap contents prior to replacing old skillMap XML Node
            skillMap = $"<skillMap>{skillMap}</skillMap>";

            return skillMap;
        }
        private HttpWebResponse UpdateAgentResource(string requestXml, string agentUrl)
        {
            eventLog.WriteEntry("Sending PUT Request to update Agent Resource Data (" + agentUrl.Substring(agentUrl.LastIndexOf("/") + 1) + ").", EventLogEntryType.Information, ++eventNum);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(agentUrl);

            // Add Basic Authorization Headers
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(cm.Username + ":" + cm.Password));
            request.Headers.Add("Authorization", "Basic " + encoded);

            // Add Standard Encoding (Do not add into Request Body Header)
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;

            // Method is PUT, not POST
            request.Method = "PUT";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();

            // return reponse to action in previous scope
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            return response;
        }
        public void Info()
        {
            //Console.WriteLine("\n\n###################################################################");
            //Console.WriteLine("###################### CREDENTIAL MANAGER DATA ######################");
            //Console.WriteLine("#####################################################################\n");
            cm.Info();

            //Console.WriteLine("\n\n###################################################################");
            //Console.WriteLine("############################# API DATA ##############################");
            //Console.WriteLine("#####################################################################\n");
            apiData.Info();
        }
        public void UpdateConsoleStep(string message)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(message);
        }
    }
}
