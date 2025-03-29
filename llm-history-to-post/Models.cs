using System;
using System.Collections.Generic;
using System.Linq;

namespace LlmHistoryToPost
{
    public class Conversation
    {
        public string Prompt { get; set; }
        public string Response { get; set; }
        public DateTime Timestamp { get; set; }
        public Verdict Verdict { get; set; }
    }

    public class Verdict
    {
        public bool IsSuccess { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return $"Verdict: {(IsSuccess ? "✅" : "❌")} {Comment}";
        }
    }

    public class DayGroup
    {
        public DateTime Date { get; set; }
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
    }

    public class ChatHistory
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();

        public List<DayGroup> GroupByDay()
        {
            return Conversations
                .GroupBy(c => c.Timestamp.Date)
                .Select(g => new DayGroup
                {
                    Date = g.Key,
                    Conversations = g.ToList()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }
    }
}
