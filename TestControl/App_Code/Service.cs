using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
using CommandAndControl.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
	public bool RunTest(string filePath)
	{
		bool valid = false;
		// parse the file at filePath
		// execute each script by calling the EC service
		foreach (string line in System.IO.File.ReadLines(filePath))
		{

			if (line.StartsWith("//"))
			{
				// skip comment lines
			}
			else
			{
				SCPICommand cmd = new SCPICommand
				{
					device = "unknown",
					command = "unknown",
					valueList = new List<string>()
			     };

				string[] words = line.Split(' ');
				cmd.device = words[0];
				cmd.command = words[1];
				if (words.Length > 2)
                {
					for(int i = 2; i < words.Length; i++)
                    {
						cmd.valueList.Add(words[i]);
                    }
                }
				// send request to equipment control
				// Prepare the url string to send
				string parameterString = "&parameters=";
				CommandMessage cmdMessage = new CommandMessage
				{
					message = "NoGood",
					pass = false
				};
				if(cmd.valueList.Count !=0)
                {
					for(int i = 0; i<cmd.valueList.Count; i++)
                    {
						if (i == (cmd.valueList.Count - 1))
						{
							parameterString += cmd.valueList[i];

						}
						else
                        {
							parameterString += cmd.valueList[i] + ",";
						}

					}
                }
                else
                {
					parameterString += "none";
                }

				try
				{
					string FullUrl = "http://localhost:60719/Service.svc/executeSCPICommand?deviceName=";
				//string valueString = "empty";
					FullUrl = FullUrl + cmd.device + "&command=" + cmd.command+parameterString;

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
				catch (Exception e)
				{
					string msg = e.Message;
					//ViewBag.DeviceStatus = "Could execute command : " + ////scpiCommand.command + " for device : " + scpiCommand.device;
				}

				if (cmdMessage.pass)
				{
					//ViewBag.DeviceStatus = "Command Executed Successfully";
				}
				else
				{
					//ViewBag.DeviceStatus = cmdMessage.message;
				}
			}

			// no errors
			valid = true;
		}

		return valid;
	}
	
}

