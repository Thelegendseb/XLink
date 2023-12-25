using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XLink.Actions;

namespace XLink.Utility
{

    // summary: Logger class for debug statements with additional information

    // summary: The levels of logs
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class Logger
    {

        // summary: Log a message to the console
        // param: string message - the message to log
        public static void Log(string message)
        {
           // change console color to white
           Console.ForegroundColor = ConsoleColor.White;
           Console.Write("[" + DateTime.Now.ToString() + "]");
           Console.Write(" [" + message + "]\n");

        }

        // summary: Log a message to the console with a type
        // param: string message - the message to log
        // param: LogType type - the type of the log
        public static void Log(string message,LogLevel type)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[" + DateTime.Now.ToString() + "] ");
            switch (type)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[INFO]");
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[WARNING]");
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR]");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" [" + message + "]\n");

        }

        public static void Log(XActionResponse actionresponse)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[" + DateTime.Now.ToString() + "] ");
            if(actionresponse.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[SUCCESS]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR]");
            }
 
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" [" + actionresponse + "] ");

        }

    }
}
