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
                if(deviceList[i] != "CommonCommands.config")
                {
                    tmpDevList[tmpDevCounter] = deviceList[i].Replace(".config", "");
                    tmpDevCounter++;
                }
                // parse all files at the App_Data directory path
                absPath = Path.Combine(Server.MapPath("~/App_Data"), deviceList[i]);

                file = Path.GetFileName(absPath);
                deviceType deserializeDevice = (deviceType)serializer.Deserialize(new XmlTextReader(absPath));
                devices.Add(deserializeDevice);
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
                message = "NoGood",
                pass = false
            };

            try
            {
                string FullUrl = "http://localhost:60719/Service.svc/executeSCPI?deviceName=";
                FullUrl = FullUrl + scpiCommand.device + "&command=" + scpiCommand.command;

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