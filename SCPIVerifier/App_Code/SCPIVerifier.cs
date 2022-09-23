using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Xml.Linq;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class SCPIVerifier : ISCPIVerifier
{
	public string GetData(int value)
	{
		return string.Format("You entered: {0}", value);
	}

	public bool VerifyConfigFile(string filePath)
    {
		// TODO:
	//	1.Parse "device"
	//		a.Parse "commands"
	//			i.Parse "command"
	//				1) Parse "parameters"
	//					a)Parse "Parameter"

		bool valid = false;

		// read the file

		// check syntax
		//verifyScpiString(line, typeOfDevice);
		StreamReader sReader = new StreamReader(filePath);
		using (sReader)
		{
			//XDocument xDoc = XDocument.Parse(sReader.ReadToEnd().ToString());

			if (sReader.BaseStream.Position > 0)
			{
				sReader.BaseStream.Position = 0;
			}

			try
			{
				XDocument xDoc = XDocument.Load(sReader);
				// device
				if (xDoc.Element("device") != null)
				{
					XElement root = xDoc.Root;
					string deviceName = root.Attribute("name").Value;
					string model = root.Attribute("model").Value;
					string manufacturer = root.Attribute("manufacturer").Value;

					// common
					if (deviceName == "common")
					{
						// parse common commands file
					}
					else
					{
						// actual equipment config files
						// commands
						IEnumerable<XElement> tmpCmd = root.Elements("commands");
						if (tmpCmd != null)
                        {
							string cmdString = "";
							string scpiString = "";
							IEnumerable<XElement> commands = tmpCmd.Elements("command");
							foreach (XElement command in commands)
							{
								//command
								if (command.HasAttributes)
								{
									cmdString = command.Attribute("cmdName").Value;
									scpiString = command.Attribute("scpi").Value;
								}
								IEnumerable<XElement> tmpParams = command.Elements("parameters");
								// parameters
								if (tmpParams != null)
                                {
									if(tmpParams.First().HasAttributes)
                                    {
										string defaulValue = tmpParams.First().Attribute("default").Value;

									}
									// parameter
									IEnumerable<XElement> parameters = tmpParams.Elements("parameter");
									foreach (XElement param in parameters)
                                    {
										string value = param.Value;
                                        if (param.HasAttributes)
                                        {
											string min = param.Attribute("min").Value;
											string max = param.Attribute("max").Value;
										}										
                                    }
                                }
								// command parameters
							}

							//IEnumerable<XElement> parameters = element.Descendants("parameters");
							//var e = parameters.First();
							// iterate over each parameter
							//e.Element("parameter").Attribute("max").Value
							
							valid = true;
						}
					}
				}
				
			}catch(XmlException exc)
            {
				string exceptionMessage = exc.Message;
            }


		}

		return valid;
    }

	// verifyScpiString
	// Purpose: determines if a string is valid for the type of device entered
	private bool verifyScpiString(string scpiString, string typeOfDevice)
    {
		bool valid = false;

		using (StreamReader sReader = new StreamReader(scpiString))
		{
			XDocument xDoc = XDocument.Parse(sReader.ReadToEnd().ToString());
			XElement xElem = xDoc.Root;
			//bool pend = xElem.Value;
		}


		return valid;
	}

	// loadAllDevices
	// Purpose: reads and load all SCPI device files into memory. Executed on start up
	private void loadAllDevices()
    {

    }
}
