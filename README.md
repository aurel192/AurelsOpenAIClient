# AurelsOpenAIClient

**Version:** 1.1.3

AurelsOpenAIClient is a simple .NET library for integrating OpenAI APIs into your applications. It supports Chat Completion, Speech-to-Text, Text-to-Speech, Translation, and Image Generation. You only need a valid OpenAI API key to use it.

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
- **Image Generation**: Generate images from text prompts using OpenAI's image generation models.

- **I am working on other useful endpoints.  
If you're interested in this easy to use client check the [NuGet Package](https://www.nuget.org/packages/AurelsOpenAIClient)  or the [GitHub repository](https://github.com/aurel192/AurelsOpenAIClient) weekly**
---
#### Check out the audio library if you want to record and play audio

[NuGet Package - AurelsAudioLibrary](https://www.nuget.org/packages/AurelsAudioLibrary)

---
## Installation

Add AurelsOpenAIClient to your project via CLI or the NuGet Package Manager:

```
dotnet add package AurelsOpenAIClient --version 1.1.3
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
string response = await chatClient.SendChat(input, keywords: new List<string>{"Nvidia", "TSM" });


// The messages parameter is a list of Q&A pairs and the current question.
List<ChatCompletionsMessage> messages = ...
string response = SendChat(messages);

// If you want to manually assamble all the parameters
string response = await chatClient.SendChatAdvanced(chatCompletetionParameters: allParameters);
```

---

All the important parameters can be modified:  
```csharp
chatClient.SetEndpoint("https://api.openai.com/v1/chat/completions");
chatClient.SetModel("gpt-4o-mini");
chatClient.SetMaxTokens(5000);
chatClient.SetSystemRole("You are a professional accountant, and lawyer");
chatClient.SetTemperature(0.7);
chatClient.ClearPreviousQuestionAndAnswerPairs();
```

---

 You have the ability to get the whole response, not just the string response.  
Use these functions if you would like to know more additional information of the request and response.  
```csharp
ChatResponse chatResponse = chatClient.GetFullChatResponse();
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
var textToSpeech = new TextToSpeech("YOUR-OPENAI-API-KEY");
textToSpeech.SetFilePath("Speech.mp3"); // Optional
string response = await textToSpeech.GetResponse(text: "You will hear this sentence!");
```

---
## Speech-to-Text

The `SpeechToText` class converts audio files to text. It uses the `gpt-4o-transcribe` model by default.

```csharp
var speechToText = new SpeechToText("YOUR-OPENAI-API-KEY");
string speech = await speechToText.Transcribe("RecordedVoice.mp3");
```

---
## Create translation

The `Translate` class translates audio files into English. It uses the `whisper-1` model by default.

```csharp
var translate = new Translate("YOUR-OPENAI-API-KEY");
string englishText = await Translate.GetResponse("RecordedForeignAudio.mp3");
```

---
## Image Generation

The `GenerateImage` class generates images from text prompts.  
You can customize the model, size, and output file.
You must be verified to use the model `gpt-image-1`.  
Go to: https://platform.openai.com/settings/organization/general  
Do the verification on your mobile phone or laptop because the it requires a camera.

```csharp
var imageClient = new GenerateImage("YOUR-OPENAI-API-KEY");
createImageClient.SetModel("dall-e-3");
string[] imageFilesPath = await imageClient.Generate("two cute smiling gecko running towards you. forest in the background.", "GeneratedImage.png", "1024x1024", 1);
```
You can also generate multiple images at once using the 'gpt-image-1' by specifying the `n` parameter:

Resolution:
>The size of the generated images. Must be one of 1024x1024, 1536x1024 (landscape), 1024x1536 (portrait), or auto (default value) for gpt-image-1, one of 256x256, 512x512, or 1024x1024 for dall-e-2, and one of 1024x1024, 1792x1024, or 1024x1792 for dall-e-3

For more info: https://platform.openai.com/docs/api-reference/images/create

```csharp
generateImageClient.SetModel("gpt-image-1"); // much better than dall-e
string[] imageFilesPath = await imageClient.Generate(
    prompt: "A cat sleeping in the livingroom, photorealistic",
    outputFileName: "SleepyCat.png",
    size: "1024x1024",
    n: 3);
```  

This will generate 3 images from the same prompt and save them as `SleepyCat_1.png`, `SleepyCat_2.png`, etc.
 
> See the generated images:  
[Generated image (SleepyCat_1.png)](http://collectioninventory.com/ai/SleepyCat_1.png)  
[Generated image (SleepyCat_2.png)](http://collectioninventory.com/ai/SleepyCat_2.png)  
[Generated image (SleepyCat_3.png)](http://collectioninventory.com/ai/SleepyCat_3.png)  

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
## Useful Link on OpenAI
https://platform.openai.com/settings/organization/billing/overview  
https://platform.openai.com/settings/organization/limits  
https://platform.openai.com/settings/organization/usage

## License

This library is licensed under the MIT License. See the LICENSE file for details.

---
## Contributing

Contributions are welcome. Please submit issues or pull requests on the [AurelsOpenAIClient GitHub repository](https://github.com/aurel192/AurelsOpenAIClient)
