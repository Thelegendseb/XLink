using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XLink.Actions;
using XLink.Context;
using XLink.Context.Contexts;
using XLink.Utility;

namespace Execution.Core
{
    // summary: A class to manage all the running contexts and their executions
    public class ContextManager
    {

        // summary: The list of contexts
        private List<Context> Contexts;

        // summary: Constructor for the context manager
        public ContextManager()
        {
            this.Contexts = new List<Context>();
        }

        // summary: The initialization function for the context manager
        public void Init()
        {
            LoadContexts();
        }

        // summary: Load all the contexts
        private void LoadContexts()
        {

            AddContext<con_Spotify>();
            Logger.Log("Spotify Context loaded.", LogLevel.Info);
           

            Logger.Log("Contexts loaded.", LogLevel.Info);

        }

        // summary: Execute an action
        // param: string actionName - the name of the action to execute
        // param: string args - the args to execute the action with
        // returns: XActionResponse - the response from the action
        public XActionResponse Execute(string contextName, string actionName, string args)
        {

             Context context = this.Contexts.Find(x => x.GetName() == contextName);
            if (context == null)
            {
                Logger.Log("Context not found: " + contextName, LogLevel.Info);
                return null;
            }
            else
            {
                return context.RunAction(actionName, args);
            }

        }

        // summary: attempting to add a single context to the context manager
        // param: TContext - the context to add
        // returns: bool - whether or not the context was added
        private bool AddContext<TContext>() where TContext : Context, new()
        {
            try
            {
                TContext context = new TContext();

                if (this.Contexts.Contains(context))
                {
                    return false;
                }
                else
                {
                    this.Contexts.Add(context);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error adding context: " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        // summary: get a list of all the contexts's actions and the context they belong to
        // returns: Dictionary<string,List<string>> - the list of contexts and their actions
        public Dictionary<string, List<string>> GetActions()
        {
            Dictionary<string, List<string>> actions = new Dictionary<string, List<string>>();
            foreach (Context context in this.Contexts)
            {
                actions.Add(context.GetName(), context.GetActions().Keys.Select(x => x.Name).ToList());
            }
            return actions;
        }

    }
}
