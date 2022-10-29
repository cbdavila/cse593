using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CommandAndControl.Models
{
    [XmlRoot(ElementName ="CommandMessage", Namespace = "http://schemas.datacontract.org/2004/07/CommandAndControl.Models")]
    public class CommandMessage
    {

        [XmlElement("passNode")]
        public bool pass { get; set; } // whether this message executed successfully or not
        [XmlElement("messageNode")]
        public string message { get; set; } // message along line of execution, usually for errors, etc...
        

    }
}