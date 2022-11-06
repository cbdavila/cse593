using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using CommandAndControl.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
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



}
