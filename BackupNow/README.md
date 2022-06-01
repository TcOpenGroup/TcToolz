

# Backup Now


### Introduction

Backup Now is a standalone tool for Windows for backing up data to a remote location. Backup Now is an open source project written by in C# under the [MIT](https://tldrlegal.com/license/mit-license) license. 

### Features
* Set and save multiple back up tasks.
* Enable/Disable back up tasks for individual or batch processing.
* Issue 'Shutdown' command upon completion.
* Scan for changes (Manually)

### Prerequisites
* Windows 10+
* Visual Studio 2019+
* .Net Framework 4.7.2+
* [Inno Setup](https://jrsoftware.org/isinfo.php)


### Installation
1. Clone from Git `git clone https://github.com/TcOpenGroup/TcToolz.git`
2. Compile source with Visual Studio in release mode.
3. Compile the .iss file via Inno Setup to generate the setup executable. Inno Setup is open source and is available at https://jrsoftware.org/isinfo.php
4. Run the generated *BackupNow.exe*

