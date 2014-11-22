Dynamics NAV Application Profiler
=================================
Sample for profiling Microsoft Dynamics NAV Application code. Consumes NAV Execution Events from ETW.

The sample consists of C# code for listening to NAV Server ETW events and Application objects for storing the events in a Table and presenting the results in a Page.

**Disclamer:** Use at own risk. No warranty or Guaranty. No support.

Installing and Running
=================================
* Create a folder for the Profiler in the Client and Server Addins folder.
  * Client Addins Folder e.g.: C:\Program Files (x86)\Microsoft Dynamics NAV\80\RoleTailored Client\Add-ins
  * Server Addins Folder on the Service Tier Server e.g.: C:\Program Files\Microsoft Dynamics NAV\80\Service\Add-ins
* Copy all files from the project output (~\EtwPerformanceProfiler\bin\Debug) into the Addins folders.
* Import and compile the application objects from the **App Objects** folder.
* Open the **Microsoft Dynamics NAV 2013 R2 Administration** or **Microsoft Dynamics NAV 2015 Administration** on the Server and change **Enable Full C/AL Function Tracing** to **Yes**. This specifies whether full C/AL method tracing is enabled when an ETW session is performed.
* Restart the Service Tier.
* Run the Page 50000.

If you want to analyze ETL file here is an article, which describes how to collect ETL file for Dynamics NAV.
http://msdn.microsoft.com/en-us/library/dn271709(v=nav.71).aspx
