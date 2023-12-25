using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLink.Actions
{
    // summary: Class for all action based operations
    public abstract class XAction
    {

        // summary: The schema for an action
        // param: string query - the query to run the action with
        // returns: bool - whether or not the action was successful
        public delegate XActionResponse Schema(string query);

    }
}
