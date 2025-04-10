namespace LlmHistoryToPost.Services;

using System.Text.RegularExpressions;
using LlmHistoryToPost.Models;

public partial class ChatHistoryParser
{
	private static readonly Regex SessionStartRegex = AiderChatRegex();
	private static readonly Regex UserPromptRegex = UserRegex();
	
	public ChatHistory ParseHistoryContent(string content)
	{
		var history = new ChatHistory();
		
		// Parse sessions
		var sessionMatches = SessionStartRegex.Matches(content);
		history.Sessions.AddRange(ParseSessions(content, sessionMatches));
		
		// Group by day
		foreach (var session in history.Sessions)
		{
			var dateOnly = DateOnly.FromDateTime(session.StartTime);
			
			if (!history.PromptsByDay.TryGetValue(dateOnly, out var value))
			{
				value = []; 
				history.PromptsByDay[dateOnly] = value;
			}

            value.AddRange(session.PromptResponsePairs);
		}
		
		return history;
	}
	
	private void ParsePromptResponsePairs(string sessionContent, ChatSession session)
	{
		var promptMatches = UserPromptRegex.Matches(sessionContent);
		if (promptMatches.Count == 0)
		{
			return;
		}
		
		for (var i = 0; i < promptMatches.Count;)
		{
			// Process a prompt group (consecutive prompts)
			var (combinedPrompt, lastPromptIndex) = ExtractPromptGroup(promptMatches, sessionContent, i);
			
			// Extract the response that follows this prompt group
			var response = ExtractResponse(promptMatches, sessionContent, lastPromptIndex);
			
			// Create and add the prompt-response pair
			session.PromptResponsePairs.Add(new PromptResponsePair
			{
				Prompt = string.Join("\n", combinedPrompt),
				Response = response
			});
			
			// Move to the next prompt group
			i = lastPromptIndex + 1;
		}
	}
	
	private (List<string> CombinedPrompt, int LastIndex) ExtractPromptGroup(
		MatchCollection promptMatches, string sessionContent, int startIndex)
	{
		var combinedPrompt = new List<string> { promptMatches[startIndex].Groups[1].Value.Trim() };
		var lastIndex = startIndex;
		
		for (var j = startIndex + 1; j < promptMatches.Count; j++)
		{
			if (!IsConsecutivePrompt(sessionContent, promptMatches[j-1], promptMatches[j]))
			{
				break;
			}
			
			combinedPrompt.Add(promptMatches[j].Groups[1].Value.Trim());
			lastIndex = j;
		}
		
		return (combinedPrompt, lastIndex);
	}
	
	private string ExtractResponse(MatchCollection promptMatches, string sessionContent, int lastPromptIndex)
	{
		var lastPromptMatch = promptMatches[lastPromptIndex];
		var responseStartIndex = lastPromptMatch.Index + lastPromptMatch.Length;
		
		var responseEndIndex = lastPromptIndex < promptMatches.Count - 1 
			? promptMatches[lastPromptIndex + 1].Index 
			: sessionContent.Length;
		
		return sessionContent[responseStartIndex..responseEndIndex].Trim();
	}
	
	private bool IsConsecutivePrompt(string content, Match current, Match next)
	{
		// Calculate the text between the end of the current match and the start of the next match
		var endOfCurrentLine = current.Index + current.Length;
		var startOfNextMatch = next.Index;
		
		// Extract the text between the two matches
		var textBetween = content.Substring(endOfCurrentLine, startOfNextMatch - endOfCurrentLine);
		
		// If there are only newlines and whitespace between matches, they're consecutive
		return textBetween.Trim().Length == 0;
	}

	private List<ChatSession> ParseSessions(string content, MatchCollection sessionMatches)
	{
		var sessions = new List<ChatSession>();
		
		for (var i = 0; i < sessionMatches.Count; i++)
		{
			var sessionMatch = sessionMatches[i];
			var startTimeStr = sessionMatch.Groups[1].Value;
			var startTime = DateTime.Parse(startTimeStr);
			
			var session = new ChatSession
			{
				StartTime = startTime
			};
			
			// Determine the content of this session
			var startIndex = sessionMatch.Index + sessionMatch.Length;
			var endIndex = (i < sessionMatches.Count - 1) 
				? sessionMatches[i + 1].Index 
				: content.Length;
			
			var sessionContent = content.Substring(startIndex, endIndex - startIndex);
			
			// Parse prompt-response pairs
			ParsePromptResponsePairs(sessionContent, session);
			
			sessions.Add(session);
		}
		
		return sessions;
	}
	
	[GeneratedRegex(@"# aider chat started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})", RegexOptions.Compiled)] 
	private static partial Regex AiderChatRegex(); 
	[GeneratedRegex(@"^####\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled)] 
	private static partial Regex UserRegex();
}
