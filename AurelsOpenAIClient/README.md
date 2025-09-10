# AurelsOpenAIClient

**Version:** 1.0.9

AurelsOpenAIClient is a simple .NET library for integrating OpenAI APIs into your applications. It supports various functionalities such as Chat Completion, Speech-to-Text, Text-to-Speech, and Translation. All you need is an OpenAI API key.

---

## Features

- **Chat Completion**: Interact with OpenAI's chat models like `gpt5`, `gpt-4o`, `gpt-3.5-turbo`, and more.
- **Speech-to-Text**: Convert audio files into text using OpenAI's `whisper-1` model.
- **Text-to-Speech**: Generate speech from text with customizable voice and speed.
- **Translation**: Translate audio files into English using OpenAI's `whisper-1` model.

---

####  Check out my Audio library if you want to record and play audio

 [Nuget Package - AurelsAudioLibrary](https://www.nuget.org/packages/AurelsAudioLibrary)

---

## Installation

Add the AurelsOpenAIClient library to your project using CLI or via NuGet Package Manager.
```
dotnet add package AurelsOpenAIClient --version 1.0.9
```
Ensure you have a valid OpenAI API key to use the services!
You can top up your OpenAI credit with as little as 5 USD
[OpenAI Billing](https://platform.openai.com/settings/organization/billing/overview)

---

## Chat Completion

The `ChatCompletion` class facilitates interaction with OpenAI's chat models, allowing for customization of parameters to suit your preferences. You have the ability to define the system role, which influences the model's behavior, specify keywords to exclude question and answer pairs that are not needed. The request includes previous question-and-answer pairs tailored to your requirements. Additionally, you can manually adjust previous messages or modify the parameters, even the model as needed.

```csharp
var chatClient = new ChatCompletions("YOUR-OPENAI-API-KEY");
// SendChat method has 4 method overloads. Use the one that suits your needs.

string response = await chatClient.SendChat("What is the meaning of life?");

// In the example below with the second parameter, it will remember previous 5 Q&As
string response = await chatClient.SendChat(input, numberOfPreviousQA: 5);

// In this example the messages array will contain only the Q&A pairs that are related to the second parameter.
// Only those Q&A pairs will be in the messages array that contained "nvidia" or "tsm". It is case insensitive.
string response = await chatClient.SendChat(input, keywords: new List<string>{"Nvidia", "TSM" } );

// If you want to manually assamble all the parameters
string response = await chatClient.SendChatAdvanced(chatCompletetionParameters: allParameters);
```

---

```csharp

// All the important parameters can be modified:

chatClient.SetEndpoint("https://api.openai.com/v1/chat/completions");
chatClient.SetModel("gpt-5-nano");
chatClient.SetMaxTokens(20000);
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

The `TextToSpeech` class generates speech from text. You can customize the voice, speed, and output file.

```csharp
var textToSpeech = new TextToSpeech("YOUR-OPENAI-API-KEY",);
textToSpeech.SetFilePath("Speech.mp3"); // Optional

// returns the path to the generated audiofile.
string response = await textToSpeech.GetResponse(text: "You will hear this sentence!");

```

---

## Speech-to-Text

The `SpeechToText` class converts audio files into text. It uses the `gpt-4o-transcribe` model by default.

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
WIP. Coming soon!

---

## License

This library is licensed under the MIT License. See the LICENSE file for details.

---

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests on the [AurelsOpenAIClient GitHub repository](https://github.com/aurel192/AurelsOpenAIClient)
