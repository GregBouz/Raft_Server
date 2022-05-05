using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raft.Node
{
    public class MessageLog
    {
        private Dictionary<int, LogEntry> _logEntries;

        public void AddLogEntry(int term, string message)
        {
            if (!_logEntries.ContainsKey(term))
                _logEntries.Add(term, new LogEntry() { Term = term, Message = message});
        }
    }
}
