

                                                           High Level Goal

llm-history-to-post is an application that parses LLM chat logs and creates blog post templates based on user input, following the
format seen in the example blog post.


                                                        Detailed Requirements

                                                            CLI Interface

• The application should be invoked via command line: llm-history-to-post [path-to-history-file]
• There are no flags specified
• The input file is stored in ".aider.chat.history.md" which resides in the current directory
• The output file will be specified as the path content/posts/{year}/{month}/{year}-{month}-{day}-hadai-day-.md

                                                        Parsing Chat History

• Parse a ".aider.chat.history.md" file into an in-memory collection of user prompts and LLM responses
• The file format contains sections with:
• User prompts prefixed with "####"
• LLM responses are the gaps in-between prompts
• Group these prompt-response pairs by day based on timestamps in the file. Each line won't have a timestamp but there will be section headings like "# aider chat started at 2025-03-26 19:18:06"
• Each day should be identified by a date in the format YYYY-MM-DD

                                                            Day Selection

• Present the user with a numbered list of available days found in the history file
• Display dates in YYYY-MM-DD format
• If only one day exists, automatically select it
• Allow user to select a single day to process

                                                          Prompt Selection

• After day selection, display a multi-select list (using SharpPrompt or similar) of all prompts from that day
• Show the first 100 characters of each prompt as a preview, with "..." if truncated
• Allow selecting any number of prompt-response pairs for inclusion in the blog post
• Provide an option to view the full prompt before selecting (optional)

                                                         Verdict Collection

• For each selected prompt-response pair:
• Display the full prompt and response to the user
• Ask if the verdict was "pass" or "failure" with a simple prompt: "Was this a success? (Y/N)"
• Store the verdict with the prompt-response pair
• Format verdicts in the blog post as:
• Success: "Verdict: ✅ [user comment]"
• Failure: "Verdict: ❌ [user comment]"

                                                        Blog Post Generation

• Use a template similar to the example blog post (2025-03-27-hadai-day-1.md)
• Include:
• YAML frontmatter with title, date, categories, and tags
• Introduction section (can be static or prompt user for input)
• Selected prompt-response pairs formatted as in the example:
• Prompt in a blockquote with "> Prompt:" prefix
• Response in a blockquote with "> Response:" prefix
• Verdict with emoji (✅/❌) and user comment
• Conclusion section (can be static or prompt user for input)
• Generate the post as a Markdown file with naming convention: YYYY-MM-DD-hadai-day-N.md
• Save to the specified output directory or current directory

                                                           Error Handling

• Gracefully handle invalid or missing history files
• Allow cancellation at any point in the selection process
• Handle empty selections (warn user and confirm before proceeding)
• Validate that the output directory exists and is writable

                                                         Additional Features

• Support for custom templates with variable substitution