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

        //borrowed below form page 205 of Dr Chens book
        // Display any warnings or errors.
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
            else
                Console.WriteLine("\tValidation error: " + args.Message);
        }


        public void GetConfigFileList()
        {

            string returnMsg = "Error Detected";

            string appData = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            //string appData = Server.MapPath("~/App_Data");

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            //string deviceSchema = appData + "\\device.xsd";
            schemaSet.Add(null, "https://www.public.asu.edu/~cbdavila/Project/device.xsd");
            //schemaSet.Add(appData, "device.xsd");

            // add the schema set to the reader settings for verifications
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schemaSet;
            settings.XmlResolver = new XmlUrlResolver();
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            //string XMLFileLocation = appData + "\\N6700C.config";
            XmlReader rdr = XmlReader.Create("https://www.public.asu.edu/~cbdavila/Project/N6700C.config", settings);
            XmlReader ccrdr = XmlReader.Create("https://www.public.asu.edu/~cbdavila/Project/CommonCommands.config", settings);
            while (rdr.Read())
            {
                Console.WriteLine("Xml file is valid for the given xsd file");
                returnMsg = "No Error";
            }
            while (ccrdr.Read())
            {
                Console.WriteLine("Xml file is valid for the given xsd file");
                returnMsg = "No Error";
            }
            //return returnMsg;

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