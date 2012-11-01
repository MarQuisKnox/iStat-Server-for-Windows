iStat Server allows you to remotely monitor your Windows PC using [iStat for iOS](http://bjango.com/ios/istat/). iStat Server consits of a Windows Service application and a control application. The service is automatically started at boot time. The control application allows you to control the port that is used for communication and the passcode that is used for authenication.

Installing
-------

iStat Server is distributed as a standard Windows msi installer package. You will need administrator rights to install it.


ZeroConf/Bonjour Support
-------

To enable ZeroConf/Bonjour support you will need to download and install [Bonjour Print Services](http://support.apple.com/kb/DL999).


Monitoring over the internet
-------

To monitor a computer over the interenet you will more then likely need port fowarding set up on your router. iStat Server includes UPnP port forwarding support for compatible routers. You can enable and control UPnP port fowarding from the included control application.


Uninstalling
-------

iStat Server can be uninstalled from the Windows Control Panel.


License & Copyright
-------

iStat Server is licensed under the New BSD License. For more information please see license.txt
iStat Server is based on [iStatServer.NET](http://istatserver.codeplex.com) by Chris Bennight.
Copyright (c) 2012, Bjango & Chris Bennight