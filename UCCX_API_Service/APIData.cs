using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace UCCX_API_Service
{
    class APIData : APIHandler
    {
        public Resources ResourcesData { get; set; }
        public Skills SkillsData { get; set; }
        public APIData(CredentialManager cm)
        {
            // Deserialize Agent Data from API
            //UpdateConsoleStep("Fetching Resource Data from UCCX API...");
            ResourcesData = ApiWebRequestHelper.GetXmlRequest<Resources>("/resource", cm);
            // Deserialize Skills Data from API
            //UpdateConsoleStep("Fetching Skill Data from UCCX API...");
            SkillsData = ApiWebRequestHelper.GetXmlRequest<Skills>("/skill", cm);
        }
        public void Refresh(CredentialManager cm, EventLog eventLog, ref int eventId)
        {
            eventLog.WriteEntry("Requesting Agent Data via UCCX API.", EventLogEntryType.Information, ++eventId);
            ResourcesData = ApiWebRequestHelper.GetXmlRequest<Resources>("/resource", cm);
            eventLog.WriteEntry("Requesting Skill Data via UCCX API.", EventLogEntryType.Information, ++eventId);
            SkillsData = ApiWebRequestHelper.GetXmlRequest<Skills>("/skill", cm);
        }
        public new void Info()
        {
            Console.WriteLine("\n\n###################################################################");
            Console.WriteLine("########################## RESOURCE DATA ##########################");
            Console.WriteLine("###################################################################\n");
            foreach (Resource rs in ResourcesData.Resource)
            {
                Console.WriteLine($"{rs.FirstName} {rs.LastName} ({rs.UserID}) -- {rs.Extension}\n\trefURL: {rs.Self}");
            }
            Console.WriteLine("\n\n###################################################################");
            Console.WriteLine("########################### SKILLS DATA ###########################");
            Console.WriteLine("###################################################################\n");
            foreach (Skill sk in SkillsData.Skill)
            {
                Console.WriteLine($"[{sk.SkillId}] {sk.SkillName}\n\trefURL: {sk.Self}");
            }
            Console.WriteLine("\n");

        }
    }
}
