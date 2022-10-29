using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class SCPIVerifier : ISCPIVerifier
{ 


	public string[] GetTestFiles()
	{

		string workingDirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
		string[] files = Directory.GetFiles(workingDirectory);
		for(int i =0; i < files.Length; i++)
        {
			files[i] = Path.GetFileName(files[i]);
        }

		

		return files;
	}

	public bool VerifyConfigFile(string filePath)
    {

		bool valid = false;

		StreamReader sReader = new StreamReader(filePath);
		using (sReader)
		{
 
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
					

					// common
					if (deviceName == "common")
					{
						valid = true;
					}
					else
					{
						string model = root.Attribute("model").Value;
						string manufacturer = root.Attribute("manufacturer").Value;
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
									valid = true;
								}
								// command parameters
							}

							//IEnumerable<XElement> parameters = element.Descendants("parameters");
							//var e = parameters.First();
							// iterate over each parameter
							//e.Element("parameter").Attribute("max").Value
							
							//valid = true;
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
