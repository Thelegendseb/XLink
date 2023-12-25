using Microsoft.Extensions.Configuration;

using XLink.Utility;
using XLink.Actions;
using XLink.Context.Contexts;

namespace XLink.Core
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var context = new con_Spotify("Spotify");

            //Dictionary<string,XAction.Schema> actions = context.GetActions();

            //for (int i = 0; i < actions.Count; i++)
            //{
            //    Console.WriteLine(actions.ElementAt(i).Key);
            //}

            //Console.WriteLine();
            //Console.WriteLine();

            while (true)
            {

                string command = Console.ReadLine();
                string[] command_parts = command.Split(" ");
                string action = command_parts[0];
                string query = command_parts.Length > 1 ? command_parts[1] : "";
                XActionResponse result = context.RunAction(action, query);
                ClearCurrentConsoleLine();

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

