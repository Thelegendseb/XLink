using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLink.Actions
{
    // summary: The response from an action
    public class XActionResponse
    {
        // summary: the name of the context
        public string ContextName { get; set; }

        // summary: The name of the action
        public string ActionName { get; set; }

        // summary: The arguments used to run the action
        public string Args { get; set; }

        // summary: The result of the action
        public string Result { get; set; }

        // summary: Whether or not the action was successful
        public bool Success { get; set; }

        // summary: The error message if the action was not successful
        public string ErrorMessage { get; set; }

        public XActionResponse(string contextname,
                              string actionname,
                              string args,
                              bool success,
                              string errorMessage,
                              string result = "This action does not return a result.")
        {
            this.ContextName = contextname;
            this.ActionName = actionname;
            this.Args = args;
            this.Result = result;
            this.Success = success;
            this.ErrorMessage = errorMessage;
        }

        // summary: Overriden ToString method
        public override string ToString()
        {
            return $"Context: {this.ContextName} | Action: {this.ActionName} | Args: {this.Args} | Result: {this.Result} | Success: {this.Success} | ErrorMessage: {this.ErrorMessage}";
        }


    }
}
