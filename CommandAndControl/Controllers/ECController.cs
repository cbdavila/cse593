using CommandAndControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CommandAndControl.Controllers
{
    public class ECController : Controller
    {
        public static string[] deviceList;
        public static List<deviceType> devices = new List<deviceType>();


        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EquipmentControl()
        {
            ViewBag.Message = "This is the Equipment Control page";
            ValidateConfigFileList();
            GetConfigFileList();

            string[] tmpDevList = new string[deviceList.Length-1];
            int tmpDevCounter = 0;
            string absPath = "";
            string file = "";
            XmlSerializer serializer = new XmlSerializer(typeof(deviceType));
            for (int i = 0; i < deviceList.Length; i++)
            {
                // parse all files at the App_Data directory path
                absPath = Path.Combine(Server.MapPath("~/App_Data"), deviceList[i]);

                file = Path.GetFileName(absPath);
                deviceType deserializeDevice = (deviceType)serializer.Deserialize(new XmlTextReader(absPath));
                devices.Add(deserializeDevice);
                if (deviceList[i] != "CommonCommands.config")
                {
                   // tmpDevList[tmpDevCounter] = deviceList[i].Replace(".config", "");
                    
                    tmpDevList[tmpDevCounter] = deserializeDevice.name;
                    tmpDevCounter++;
                }
                
            }
            deviceList = tmpDevList;
            ViewBag.deviceList = deviceList;

            return View();
        }

        [HttpPost]
        public ActionResult EquipmentControl(SCPICommand scpiCommand)
        {
            CommandMessage cmdMessage = new CommandMessage
            {
                message = "NoGood\n",
                pass = false
            };

            string scpiCmdString = "";
            scpiCommand.command = scpiCommand.command.Trim();
            foreach (deviceType i in devices)
            {
                // validate device name
                if (scpiCommand.device.Equals( i.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    cmdMessage.message = ("Device "+ scpiCommand.device+ " exists.\n");

                    string[] str = scpiCommand.command.Split(' ');
                    scpiCommand.command = str[0];
                    // for remainder, these are consider parameters for values
                    for(int j = 1; j < str.Length; j++)
                    {
                       scpiCommand.valueList.Add(str[j]);
                    }

                    // validate command for this device
                    List<commandType> cmdStrings = i.commands.ToList();
                    int itemLoc = 0;
                    for(int j = 0; j < cmdStrings.Count; j++)
                    {
                        if(scpiCommand.command.Equals(cmdStrings[j].cmdName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            cmdMessage.message += ("valid command found\n");
                            itemLoc = j;
                            break;
                        }
                       
                    }

                    scpiCmdString = i.commands[itemLoc].scpi;
                    int valueCount = 0;//= scpiCommand.valueList.Count;

                    // validate parameters
                    List < parameterType> paramList = i.commands[itemLoc].Items[0].parameter.ToList();
                    for (int j = 0; j < paramList.Count; j++)
                    {
                        if (scpiCommand.command.Equals(("Output"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (scpiCommand.valueList[valueCount].Equals(paramList[j].power.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                cmdMessage.message += ("valid parameter found\n");
                                scpiCmdString = scpiCmdString.Replace("PARAM"+(valueCount + 1), paramList[j].power.ToString());
                                //itemLoc = j;
                                valueCount++;
                                //break;
                            }
                            //else
                            //{
                            //    cmdMessage.message += ("invalid parameter\n");
                            //    j = cmdStrings.Count;
                            //}
                        }
                        else if (scpiCommand.command.Equals(("Voltage"), StringComparison.InvariantCultureIgnoreCase))
                        {

                            // check if is within min and max
                            double min = paramList[j].min;
                            double max = paramList[j].max;
                            double val1 = double.Parse(scpiCommand.valueList[valueCount], System.Globalization.CultureInfo.InvariantCulture);
                            //double val = (double)scpiCommand.valueList[valueCount];
                            if ( min <= val1 && val1<=max)
                            {
                                scpiCmdString = scpiCmdString.Replace("PARAM" + (valueCount + 1), val1.ToString());
                                valueCount++;

                            }

                        }
                        else if (scpiCommand.command.Equals(("Current"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            double min = paramList[j].min;
                            double max = paramList[j].max;
                            double val1 = double.Parse(scpiCommand.valueList[valueCount], System.Globalization.CultureInfo.InvariantCulture);
                            //double val = (double)scpiCommand.valueList[valueCount];
                            if (min <= val1 && val1 <= max)
                            {
                                scpiCmdString = scpiCmdString.Replace("PARAM" + (valueCount + 1), val1.ToString());
                                valueCount++;

                            }
                        }
                        else
                        {
                           // command not valid, should not have reached this ever

                        }
                    }
                    continue;
                }
                else
                {
                    cmdMessage.message = ("Device " + scpiCommand.device + " does not exist\n");
                }
            }

            // Verify Device, Command, and Parameters against the devices container

            try
            {
                string FullUrl = "http://localhost:60719/EquipmentControl.svc/executeSCPI?deviceName=";
                //FullUrl = FullUrl + scpiCommand.device + "&command=" + scpiCommand.command;
                FullUrl = FullUrl + scpiCommand.device + "&command=" + scpiCmdString;

                HttpWebRequest req1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(FullUrl));
                HttpWebResponse response = (HttpWebResponse)req1.GetResponse();

                StreamReader sReader;
                sReader = new StreamReader(response.GetResponseStream());
                using (sReader = new StreamReader(response.GetResponseStream()))
                {
                    // painfully parse object
                    XDocument xDoc = XDocument.Parse(sReader.ReadToEnd().ToString());

                    List<XNode> l = xDoc.DescendantNodes().ToList();
                    XNode node2 = l[2];
                    XNode node4 = l[4];

                    string msg = node2.ToString();
                    string pass = node4.ToString();

                    cmdMessage.message = msg;
                    cmdMessage.pass = pass == "true" ? true : false;
                }
                
            }
            catch(Exception e)
            {
                string msg = e.Message;
                ViewBag.DeviceStatus = "Could execute command : " + scpiCommand.command + " for device : " + scpiCommand.device ;
            }

            if (cmdMessage.pass)
            {
                ViewBag.DeviceStatus = "Command Executed Successfully";
            }
            else
            {
                ViewBag.DeviceStatus = cmdMessage.message;
            }
            ViewBag.deviceList = deviceList;

            // send command requipt to equipment control service
            //ViewBag.Message = "This is the Equipment Control page";

            return View();
        }


        // this simply updates the config file list
        // I want to execute this on the service startup, not sure how to yet.
        public void ValidateConfigFileList()
        {
            try
            {
                string appData = Server.MapPath("~/App_Data");
                //string[] testFiles = Directory.GetFiles(appData);
                string FullUrl = "http://localhost:58974/SCPIVerifier.svc/ValidateConfigFiles?";
                HttpWebRequest req1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(FullUrl));
                HttpWebResponse response = (HttpWebResponse)req1.GetResponse();

                StreamReader sReader;
                using (sReader = new StreamReader(response.GetResponseStream()))
                {
                    string str = sReader.ReadToEnd().ToString();

                }
            }
            catch
            {
                ViewBag.Message = "Could verify config files files";

            }
        }

        public void GetConfigFileList()
        { 
            try
            {
                //string appData = Server.MapPath("~/App_Data");
                string FullUrl = "http://localhost:58974/SCPIVerifier.svc/GetTestFiles?";
                HttpWebRequest req1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(FullUrl));
                HttpWebResponse response = (HttpWebResponse)req1.GetResponse();

                StreamReader sReader;
                using (sReader = new StreamReader(response.GetResponseStream()))
                {
                    string str = sReader.ReadToEnd().ToString();
                    str = str.Replace("ArrayOfstring", "ArrayOfString");
                    XDocument xDoc = XDocument.Parse(str);

                    XElement root = xDoc.Root;

                    IEnumerable<XElement> tmpParams = root.Elements();
                    List<string> fileList = new List<string>();

                    foreach (XElement param in tmpParams)
                    {
                        string value = param.Value;
                    //    value = value.Replace(".xml", ".config");
                        fileList.Add(value);
                    }

                    deviceList = fileList.ToArray<string>();

                    deviceList.Select(s => new SelectListItem { Value = s }).ToList();
                    //ViewBag.FileNames = fileNames;
                }
            }
            catch
            {
                ViewBag.Message = "Could not get equipment config files";
            }
        }
    }
}