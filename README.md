# AurelsOpenAIClient

**Version:** 1.1.1

AurelsOpenAIClient is a simple .NET library for integrating OpenAI APIs into your applications. It supports Chat Completion, Speech-to-Text, Text-to-Speech, and Translation. You only need a valid OpenAI API key to use it.

---
## Links
- [GitHub - AurelsOpenAIClient](https://github.com/aurel192/AurelsOpenAIClient)
- [NuGet Package - AurelsOpenAIClient](https://www.nuget.org/packages/AurelsOpenAIClient)
---

## Features
- **Models**: List available OpenAI models.  
> Each feature uses a default model; you can override it by calling the SetModel method for the specific client.

- **Chat Completion**: Interact with OpenAI's chat models such as `gpt-5-chat-latest`, `gpt-4o`, `gpt-4o-mini`, `gpt-4.1-mini`, `gpt-3.5-turbo`, and more.
- **Speech-to-Text**: Convert audio files to text using OpenAI's `whisper-1` model.
- **Text-to-Speech**: Generate speech from text with customizable voice and speed; uses `tts-1` by default.
- **Translation**: Translate audio files into English using OpenAI's `whisper-1` model.

---

#### Check out the audio library if you want to record and play audio

[NuGet Package - AurelsAudioLibrary](https://www.nuget.org/packages/AurelsAudioLibrary)

---

## Installation

Add AurelsOpenAIClient to your project via CLI or the NuGet Package Manager:

```
dotnet add package AurelsOpenAIClient --version 1.1.1
```

Make sure you have a valid OpenAI API key. You can top up your OpenAI credit with as little as 5 USD:
[OpenAI Billing](https://platform.openai.com/settings/organization/billing/overview)

---
## Models
```csharp
var models = new Models("YOUR-OPENAI-API-KEY");
string availableModels = await models.GetModels();
```
---

## Chat Completion

The `ChatCompletion` class enables interaction with OpenAI's chat models and offers many customization options. You can define a system role to influence model behavior, limit which previous Q&A pairs are included by specifying keywords, and control how many previous Q&A pairs are remembered. You can also manually assemble the message list or provide all parameters directly.

```csharp
var chatClient = new ChatCompletions("YOUR-OPENAI-API-KEY");

// SendChat method has 4 method overloads. Use the one that suits your needs.
string response = await chatClient.SendChat("What is the meaning of life?");

// In the example below with the second parameter, it will remember previous 5 Q&As
string response = await chatClient.SendChat(input, numberOfPreviousQA: 5);

// In this example the messages array will contain only the Q&A pairs that are related to the second parameter.
// Only those Q&A pairs will be in the messages array that contained "nvidia" or "tsm". It is case insensitive.
string response = await chatClient.SendChat(input, keywords: new List<string>{"Nvidia", "TSM" } );

// The messages parameter is a list of Q&A pairs and the current question.
List<ChatCompletionsMessage> messages = ...
string response = SendChat(messages);

// If you want to manually assamble all the parameters
string response = await chatClient.SendChatAdvanced(chatCompletetionParameters: allParameters);
```

---

```csharp

// All the important parameters can be modified:

chatClient.SetEndpoint("https://api.openai.com/v1/chat/completions");
chatClient.SetModel("gpt-4o-mini");
chatClient.SetMaxTokens(5000);
chatClient.SetSystemRole("You are a professional accountant, and lawyer");
chatClient.SetTemperature(0.7);
chatClient.ClearPreviousQuestionAndAnswerPairs();
```

---

```csharp
// You have the ability to get the whole response, not just the string response

ChatResponse chatResponse = chatClient.GetFullChatResponse();

// Use these functions if you would like to know more additional information of the request and response

int tokensTotal = chatClient.GetTotalTokens();
int tokensPromt = chatClient.GetPromtTokens();
int tokensCompletion = chatClient.GetCompletionTokens();

string jsonRequest = chatClient.GetJsonRequest();
string jsonResponse = chatClient.GetJsonResponse();
string responseTime = chatClient.GetResponseTimeMs();

```

---

## Text-to-Speech

The `TextToSpeech` class generates speech from text. You can customize voice, speed, and the output file.

```csharp
var textToSpeech = new TextToSpeech("YOUR-OPENAI-API-KEY",);
textToSpeech.SetFilePath("Speech.mp3"); // Optional

// returns the path to the generated audiofile.
string response = await textToSpeech.GetResponse(text: "You will hear this sentence!");

```

---

## Speech-to-Text

The `SpeechToText` class converts audio files to text. It uses the `gpt-4o-transcribe` model by default.

```csharp
var speechToText = new SpeechToText("YOUR-OPENAI-API-KEY");
// string speech variable contains the transcribed text
string speech = await speechToText.Transcribe("RecordedVoice.mp3");
```

---

## Create translation

The `Translate` class translates audio files into English. It uses the `whisper-1` model by default.

```csharp
var translate = new Translate("YOUR-OPENAI-API-KEY");
// string englishText variable contains the translated text
string englishText = await Translate.GetResponse("RecordedForeignAudio.mp3");
```

---

## Image generation
WIP — coming soon!

---

## CLI
- **Console application**: Useful as a simple CLI or for file redirection.  
> Settings are stored in a settings.json file and can be overridden with command-line parameters.  
You can set important parameters from the command prompt / terminal:  
-k ApiKey  
-m Model  
-s System role  
-t Temperature  
-q User input (prompt)  

Multiplatform: Works on Windows, Linux and macOS.

**Usage examples:**

>dotnet oai.dll -q "Say Hello!"  
dotnet oai.dll -q "Why is the sun red at sunset?" -m gpt-4o-mini -k YOUR_API_KEY  
dotnet oai.dll < input.txt > out.txt  
dotnet oai.dll input.txt -s "Do the opposite!"   
(input.txt contains "say hello" → with system role "do the opposite", the answer will be "goodbye".)

[GitHub - AurelsOpenAIClient - CLI App](https://github.com/aurel192/AurelsOpenAIClient/tree/main/oai)

---

## License

This library is licensed under the MIT License. See the LICENSE file for details.

---

## Contributing

Contributions are welcome. Please submit issues or pull requests on the [AurelsOpenAIClient GitHub repository](https://github.com/aurel192/AurelsOpenAIClient)
