using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UCCX_API_Service
{
    class ExcelData : APIHandler
    {
        private EventLog eventL { get; set; }
        public ExcelData(EventLog eLog)
        {
            eventLog = eLog;
        }
        public void Refresh(CredentialManager cm, ref int eventId, EventLog eLog)
        {
            eventL = eLog;
            reader = new Reader(cm.ExcelFile);
            eventL.WriteEntry("Reading Excel Queue Data.", System.Diagnostics.EventLogEntryType.Information, ++eventId);
            //UpdateConsoleStep("Reading Excel Queue Data...");
            excelSkills = reader.ReadSkillData("Queues");
            eventL.WriteEntry("Reading Excel Agent Data.", System.Diagnostics.EventLogEntryType.Information, ++eventId);
            //UpdateConsoleStep("Reading Excel Agent Data...");
            excelAgents = reader.ReadAgentData("Agents");
        }
        public Reader reader { get; set; }
        public List<ExcelAgent> excelAgents { get; set; }
        public List<ExcelSkill> excelSkills { get; set; }
        public new void Info()
        {
            Console.WriteLine("\n\n###################################################################");
            Console.WriteLine("############################ AGENT DATA #############################");
            Console.WriteLine("#####################################################################\n");
            foreach (ExcelAgent agent in excelAgents)
            {
                agent.Info();
            }
            Console.WriteLine("\n\n###################################################################");
            Console.WriteLine("######################## SKILL (EXCEL) DATA #########################");
            Console.WriteLine("#####################################################################\n");
            foreach (ExcelSkill sk in excelSkills)
            {
                sk.Info();
            }
            Console.WriteLine("\n");
        }
    }
}
