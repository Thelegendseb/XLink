using Microsoft.Extensions.Configuration;

using XLink.Utility;
using XLink.Actions;
using XLink.Context.Contexts;
using Execution.Core;
using Microsoft.Extensions.Primitives;

namespace XLink.Core
{

    public class Program
    {
        public static void Main()
        {

            ContextManager contextManager = new ContextManager();

            contextManager.Init();

            Console.WriteLine();
            foreach((string,string) kvp in contextManager.GetActions())
            {
                Console.WriteLine(kvp.Item1 + " " + kvp.Item2);
            }
            Console.WriteLine();

            while (true)
            {

                string command = Console.ReadLine();
                ClearCurrentConsoleLine();
                string[] command_parts = command.Split(" ");
                string context_name = command_parts[0];
                string action = command_parts[1];
                string args = command_parts.Length > 2 ? command_parts[2] : "";
                XActionResponse result = contextManager.Execute(context_name, action, args);

                if(result == null)
                {
                    continue;
                }

                if (result.Result != null)
                {
                    if (result.Result.Contains("\n"))
                    {
                        result.Result = result.Result.Substring(0, result.Result.IndexOf("\n")) + "...";
                        if (result.Result.Length > 15)
                        {
                            result.Result = result.Result.Substring(0, 15) + "...";
                        }
                    }
                }

                Logger.Log(result);

                Console.WriteLine();
            }

            static void ClearCurrentConsoleLine()
            {
                int currentLineCursor = Console.CursorTop - 1;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);
            }


        }

    }

}

