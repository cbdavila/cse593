using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CommandAndControl.Models;
using System.IO;
using System.Xml.Linq;

namespace CommandAndControl.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult TestControl()
        {
            ViewBag.Message = "This is the Test Control web page";

            return View();
        }


        [HttpPost]
        public ActionResult TestControl(FileName fileName)
        {
            ViewBag.Message = "This is the Test Control web page";

            ViewBag.FileStatus = "No file loaded";
            string absPath = "";

            try
            {
                if (fileName.filePath != null)
                {
                    absPath = Path.Combine(Server.MapPath("~/App_Data"), fileName.filePath);

                    string file = Path.GetFileName(absPath);
                }
                string baseUrl = "http://localhost:58911/TestControl.svc/RunTest?filePath=";
                string FullUrl = baseUrl + absPath;
                HttpWebRequest req1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(FullUrl));
                HttpWebResponse response = (HttpWebResponse)req1.GetResponse();

                //ViewBag.Message = "File Uploaded Successfully!!";

                using (StreamReader sReader = new StreamReader(response.GetResponseStream()))
                {
                    XDocument xDoc = XDocument.Parse(sReader.ReadToEnd().ToString());
                    XElement xElem = xDoc.Root;
                    string pass = xElem.Value;
                    if (pass == "true")
                    {
                        ViewBag.FileStatus = "Test Script executed";

                    }
                    else
                    {
                        ViewBag.FileStatus = "Test script failed";
                        // provide error
                    }
                }

            }
            catch (Exception e)
            {

                string msg = e.ToString();
                ViewBag.FileStatus = "File upload failed ! :" + msg;
                //return View();
            }

            // parse the file
            //ViewBag.FileNames = fileNames;
            return View();
        }
    }
}