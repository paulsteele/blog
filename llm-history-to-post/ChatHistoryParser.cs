using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LlmHistoryToPost
{
    public class ChatHistoryParser
    {
        private static readonly Regex TimestampRegex = new Regex(@"# aider chat started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})");
        private static readonly Regex UserPromptRegex = new Regex(@"^####\s+(.*)$", RegexOptions.Multiline);

        public async Task<ChatHistory> ParseAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var chatHistory = new ChatHistory();
            
            // Split the content by timestamp sections
            var sections = TimestampRegex.Split(content);
            
            DateTime currentTimestamp = DateTime.MinValue;
            
            for (int i = 1; i < sections.Length; i += 2)
            {
                // Parse the timestamp
                if (DateTime.TryParse(sections[i], out DateTime timestamp))
                {
                    currentTimestamp = timestamp;
                }
                
                string sectionContent = sections[i + 1];
                
                // Split by user prompts
                var promptMatches = UserPromptRegex.Matches(sectionContent);
                
                if (promptMatches.Count > 0)
                {
                    for (int j = 0; j < promptMatches.Count; j++)
                    {
                        var match = promptMatches[j];
                        int startIndex = match.Index;
                        int endIndex = (j < promptMatches.Count - 1) ? promptMatches[j + 1].Index : sectionContent.Length;
                        
                        string prompt = match.Groups[1].Value.Trim();
                        string response = sectionContent.Substring(
                            startIndex + match.Length, 
                            endIndex - (startIndex + match.Length)
                        ).Trim();
                        
                        chatHistory.Conversations.Add(new Conversation
                        {
                            Prompt = prompt,
                            Response = response,
                            Timestamp = currentTimestamp
                        });
                    }
                }
            }
            
            return chatHistory;
        }
    }
}
