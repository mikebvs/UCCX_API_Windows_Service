using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace UCCX_API_Service
{
    [XmlRoot(ElementName = "skill")]
    public class Skill
    {
        [XmlElement(ElementName = "self")]
        public string Self { get; set; }
        [XmlElement(ElementName = "skillId")]
        public string SkillId { get; set; }
        [XmlElement(ElementName = "skillName")]
        public string SkillName { get; set; }
    }

    [XmlRoot(ElementName = "skills")]
    public class Skills
    {
        [XmlElement(ElementName = "skill")]
        public List<Skill> Skill { get; set; }
    }

}
