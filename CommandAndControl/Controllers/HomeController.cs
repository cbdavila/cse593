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
    public class HomeController : Controller
    {

        public static Boolean validFile = false;
        public static string[] fileNames;


        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult About()
        {
            ViewBag.Message = "About page.";

            return View();
        }


        [HttpGet]
        public ActionResult SyntaxVerifier()
        {
            ViewBag.Message = "These tools are used for quick verification of custom files to minimize errors at run time ";
            ViewBag.FileStatus = "No file loaded";
            ViewBag.FileNames = new string[] { };

            // Query the 'test files' available to test located in server directories.
            // prompt user to select a file and validate it


            ValidateConfigFileList();
            GetConfigFileList();
            ViewBag.FileNames = fileNames;

            return View();
        }
         
        [HttpPost]
        public ViewResult SyntaxVerifier(FileName fileName)
        {
            ViewBag.FileStatus = "No file loaded";
            string absPath = "";
            
            try
            {
                if (fileName.filePath != null)
                {
                    absPath = Path.Combine(Server.MapPath("~/App_Data"), fileName.filePath);
                    
                    string file = Path.GetFileName(absPath);
                }
                string baseUrl = "http://localhost:58974/SCPIVerifier.svc/VerifyConfigFile?filePath=";
                string FullUrl = baseUrl + absPath;
                HttpWebRequest req1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(FullUrl));
                HttpWebResponse response = (HttpWebResponse)req1.GetResponse();

                //ViewBag.Message = "File Uploaded Successfully!!";

                using (StreamReader sReader = new StreamReader(response.GetResponseStream()))
                {
                    XDocument xDoc = XDocument.Parse(sReader.ReadToEnd().ToString());
                    XElement xElem = xDoc.Root;
                    string pass = xElem.Value;
                    if(pass == "true")
                    {
                        ViewBag.FileStatus = "SCPI File Syntax is valid";

                    }
                    else
                    {
                        ViewBag.FileStatus = "Incorrect SCPI Syntax";
                    }
                }

            }
            catch(Exception e)
            {
                
                string msg = e.ToString();
                ViewBag.FileStatus = "File upload failed ! :" + msg;
                //return View();
            }

            // parse the file
            ViewBag.FileNames = fileNames;
            return View();
        }
        private static void myCallbackFunc(IAsyncResult requestObj)
        {
            string str;
            HttpWebRequest hwReq2 = (HttpWebRequest)requestObj.AsyncState;
            HttpWebResponse hwResponse = (HttpWebResponse)hwReq2.EndGetResponse(requestObj);
            using (StreamReader sReader = new StreamReader(hwResponse.GetResponseStream()))
            {
                //get the secret number 
                str = sReader.ReadToEnd().ToString();
            }
        }

        public bool ValidateSingleFile()
        {
            bool valid = false;

            return valid;
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

                    fileNames = fileList.ToArray<string>();

                    fileNames.Select(s => new SelectListItem { Value = s }).ToList();
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


