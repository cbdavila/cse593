// File: N6700C.c
// Purpose: The purpose of this file is to emulate the Keysight N6700C Signal Generator
//          Basic implementation includes a socket connection, prints received commands
//          to be executed, and responds with whether command was executed successfully
//          or not.


/* A simple server in the internet domain using TCP
   The port number is passed as an argument */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>
#include <signal.h>

// Global variables
static volatile sig_atomic_t running = 1;

// Global functions
static void handleSignal(int sig)
{
	(void)sig;
	running = 0;
}

void error(const char *msg)
{
    perror(msg);
    exit(1);
}

int main(int argc, char *argv[])
{
     int mainSocketFd, clientSocketFd, portno;
     socklen_t clilen;
     char buffer[256];
     struct sockaddr_in serv_addr, cli_addr;
     int n;
     if (argc < 2) {
         fprintf(stderr,"ERROR, no port provided\n");
         exit(1);
     }
     mainSocketFd = socket(AF_INET, SOCK_STREAM, 0);
     if (mainSocketFd < 0) 
        error("ERROR opening socket");
	
	 printf("open socket successfully\n");
	 
     bzero((char *) &serv_addr, sizeof(serv_addr));
     portno = atoi(argv[1]);
     serv_addr.sin_family = AF_INET;
     serv_addr.sin_addr.s_addr = INADDR_ANY;
     serv_addr.sin_port = htons(portno);
     if (bind(mainSocketFd, (struct sockaddr *) &serv_addr,
              sizeof(serv_addr)) < 0) 
              error("ERROR on binding");
			  
	printf("listening for socket connection\n");
	
	listen(mainSocketFd,5);
    clilen = sizeof(cli_addr);
	 
	while(running)
    {		
		clientSocketFd = accept(mainSocketFd, 
					 (struct sockaddr *) &cli_addr, 
					 &clilen);
		if (clientSocketFd < 0) 
			  error("ERROR on accept");
		bzero(buffer,256);
		
		printf("socket connection accepted\n");
		
		n = read(clientSocketFd,buffer,255);
		if (n < 0) 
			error("ERROR reading from socket");
		
		printf("Here is the message: %s\n",buffer);
		n = write(clientSocketFd,"Good",4);
		
		if (n < 0) 
			error("ERROR writing to socket");
		
		close(clientSocketFd);
	}
    close(mainSocketFd);
    return 0; 
}