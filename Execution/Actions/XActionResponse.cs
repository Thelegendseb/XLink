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

        // summary: The name of the action
        public string Name { get; set; }

        // summary: The query used to run the action
        public string Query { get; set; }

        // summary: The result of the action
        public string Result { get; set; }

        // summary: Whether or not the action was successful
        public bool Success { get; set; }

        // summary: The error message if the action was not successful
        public string ErrorMessage { get; set; }

        // summary: Constructor for the action response
        // param: string name - the name of the action
        // param: string query - the query used to run the action
        // param: string result - the result of the action
        // param: bool success - whether or not the action was successful
        // param: string errorMessage - the error message if the action was not successful
        public XActionResponse(string name,
                              string query,
                              bool success,
                              string errorMessage,
                              string result = "This action does not return a result.")
        {
            this.Name = name;
            this.Query = query;
            this.Result = result;
            this.Success = success;
            this.ErrorMessage = errorMessage;
        }

        // summary: Overriden ToString method
        public override string ToString()
        {
            return $"Action: {this.Name} Query: {this.Query} | Result: {this.Result} | Success: {this.Success} | ErrorMessage: {this.ErrorMessage}";
        }


    }
}
