namespace LlmHistoryToPost.Services;

using System.Text.RegularExpressions;
using LlmHistoryToPost.Models;

public partial class ChatHistoryParser
{
	private static readonly Regex SessionStartRegex = AiderChatRegex();
	private static readonly Regex UserPromptRegex = UserRegex();
	
	public ChatHistory ParseHistoryFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Chat history file not found: {filePath}");
		}
		
		var content = File.ReadAllText(filePath);
		var history = new ChatHistory();
		
		// Parse sessions
		var sessionMatches = SessionStartRegex.Matches(content);
		for (int i = 0; i < sessionMatches.Count; i++)
		{
			var sessionMatch = sessionMatches[i];
			var startTimeStr = sessionMatch.Groups[1].Value;
			var startTime = DateTime.Parse(startTimeStr);
			
			var session = new ChatSession
			{
				StartTime = startTime
			};
			
			// Determine the content of this session
			int startIndex = sessionMatch.Index + sessionMatch.Length;
			int endIndex = (i < sessionMatches.Count - 1) 
				? sessionMatches[i + 1].Index 
				: content.Length;
			
			var sessionContent = content.Substring(startIndex, endIndex - startIndex);
			
			// Parse prompt-response pairs
			ParsePromptResponsePairs(sessionContent, session);
			
			history.Sessions.Add(session);
		}
		
		// Group by day
		foreach (var session in history.Sessions)
		{
			var dateOnly = DateOnly.FromDateTime(session.StartTime);
			
			if (!history.PromptsByDay.ContainsKey(dateOnly))
			{
				history.PromptsByDay[dateOnly] = new List<PromptResponsePair>();
			}
			
			history.PromptsByDay[dateOnly].AddRange(session.PromptResponsePairs);
		}
		
		return history;
	}
	
	private void ParsePromptResponsePairs(string sessionContent, ChatSession session)
	{
		var promptMatches = UserPromptRegex.Matches(sessionContent);
		
		for (int i = 0; i < promptMatches.Count; i++)
		{
			var promptMatch = promptMatches[i];
			var prompt = promptMatch.Groups[1].Value.Trim();
			
			// Determine the response (text between this prompt and the next one)
			int responseStartIndex = promptMatch.Index + promptMatch.Length;
			int responseEndIndex = (i < promptMatches.Count - 1) 
				? promptMatches[i + 1].Index 
				: sessionContent.Length;
			
			var response = sessionContent.Substring(responseStartIndex, responseEndIndex - responseStartIndex).Trim();
			
			var pair = new PromptResponsePair
			{
				Prompt = prompt,
				Response = response
			};
			
			session.PromptResponsePairs.Add(pair);
		}
	}

    [GeneratedRegex(@"# aider chat started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})", RegexOptions.Compiled)]
    private static partial Regex AiderChatRegex();
    [GeneratedRegex(@"^####\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex UserRegex();
}
