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
        public ActionResult TestControl()
        {
            ViewBag.Message = "This is the Test Control web page";

            return View();
        }

        
        [HttpGet]
        public ActionResult EquipmentControl()
        {
            ViewBag.Message = "This is the Equipment Control page";

            return View();
        }

        [HttpGet]
        public ActionResult SyntaxVerifier()
        {
            ViewBag.Message = "These tools are used for quick verification of custom files to minimize errors at run time ";
            ViewBag.FileStatus = "No file loaded";
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

                    //fileName.SaveAs(_path);
                }
                // filePath={filePath}&typeOfDevice={typeOfDevice}
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
            catch
            {
                ViewBag.Message = "File upload failed!!";
                //return View();
            }
            // verify the file exists
            //if (fileName.filePath != null)
            //{
            //    // Verify the file is not empty

            //    // If not empty send the file to the service to be parsed and verified
            //    string baseUrl = "http://http://localhost:58974/SCPIVerifier.svc/Get?value=" + fileName.filePath;
            //    HttpWebRequest hwReq1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(baseUrl));
            //    hwReq1.BeginGetResponse(new AsyncCallback(myCallbackFunc), hwReq1);


            //}
            // determine file type

            // parse the file
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
    }
}