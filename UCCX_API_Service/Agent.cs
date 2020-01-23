using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace UCCX_API_Service
{
    [XmlRoot(ElementName = "resourceGroup")]
    public class AgentResourceGroup
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "skillNameUriPair")]
    public class AgentSkillNameUriPair
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "skillCompetency")]
    public class AgentSkillCompetency
    {
        [XmlElement(ElementName = "competencelevel")]
        public string Competencelevel { get; set; }
        [XmlElement(ElementName = "skillNameUriPair")]
        public SkillNameUriPair SkillNameUriPair { get; set; }
    }

    [XmlRoot(ElementName = "skillMap")]
    public class AgentSkillMap
    {
        [XmlElement(ElementName = "skillCompetency")]
        public List<SkillCompetency> SkillCompetency { get; set; }
    }

    [XmlRoot(ElementName = "team")]
    public class AgentTeam
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "resource")]
    public class Agent
    {
        [XmlElement(ElementName = "self")]
        public string Self { get; set; }
        [XmlElement(ElementName = "userID")]
        public string UserID { get; set; }
        [XmlElement(ElementName = "firstName")]
        public string FirstName { get; set; }
        [XmlElement(ElementName = "lastName")]
        public string LastName { get; set; }
        [XmlElement(ElementName = "extension")]
        public string Extension { get; set; }
        [XmlElement(ElementName = "alias")]
        public string Alias { get; set; }
        [XmlElement(ElementName = "resourceGroup")]
        public ResourceGroup ResourceGroup { get; set; }
        [XmlElement(ElementName = "skillMap")]
        public SkillMap SkillMap { get; set; }
        [XmlElement(ElementName = "autoAvailable")]
        public string AutoAvailable { get; set; }
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "team")]
        public Team Team { get; set; }
        [XmlElement(ElementName = "primarySupervisorOf")]
        public string PrimarySupervisorOf { get; set; }
        [XmlElement(ElementName = "secondarySupervisorOf")]
        public string SecondarySupervisorOf { get; set; }
        public void Info()
        {
            Console.WriteLine($"{FirstName} {LastName} -- {UserID}\n\tURL: {Self}\n\t---- CURRENT SKILLS ----");
            foreach (SkillCompetency skn in SkillMap.SkillCompetency)
            {
                Console.WriteLine("\t" + skn.SkillNameUriPair.Name);
            }
            Console.WriteLine("\n");
        }
    }
}