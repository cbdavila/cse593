using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommandAndControl.Models
{
    public class Device
    {
        string name;
        string model;
        string manufacturer;
        Dictionary<string, string> commands; //<command, scpiString
        Dictionary<string, string[]> commandParameters; // command, scpi Parameter options
    }
}