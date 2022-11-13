using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using CommandAndControl.Models;



public class SCPIVerifier : ISCPIVerifier
{
    public ValidType validType = new ValidType();
    public ValidFileLists validFileLists = new ValidFileLists();
    public bool ValidateConfigFiles()
    {
        //string appData = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
        XmlSchemaSet schemaSet = new XmlSchemaSet();
        schemaSet.Add(null, "https://www.public.asu.edu/~cbdavila/Project/device.xsd");
        // add the schema set to the reader settings for verifications
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ValidationType = ValidationType.Schema;
        settings.Schemas = schemaSet;
        settings.XmlResolver = new XmlUrlResolver();
        settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

        XmlReader rdr;
        string prefix = "https://www.public.asu.edu/~cbdavila/Project/";
        List<string> unused = validFileLists.configFileList;

        int configFileSize = validFileLists.configFileList.Count;
        for (int i = 0; i < configFileSize; i++)
        {
            string full = prefix + unused[i];
            rdr = XmlReader.Create(full, settings);
            while (rdr.Read())
            {
                //Console.WriteLine("Xml file is valid for the given xsd file");
            }

            // If config file is invalid (e.g syntax, remove it from list)
            if (validType.boolValid == false)
            {
                unused.RemoveAt(i);
                // send the message to user
                // validType.errorMessage
            }

        }

        return validType.boolValid;
    }
    public string[] GetTestFiles()
    {
        string[] files = validFileLists.configFileList.ToArray();
        return files;
    }

    // This function is meant to verify a single file
    // The file can be any config file that is not already validated and in the recognized device lists
    public bool VerifyConfigFile(string filePath)
    {
        bool valid = true;


        XmlSchemaSet schemaSet = new XmlSchemaSet();
        schemaSet.Add(null, "https://www.public.asu.edu/~cbdavila/Project/device.xsd");
        // add the schema set to the reader settings for verifications
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ValidationType = ValidationType.Schema;
        settings.Schemas = schemaSet;
        settings.XmlResolver = new XmlUrlResolver();
        settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

        XmlReader rdr;
        string prefix = "https://www.public.asu.edu/~cbdavila/Project/";


        //string full = prefix + filePath;
        rdr = XmlReader.Create(filePath, settings);
        while (rdr.Read())
        {
            //Console.WriteLine("Xml file is valid for the given xsd file");
        }

        // If config file is invalid (e.g syntax, remove it from list)
        if (validType.boolValid == false)
        {
            valid = false;
        }

        return valid;
    }

    //TODO: get deserialize xml object from deviceList

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

    //borrowed below form page 205 of Dr Chens book
    // Display any warnings or errors.
    private void ValidationCallBack(object sender, ValidationEventArgs args)
    {
        validType.boolValid = false;
        if (args.Severity == XmlSeverityType.Warning)
            Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
        else
            Console.WriteLine("\tValidation error: " + args.Message);

        validType.errorString = args.Message;
    }
}
