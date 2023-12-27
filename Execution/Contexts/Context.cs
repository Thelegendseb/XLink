using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLink.Actions;
using XLink.Utility;

namespace XLink.Context
{
    // summary: The base class for all contexts
    public abstract class Context
    {

        // summary: The name of the context
        protected string Name = "EmptyContext";

        // summary: The type of the context
        protected ContextType ContextType = ContextType.Misc;

        // summary: The actions for the context
        protected Dictionary<XAction.RequestSchema, XAction.ResponseSchema> Actions;

        // summary: Whether or not an action is running
        protected bool actionRunning = false;

        // summary: The config values for the context
        private Dictionary<string, string> ConfigValues;

        // summary: Initialize the context
        protected abstract void Init();

        // summary: Load the config values for the context
        protected abstract Dictionary<string, string> LoadConfigValues(IConfigurationSection section);

        // summary: Load the actions for the context
        protected abstract Dictionary<XAction.RequestSchema, XAction.ResponseSchema> LoadActions();

        // summary: Constructor for the context
        // param: string name - the name of the context
        // param: ContextType type - the type of the context
        public Context(string name, ContextType type)
        {
            this.Name = name;
            this.ContextType = type;
            this.ConfigValues = this.LoadConfigValues(AppConfiguration.Instance.Configuration.GetSection(this.Name));
            this.Init();
            this.Actions = this.LoadActions();
        }


        // summary: Run an action from the context
        // param: string actionName - the name of the action to run
        // param: string query - the query to run the action with
        // returns: bool - whether or not the action was successful
        public XActionResponse RunAction(string actionName, string query)
        {
            if (Actions.Keys.Any(action => action.Name == actionName))
            {
                XAction.ResponseSchema action = this.Actions.First(x => x.Key.Name == actionName).Value;
                this.actionRunning = true;
                XActionResponse result = action(query);
                this.actionRunning = false;
                return result;
            }
            else
            {
                return new XActionResponse(this.GetName(), actionName, query, false, "Action not found in context '" + this.Name + "'.", "");
            }
        }

        // ==================== Getters ====================

        // summary: Get a config value from the context
        // param: string key - the key of the config value to get
        // returns: string - the config value
        protected string GetConfigValue(string key)
        {
            return this.ConfigValues[key];
        }

        // summary: Get the name of the context
        // returns: string - the name of the context
        public string GetName()
        {
            return this.Name;
        }

        // summary: Get the type of the context
        // returns: ContextType - the type of the context
        public ContextType GetContextType()
        {
            return this.ContextType;
        }

        // summary: Get the actions for the context
        // returns: Dictionary<string, XAction.Schema> - the actions for the context
        public Dictionary<XAction.RequestSchema, XAction.ResponseSchema> GetActions()
        {
            return this.Actions;
        }
    }

}
