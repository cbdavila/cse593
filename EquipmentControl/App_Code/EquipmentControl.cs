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
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.Schema;
using CommandAndControl.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class EquipmentControl : IEquipmentControl
{
    public static string[] deviceList;
    public static List<deviceType> devices = new List<deviceType>();
    public string GetData(int value)
    {
        return string.Format("You entered: {0}", value);
    }

    public CompositeType GetDataUsingDataContract(CompositeType composite)
    {
        if (composite == null)
        {
            throw new ArgumentNullException("composite");
        }
        if (composite.BoolValue)
        {
            composite.StringValue += "Suffix";
        }
        return composite;
    }
    public CommandMessage executeSCPI(string deviceName, string command)
    {
        //bool executed = false;
        CommandMessage cmdMessage = null;

        // send the command string to remote device
        cmdMessage = openDeviceSocketConnection(deviceName, command);
        //if(cmdMessage.pass)
        //{
        //    executed = true;
        //}

        //return executed;
        return cmdMessage;
    }

    public CommandMessage executeSCPICommand(string deviceName, string command, string parameters)
    {
        //CommandMessage cmdMessage = null;
        // cmdMessage.pass = false;
        //cmdMessage.message = "No Command Executed";
        CommandMessage cmdMessage = new CommandMessage
        {
            message = "NoGood\n",
            pass = false
        };


        // look up actual SCPI command string

        // replace <PARAM> string with parameters

        // finalize the string command to be sent 

        // send to device name

        if (deviceName == "MPS1")
        {
            string scpiCmdString = "";
            //scpiCommand.command = scpiCommand.command.Trim();

            foreach (deviceType i in devices)
            {
                // validate device name
                if (deviceName.Equals(i.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    cmdMessage.message = ("Device " + deviceName + " exists.\n");

                    string[] userParamList = parameters.Split(' ');

                    // for remainder, these are consider parameters for values
                    //for (int j = 1; j < str.Length; j++)
                    //{
                    //   scpiCommand.valueList.Add(str[j]);
                    //}

                    // validate command for this device
                    List<commandType> cmdStrings = i.commands.ToList();
                    int itemLoc = 0;
                    for (int j = 0; j < cmdStrings.Count; j++)
                    {
                        if (command.Equals(cmdStrings[j].cmdName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            cmdMessage.message += ("valid command found\n");
                            itemLoc = j;
                            break;
                        }

                    }

                    //scpiCmdString = i.commands[itemLoc].scpi;
                    int valueCount = 0;//= scpiCommand.valueList.Count;

                    // validate parameters
                    List<parameterType> liveParamList = i.commands[itemLoc].Items[0].parameter.ToList();
                    for (int k = 0; k < userParamList.Length; k++)
                    {
                        for (int j = 0; j < liveParamList.Count; j++)
                        {
                            if (command.Equals(("Output"), StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (userParamList[j].Equals(liveParamList[j].ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    cmdMessage.message += ("valid parameter found\n");
                                    scpiCmdString = scpiCmdString.Replace("PARAM" + (valueCount + 1), liveParamList[j].power.ToString());
                                    //itemLoc = j;
                                    valueCount++;
                                    break;
                                }
                                //else
                                //{
                                //    cmdMessage.message += ("invalid parameter\n");
                                //    j = cmdStrings.Count;
                                //}
                            }
                            //else if (scpiCommand.command.Equals(("Voltage"), StringComparison.InvariantCultureIgnoreCase))
                            //{

                            //    // check if is within min and max
                            //    double min = paramList[j].min;
                            //    double max = paramList[j].max;
                            //    double val1 = double.Parse(scpiCommand.valueList[valueCount], System.Globalization.CultureInfo.InvariantCulture);
                            //    //double val = (double)scpiCommand.valueList[valueCount];
                            //    if (min <= val1 && val1 <= max)
                            //    {
                            //        scpiCmdString = scpiCmdString.Replace("PARAM" + (valueCount + 1), val1.ToString());
                            //        valueCount++;

                            //    }

                            //}
                            //else if (scpiCommand.command.Equals(("Current"), StringComparison.InvariantCultureIgnoreCase))
                            //{
                            //    double min = paramList[j].min;
                            //    double max = paramList[j].max;
                            //    double val1 = double.Parse(scpiCommand.valueList[valueCount], System.Globalization.CultureInfo.InvariantCulture);
                            //    //double val = (double)scpiCommand.valueList[valueCount];
                            //    if (min <= val1 && val1 <= max)
                            //    {
                            //        scpiCmdString = scpiCmdString.Replace("PARAM" + (valueCount + 1), val1.ToString());
                            //        valueCount++;

                            //    }
                            //}
                            else
                            {
                                // command not valid, should not have reached this ever

                            }

                        }
                    }
                    //continue;
                }
                else
                {
                    // cmdMessage.message = ("Device " + scpiCommand.device + " does not exist\n");
                }

            }
        }
        else
        {
            cmdMessage.message = "Device : " + deviceName + " does not exist";
        }

            return cmdMessage;
        
    }

    public CommandMessage openDeviceSocketConnection(string deviceName, string command)
    {
        CommandMessage cmdMessage = new CommandMessage
        {
            message = "NoGood",
            pass = false
        };

        //string pass = "NoGood";
        try
        {
            TcpClient client = new TcpClient("localhost", 12345);
            string message = deviceName+":"+command;
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent: {0}", message);

            // Receive the server response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;


            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            if(responseData == "Good")
            {
                //pass = "Good";
                cmdMessage.message = "Good";
                cmdMessage.pass = true;
            }

            //pass = responseData;
            cmdMessage.message = responseData;
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
            cmdMessage.message = e.Message;


        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
            cmdMessage.message = e.Message;
        }

        return cmdMessage;
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
            //Viewbag.Message = "Could not get equipment config files";
        }
    }



}
