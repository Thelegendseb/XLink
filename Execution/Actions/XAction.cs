using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLink.Actions
{
    // summary: Classes for all action based operations
    public abstract class XAction
    {

        // summary: The schema for an action
        // param: string query - the query to run the action with
        // returns: bool - whether or not the action was successful
        public delegate XActionResponse ResponseSchema(string query);

        // summary: A class to store metadata about an action
        public class RequestSchema
        { 

            // summary: The name of the action
             public string Name { get; set; }

            // summary: A flag to determine if the action returns a result
            public bool ReturnsResult { get; set; }

            // summary: A flag to determine if the action requires arguments
            public bool RequiresArgs { get; set; }
        
        }


    }

}
