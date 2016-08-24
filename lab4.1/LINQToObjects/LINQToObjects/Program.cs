using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace LINQToObjects
{
    public static class CopyToExtension
    {
        public static void CopyTo(this object currentObject,object @object)
        {
            (from prop1 in currentObject.GetType().GetProperties()
                  where prop1.CanRead
            join prop2 in @object.GetType().GetProperties() on prop1.Name equals prop2.Name
                  where prop2.CanWrite
            select new {curr=prop1,dist=prop2}).ToList().ForEach((element) =>
            {
                element.dist.SetValue(@object,element.curr.GetValue(currentObject));
            });
        }
    }

    public class Program
    {
        private static readonly IEnumerable<Process> Processes = Process.GetProcesses();
        private static void Main()
        {
            try
            {
                DisplayAllPublicInterfaces();
                DisplayAllProcessesRunning();
                DisplayWithDroupingByBasePriority();
                DisplayTotalThreadsNumber();
    
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
            }
            //Where is CopyTo Example ???
        }

        private static void DisplayAllPublicInterfaces()
        {
            Console.WriteLine("------------------------------a-----------------------------");
            Console.WriteLine();
            var mscorlib = Assembly.Load("mscorlib");
            (from type in mscorlib.GetTypes()
             where type.IsInterface && type.IsPublic
             orderby type.Name
             select new { Interface = type.Name, methodsNum = type.GetMethods().Count() }).ToList().ForEach(Console.WriteLine);
        }

        private static void DisplayAllProcessesRunning()
        {
            //.. When I'm trying to get the process start time I got access denied => See below the answer
            Console.WriteLine();
            Console.WriteLine("------------------------------b-----------------------------");
            (from process in Processes
             where process.Threads.Count <= 5 // && CanAccess(process)
             orderby process.Id
             select new { process.ProcessName, ProcessId = process.Id/*,ProcessStartTime=process.StartTime */}).ToList().ForEach(Console.WriteLine);
        }
        //public static bool CanAccess(Process processForStartTime)
        //{
        //    try
        //    {
        //        var isExisteStartTime = processForStartTime.StartTime;
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        private static void DisplayWithDroupingByBasePriority()
        {
            Console.WriteLine();
            Console.WriteLine("------------------------------c-----------------------------");
            (from process in Processes
             where process.Threads.Count <= 5
             orderby process.Id
             group process by process.BasePriority).ToList().ForEach((group) =>
             {
                 // Extract to new method and pass "group" as parameter
                 Console.WriteLine($"{group.Key}");
                 foreach (var process in group)
                 {
                     Console.WriteLine($"ProcessName: {process.ProcessName}, ProcessID: {process.Id}");
                 }
             });
        }

        private static void DisplayTotalThreadsNumber()
        {
            Console.WriteLine();
            Console.WriteLine("------------------------------d-----------------------------");
            var threadCount = from process in Processes
                              select (process.Threads.Count);
            Console.WriteLine("...." + threadCount.Sum());
        }
    }
}
