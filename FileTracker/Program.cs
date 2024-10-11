using System;
using System.IO;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Security.Permissions;


/**
 * Set watches for directory and *.ini files 
 * In case of change - copy file and change it name to filename_datetime.ext
 */
  
public class Watcher
{
    private static String machine;
    private static FileSystemWatcher watcher;
    public static void Main()
    {
        Run();

    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public static void Run()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        // If a directory is not specified, exit program. 
        if (args.Length != 3)
        {
            // Display the proper way to call the program.
            Console.WriteLine("Usage: Watcher.exe (directory) (machine)");
           return;
        }

        // Create a new FileSystemWatcher and set its properties.
        watcher = new FileSystemWatcher();

        bool connected = false;

        // handle disconnection - reconnect after 30s

        while (!connected)
        {
            try
            {
                watcher.Path = args[1];
                machine = args[2];
                connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection timeout");
                System.Threading.Thread.Sleep(30000);
                watcher = new FileSystemWatcher();
            }
        }

        if (!Directory.Exists(machine)) Directory.CreateDirectory(machine);

        /* Watch for changes in LastAccess and LastWrite times, and the renaming of files . */
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        // Only watch text files.
        watcher.Filter = "*.ini";
        
        // Add event handlers.
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.Error += new ErrorEventHandler(OnError);

        // Begin watching.
        watcher.EnableRaisingEvents = true;

        // Wait for the user to quit the program.
        Console.WriteLine("Press \'q\' to quit the sample.");
        while (Console.Read() != 'q')
        { 
             
        }
    }

    // Define the event handlers. 
    private static void OnChanged(object source, FileSystemEventArgs e)
    {
        // Specify what is done when a file is changed
        
        //Exclude logfile
        if ("logdata.ini" == e.Name) return;

        Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        try
        {
            File.Copy(e.FullPath, machine +"/" +Path.GetFileNameWithoutExtension(e.FullPath) + "_" + string.Format("{0:yyyy-MM-dd_hh-mm}", DateTime.Now) + Path.GetExtension(e.FullPath));
        }
        catch (Exception ex)
        {

        }
    }

    //  This method is called when the FileSystemWatcher detects an error. 
    private static void OnError(object source, ErrorEventArgs e)
    {
        //  Show that an error has been detected.
        Console.WriteLine("The FileSystemWatcher has detected an error");
        //  Give more information if the error is due to an internal buffer overflow. 
        if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
        {
            //  This can happen if Windows is reporting many file system events quickly  
            //  and internal buffer of the  FileSystemWatcher is not large enough to handle this 
            //  rate of events. The InternalBufferOverflowException error informs the application 
            //  that some of the file system events are being lost.
            Console.WriteLine(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
        }
        NotAccessibleError(watcher, e);
    }

    // handle disconnection - reconnect after 30s
    static void NotAccessibleError(FileSystemWatcher source, ErrorEventArgs e)
    {
        source.EnableRaisingEvents = false;
        int iTimeOut = 30000;
        while (source.EnableRaisingEvents == false )
        {
            try
            {
                source.EnableRaisingEvents = true;
            }
            catch
            {
                source.EnableRaisingEvents = false;
                System.Threading.Thread.Sleep(iTimeOut);
            }
        }

    }
}
