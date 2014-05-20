using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace TraceUtil
{
    class Program
    {
        private static int SessionIdPayloadIndex = 1;
        static void Main(string[] args)
        {
            int sessionId = Int32.Parse(args[0]);
            string fileName = args[1];

            // Open the file
            using (var source = new ETWTraceEventSource(fileName))
            {
                using (StreamWriter file = new StreamWriter(fileName + "." + sessionId + ".dump.txt"))
                {
                    // DynamicTraceEventParser knows about EventSourceEvents
                    var parser = new DynamicTraceEventParser(source);
                    // Set up a callback for every event that prints the event
                    parser.All += delegate(TraceEvent traceEvent)
                    {
                        int currentSessionId = (int) traceEvent.PayloadValue(SessionIdPayloadIndex);
                        if (sessionId == currentSessionId)
                        {
                            file.WriteLine(traceEvent.ToString());
                        }
                    };
                    // Read the file, processing the callbacks.
                    source.Process();
                }
                // Close the file.
            }  
        }
    }
}
