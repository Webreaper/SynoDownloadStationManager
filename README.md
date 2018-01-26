# SynoDownloadStationManager
Tool for cleaning up completed torrents based on tracker keyword search.

If you use a private trackers which require maintaining a seeding ratio, you may find you have 
to go in and clean up torrents from other trackers, but leave some to seed. This is particularly 
tedious and repetitive if you use Sonarr for your TV. 

This utility can be run regularly (e.g., once an hour) and will connect to the Synology DownloadStation
via the API, query the list of seeding/completed torrents, and delete any where the tracker URL doesn't 
contain any of the keywords specified in the configuration file. 

### Settings Storage

The app reads the settings from a json file. Example Json settings:
```
{
  "logLocation": "./dscleanup.log",

  "synology": {
    "username": "admin",
    "password": "my733tpwd",
    "url": "http://192.168.1.120:5000/webapi",
    "trackersToKeep": [
      "rargb",
      "tvchaos"
    ]
  },

  "email": {
    "smtpserver": "smtp.mydomain.com",
    "smtpport": 465,
    "username": "email@mydomain.com",
    "password": "my733tpwd",
    "toaddress": "email@mydomain.com",
    "fromaddress": "dsmanager@mydomain.com",
    "toname": "My email account"
  }
}
```
### Running the service

The service is a .Net/C# application, developed with Visual Studio fo Mac. It can be run on any 
architecture - for example, I run it using Mono, on a linux-based Synology NAS. Example command-line:

   mono SynoDSManager.exe

The optional command-line parameter specifies the location of the settings.json file - otherwise it
defaults to Settings.json in the current working directory. 

### Disclaimer

I accept no liability for any data loss or corruption caused by the use of this application. Your 
use of this app is entirely at your own risk - please ensure that you have adequate backups before
you use this software.

Software (C) Copyright 2017-2018 Mark Otway
