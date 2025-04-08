namespace LlmHistoryToPost.Models;

public class ChatHistory
{
	public List<ChatSession> Sessions { get; } = [];
	public Dictionary<DateOnly, List<PromptResponsePair>> PromptsByDay { get; } = new();
}
