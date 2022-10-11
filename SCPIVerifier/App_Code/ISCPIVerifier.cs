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
