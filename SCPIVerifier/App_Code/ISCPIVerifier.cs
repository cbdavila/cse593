using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
[ServiceContract]
public interface ISCPIVerifier
{

	[OperationContract]
	[WebGet(UriTemplate = "ValidateConfigFiles?")]
	bool ValidateConfigFiles();


	[OperationContract]
	[WebGet(UriTemplate="GetTestFiles?")]
	string[] GetTestFiles();


	// ParseScpiFile
	// Purpose: This file simply verifies if a SCPI file syntax is valid.
	//          A valid SCPI file should identify the equipment device to be valid.
	//          One exception are 'common' command files
	[OperationContract]
	[WebGet(UriTemplate = "VerifyConfigFile?filePath={filePath}")]
	bool VerifyConfigFile(string filePath);

}

// Use a data contract as illustrated in the sample below to add composite types to service operations.
[DataContract]
public class ValidType
{
	bool valid = true;
	string errorMessage = "No Error";
	[DataMember]
	public bool boolValid
	{
		get { return valid; }
		set { valid = value; }
	}

	public string errorString
    {
		get { return errorMessage; }
		set { errorMessage = value;}
    }

}

[DataContract]
public class ValidFileLists
{
	// this list contains a list of all possible config files at startup, config files are removed if they are invalid.
	List<string> configFiles = new List<string>() { "N6700C.config", "CommonCommands.config" };
	public List<string> configFileList
    {
		get { return configFiles; }
        set { configFiles = value; }
    }
	
}

