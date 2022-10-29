using CommandAndControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace CommandAndControl.Controllers
{
    public class ECController : Controller
    {
        public static string[] deviceList;
 
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EquipmentControl()
        {
            ViewBag.Message = "This is the Equipment Control page";


            GetConfigFileList();
            string[] tmpDevList = new string[deviceList.Length-1];
            int tmpDevCounter = 0;
            for (int i = 0; i < deviceList.Length; i++)
            {
                if(deviceList[i] != "CommonCommands.config")
                {
                    tmpDevList[tmpDevCounter] = deviceList[i].Replace(".config", "");
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

            try
            {
                //string FullUrl = "http://localhost:60719/Service.svc/executeSCPI?deviceName={deviceName}&command={command}";
                string FullUrl = "http://localhost:60719/Service.svc/executeSCPI?deviceName=";
                FullUrl = FullUrl + scpiCommand.device + "&command=" + scpiCommand.command;

                HttpWebRequest req1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(FullUrl));
                HttpWebResponse response = (HttpWebResponse)req1.GetResponse();

                StreamReader sReader;
                using (sReader = new StreamReader(response.GetResponseStream()))
                {

                    XDocument xDoc = XDocument.Parse(sReader.ReadToEnd().ToString());
                    XElement xElem = xDoc.Root;
                    string pass = xElem.Value;
                    if (pass == "true")
                    {
                        ViewBag.FileStatus = "Command Executed Successfully";

                    }
                    else
                    {
                        ViewBag.FileStatus = "Command Failed To Executed!";
                    }

                }
            }
            catch
            {
                ViewBag.Message = "Could execute command : " + scpiCommand.command + " for device : " + scpiCommand.device ;
            }




            ViewBag.deviceList = deviceList;


            // send command requipt to equipment control service
            ViewBag.Message = "This is the Equipment Control page";

            return View();
        }

        public void GetConfigFileList()
        {
            try
            {

                string appData = Server.MapPath("~/App_Data");
                //string[] testFiles = Directory.GetFiles(appData);
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
                        value = value.Replace(".xml", ".config");
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