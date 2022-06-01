# IP Config



### Introduction

IP Config is a handy tool when setting up remote a machine and one has to frequently change the IP or the subnet mask on one of the ethernet interfaces. IP Config is an open source project written in C# under the [MIT](https://tldrlegal.com/license/mit-license) license.



### Features
* Save multiple settings for each Ethernet/Wifi interface.
* Quickly swap between saved settings for each interface.
* Easily ping destination IP for troubleshooting.

### Prerequisites
* Windows 10+
* Visual Studio 2019+
* .Net Framework 4.7.2
* [Inno Setup](https://jrsoftware.org/isinfo.php)

### Installation
1. Clone from Git git clone https://github.com/TcOpenGroup/TcToolz.git
2. Compile source with Visual Studio in release mode.
3. Compile the .iss file via Inno Setup to generate the setup executable. Inno Setup is open source and is available at https://jrsoftware.org/isinfo.php
4. Run the generated *IPCONFIG.exe*

