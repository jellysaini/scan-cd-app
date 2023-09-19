using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace ScanCD
{
    class Program
    {
        static void Main(string[] args)
        {
            Program we = new Program();
            ManagementEventWatcher w = null;
            WqlEventQuery q;
            ManagementOperationObserver observer = new
                ManagementOperationObserver();

            // Bind to local machine
            ConnectionOptions opt = new ConnectionOptions();
            opt.EnablePrivileges = true; //sets required privilege
            ManagementScope scope = new ManagementScope("root\\CIMV2", opt);

            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceModificationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 1);

                // DriveType - 5: CDROM
                q.Condition = @"TargetInstance ISA 'Win32_LogicalDisk' and
            TargetInstance.DriveType = 5";
                w = new ManagementEventWatcher(scope, q);

                // register async. event handler
                w.EventArrived += new EventArrivedEventHandler(we.CDREventArrived);
                w.Start();

                // Do something usefull,block thread for testing
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                w.Stop();
            }
        }

        // Dump all properties
        public void CDREventArrived(object sender, EventArrivedEventArgs e)
        {
            // Get the Event object and display it
            PropertyData pd = e.NewEvent.Properties["TargetInstance"];

            if (pd != null)
            {
                ManagementBaseObject mbo = pd.Value as ManagementBaseObject;

                // if CD removed VolumeName == null
                if (mbo.Properties["VolumeName"].Value != null)
                {
                    Console.WriteLine("CD has been inserted");
                }
                else
                {
                    Console.WriteLine("CD has been ejected");
                }
            }
        }
    }
}
