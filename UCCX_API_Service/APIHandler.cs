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
            cm.BeginLog();
            cm.LogMessage("Beginning WFM Agent Queue Update Process using the UCCX API.");
            cm.LogMessage("");
            
            int numFailed = 0;
            int numAgentsProcessed = 0;
            foreach (ExcelAgent excelAgent in excelData.excelAgents)
            {
                // Determine Agent URL via apiData.ResourcesData
                string agentUserId = apiData.ResourcesData.Resource.Where(p => p.FirstName + " " + p.LastName == excelAgent.agentName).First().UserID;
                string agentUrl = $"{cm.RootURL}/resource/{agentUserId}";

                // Request Agent API Info to modify for updated skillMap
                if(cm.Env == "DEV")
                {
                    eventLog.WriteEntry("Sending GET Request for Agent Resource Data (" + agentUserId + ").\nAPI Endpoint: " + agentUrl, EventLogEntryType.Information, ++eventId);
                }
                else
                {
                    eventLog.WriteEntry("Sending GET Request for Agent Resource Data (" + agentUserId + ").", EventLogEntryType.Information, ++eventId);
                }

                try
                { 
                    // Determine which Resource from APIData.ResourcesData corresponds to the current excelAgent being processed
                    Resource agentInfo = apiData.ResourcesData.Resource.Where(p => p.FirstName + " " + p.LastName == excelAgent.agentName).First();
                    // Serialize XML related to the current excelAgent being processed using Resource Object
                    XmlDocument xml = SerializeXml(agentInfo);

                    // Isolate Old skillMap Node
                    XmlNode node = xml.SelectSingleNode("/resource/skillMap");

                    // Determine Agent's desired Queue
                    ExcelSkill newQueue = excelData.excelSkills.Where(p => p.Name == excelAgent.Queue).First();
                    
                    // Build Skill Map XML to replace current using Agent's desired Queue
                    XmlDocument xmlSkillMap = new XmlDocument();
                    string skillMapString = BuildSkillMap(newQueue);

                    // Determine Action based on whether or not error occurred in BuildSkillMap()
                    if(skillMapString != "ERROR")
                    {
                        xmlSkillMap.LoadXml(skillMapString);

                        // Create new XmlNode object to replace old skillMap with
                        XmlNode newNode = xmlSkillMap.SelectSingleNode("/skillMap");

                        // Replace skillMap Node with new skillMap Node
                        node.InnerXml = newNode.InnerXml;
                
                        // Make PUT Request to update Agent Skill Map
                        try
                        {
                            // Call Method to make PUT Request to API to update Agent skillMap
                            cm.LogMessage($"Attempting to update {excelAgent.agentName} ({agentUserId}) to new Queue: {excelAgent.Queue}\n\tAgent refURL: {agentUrl}");
                            eventNum = eventId;
                            HttpWebResponse requestResponse = UpdateAgentResource(xml.OuterXml, agentUrl);
                            eventId++;
                            if(requestResponse.StatusCode != HttpStatusCode.OK)
                            {
                                //eventLog.WriteEntry($"Error - Status Code Returned: {requestResponse.StatusCode} -- {requestResponse.StatusDescription}");
                                //cm.LogMessage($"Error - Status Code Returned: {requestResponse.StatusCode} -- {requestResponse.StatusDescription}");
                                throw new System.ArgumentException($"Error - Status Code Returned: {requestResponse.StatusCode} -- {requestResponse.StatusDescription}");
                            }
                            else
                            {
                                if(requestResponse.StatusCode == HttpStatusCode.Unauthorized ||
                                   requestResponse.StatusCode == HttpStatusCode.BadRequest ||
                                   requestResponse.StatusCode == HttpStatusCode.Forbidden ||
                                   requestResponse.StatusCode == HttpStatusCode.NotFound)
                                {
                                    cm.LogMessage($"Fatal Error -- Status Code Returned: {requestResponse.StatusCode} -- {requestResponse.StatusDescription}");
                                    cm.LogMessage($"Please contact support to identify the issue.");
                                    cm.LogMessage($"The Process will not proceed.");
                                    break;
                                }
                                else
                                {
                                    cm.LogMessage($"Error - Status Code Returned: {requestResponse.StatusCode} -- {requestResponse.StatusDescription}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            numFailed += 1;
                            eventLog.WriteEntry("Invalid HttpWebResponse.StatusCode while attempting to UPDATE Agent Resource Data (" + agentUserId + ").\n" + e.Message.ToString(), EventLogEntryType.Warning, ++eventId);
                            cm.LogMessage("-->Invalid HttpWebResponse.StatusCode while attempting to UPDATE Agent Resource Data (" + agentUserId + ")");
                            cm.LogMessage("e.Message.ToString()");
                        }
                    }
                    else
                    {
                        numFailed += 1;
                    }
                    numAgentsProcessed += 1;
                }
                catch (Exception e)
                {
                    eventLog.WriteEntry($"Error Occurred while updating current user {agentUserId}.\nAgent URL Endpoint Used: {agentUrl}\nError Caught: {e.Message.ToString()}\nError Source: {e.Source.ToString()}\nError Stack Trace: {e.StackTrace.ToString()}", EventLogEntryType.Error, eventId++);
                    cm.LogMessage($"Error Occurred -- {e.Message.ToString()}");
                    cm.LogMessage($"Stack Trace: {e.StackTrace.ToString()}");
                    cm.LogMessage($"Continuing to the next agent...");
                }
                cm.LogMessage("");
            }
            cm.EndLog();
            cm.BeginLog();
            EndProcessLog(excelData.excelAgents.Count, numFailed);
            cm.EndLog();
            eventLog.WriteEntry("UCCX API Agent Queue Update has finished.", EventLogEntryType.Information, eventId++);
        }
        public XmlDocument SerializeXml<T>(T serializeObject, bool stripNamespace = true)
        {
            string xmlString = "";

            // Create our own namespaces for the output
            System.Xml.Serialization.XmlSerializerNamespaces namespaces = new System.Xml.Serialization.XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            namespaces.Add("", "");

            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(serializeObject.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(textWriter, new XmlWriterSettings { OmitXmlDeclaration = true }))
                {
                    if (stripNamespace == true)
                    {
                        new System.Xml.Serialization.XmlSerializer(serializeObject.GetType()).Serialize(writer, serializeObject, namespaces);
                    }
                    else
                    {
                        new System.Xml.Serialization.XmlSerializer(serializeObject.GetType()).Serialize(writer, serializeObject);
                    }
                }
                xmlString = textWriter.ToString();
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlString);
            return xml;
        }
        // Gets all skills associated with the input agent's Queue
        private string BuildSkillMap(ExcelSkill newQueue)
        {
            // Template skillMap XML --> Uses Replace in order to build new skillMap string
            string templateSkill = "<skillCompetency><competencelevel>COMPETENCY_LEVEL</competencelevel><skillNameUriPair name=\"SKILL_NAME\"><refURL>REF_URL</refURL></skillNameUriPair></skillCompetency>";
            string skillMap = "";
            string skillsReport = "";
            bool allSkillsFound = true;
            foreach (KeyValuePair<string, int> kvp in newQueue.SkillsAdded)
            {
                try
                {
                    // Determine Skill refUrl by querying APIData
                    Skill addSkill = apiData.SkillsData.Skill.Where(p => p.SkillName.ToUpper() == kvp.Key.ToUpper()).First();
                    //Append skillMap string with new info
                    skillMap += templateSkill.Replace("COMPETENCY_LEVEL", kvp.Value.ToString()).Replace("SKILL_NAME", addSkill.SkillName).Replace("REF_URL", addSkill.Self);

                    skillsReport += $"{kvp.Key}({kvp.Value.ToString()}), ";
                }
                catch
                {
                    // Log and output to console Error
                    cm.LogMessage($"The skill {kvp.Key} was unable to be found within the API Skills. Please check the spelling and formatting of the skill name. The skill name formatting is extremely strict.");
                    allSkillsFound = false;
                    break;
                }
            }
            if(allSkillsFound == true)
            {
                // Add XML Outer Node onto new skillMap contents prior to replacing old skillMap XML Node
                skillMap = $"<skillMap>{skillMap}</skillMap>";

                return skillMap;
            }
            else
            {
                return "ERROR";
            }
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
        public void EndProcessLog(int totalAgents, int numFailed)
        {
            cm.LogMessage($"Finished WFM Agent Queue Update Process using the UCCX API.");
            if (numFailed > 0)
            {
                cm.LogMessage($">Attempted to update {totalAgents.ToString()} Agents.");
                cm.LogMessage($">WARNING: {numFailed.ToString()}/{totalAgents.ToString()} Agents failed to update.");
                cm.LogMessage($">{(totalAgents - numFailed).ToString()}/{totalAgents.ToString()} Agents successfully updated.");
            }
            else
            {
                cm.LogMessage($">{totalAgents.ToString()} Agents successfully updated.");
            }
        }
    }
}
