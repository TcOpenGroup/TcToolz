

# HMI Publisher


### Introduction

HMI Publisher is a handy tool to install and manage multiple Human Machine Interfaces on remote PLCs. HMI Publisher is an open source project written in C# under the [MIT](https://tldrlegal.com/license/mit-license) license. 

### Features
* Save multiple configurations per project in .json format.
* Publish multiple HMIs at the same time.
* Restart multiple remote HMIs at the same time.
* Exclude selected HMIs from being pushed to remote or from being restarted.
* Different compression levels for pushing HMIs to servers with differing connection speeds.


### Prerequisites
* Windows on both Server and client
* Visual Studio 2019+
* .Net Framework 4.8+
* Shared folder on remote host to install HMI files to.  
* HMI Publisher Server needs to be installed and running on the host machine.  
* Firewall port (default 13700) on host needs to be accessible from HMI Publisher.  
* Visual Studio extension. (Installed Manually)

### Installation
1. Clone from Git `git clone https://github.com/TcOpenGroup/TcToolz.git`
2. Build project via Visual Studio.
3. Compile the *.iss* files via Inno Setup to generate the setup executables. Inno Setup is open source and is available at https://jrsoftware.org/isinfo.php
4. The generated *HmiPublisher.exe* will need to be run on the client side. (Where Visual Studio is run)
5. The generated *HmiPublisherServer.exe* will need to be run on the server side. (PLC)
6. Set up shared folder server side and ensure it is reachable from client side from Explorer.
7. Make sure that the listening port is open for HmiPublisherServer.exe, default port is 13700 and it can be changed in *HmiPublisherServer.cs*
8. Install the Visual Studio Extension see instructions to install from source [here](../TcOpen.VisualStudio.Tools2019/README.md) 


