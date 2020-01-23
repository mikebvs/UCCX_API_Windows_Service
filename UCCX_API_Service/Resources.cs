using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace UCCX_API_Service
{
    [XmlRoot(ElementName = "team")]
    public class Team
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "resource")]
    public class Resource
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
        [XmlElement(ElementName = "autoAvailable")]
        public string AutoAvailable { get; set; }
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "team")]
        public Team Team { get; set; }
        [XmlElement(ElementName = "resourceGroup")]
        public ResourceGroup ResourceGroup { get; set; }
        [XmlElement(ElementName = "skillMap")]
        public SkillMap SkillMap { get; set; }
        [XmlElement(ElementName = "secondarySupervisorOf")]
        public SecondarySupervisorOf SecondarySupervisorOf { get; set; }
        [XmlElement(ElementName = "alias")]
        public string Alias { get; set; }
        [XmlElement(ElementName = "primarySupervisorOf")]
        public PrimarySupervisorOf PrimarySupervisorOf { get; set; }
    }

    [XmlRoot(ElementName = "resourceGroup")]
    public class ResourceGroup
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "skillNameUriPair")]
    public class SkillNameUriPair
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "skillCompetency")]
    public class SkillCompetency
    {
        [XmlElement(ElementName = "competencelevel")]
        public string Competencelevel { get; set; }
        [XmlElement(ElementName = "skillNameUriPair")]
        public SkillNameUriPair SkillNameUriPair { get; set; }
    }

    [XmlRoot(ElementName = "skillMap")]
    public class SkillMap
    {
        [XmlElement(ElementName = "skillCompetency")]
        public List<SkillCompetency> SkillCompetency { get; set; }
    }

    [XmlRoot(ElementName = "supervisorOfTeamName")]
    public class SupervisorOfTeamName
    {
        [XmlElement(ElementName = "refURL")]
        public string RefURL { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "secondarySupervisorOf")]
    public class SecondarySupervisorOf
    {
        [XmlElement(ElementName = "supervisorOfTeamName")]
        public List<SupervisorOfTeamName> SupervisorOfTeamName { get; set; }
    }

    [XmlRoot(ElementName = "primarySupervisorOf")]
    public class PrimarySupervisorOf
    {
        [XmlElement(ElementName = "supervisorOfTeamName")]
        public SupervisorOfTeamName SupervisorOfTeamName { get; set; }
    }

    [XmlRoot(ElementName = "resources")]
    public class Resources
    {
        [XmlElement(ElementName = "resource")]
        public List<Resource> Resource { get; set; }
    }

}
