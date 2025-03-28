# High Level Goal for "llm-history-to-post"

llm-history-to-post is an application that will parse llm chat logs and create blog post templates based on user input.

# Requirements
* The application should interact over the cli
* The application should parse a ".aider.chat.history.md" into an in memory collection of the user entered prompt and the llm output response
  * This parsed input should be logically organized by day
* When the application is run the user should be able to select which singular day they would like to process
* After the user has selected the desired day, the user should be presented a list of the parsed prompts where they can choose to select which prompts / responses to populate into the end result
  * The user should be able to select any number of prompts to process while skipping others.
    * This should be a multi select list offered like SharPrompt
  * The user should select the prompt response pair by only previewing the prompt itself.
* Once selected, each prompt response pair should be represented to the user and then asked if verdict was pass or failure. This input should make it into the blog.
* Once all the questions have been answered the application should output a new blog post that was constructed from a source template