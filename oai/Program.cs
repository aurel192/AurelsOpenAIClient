// cd C:\CODE\GitHub\AurelsOpenAIClient\oai\bin\Debug\net8.0
// dotnet oai.dll -q "Why is the sun red at sunset?" -m gpt-4o-mini -k YOUR_API_KEY
// dotnet oai.dll -q "Say Hello!"

using AurelsOpenAIClient.Chat;
using oai;

string question = null;
string model = null;
string apikey = null;
float temperature = -1.0f;
string systemrole = null;
string errors = string.Empty;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "-q" && i + 1 < args.Length)
    {
        question = args[i + 1];
        i++;
    }
    else if (args[i] == "-m" && i + 1 < args.Length)
    {
        model = args[i + 1];
        i++;
    }
    else if (args[i] == "-k" && i + 1 < args.Length)
    {
        apikey = args[i + 1];
        i++;
    }
    else if (args[i] == "-t" && i + 1 < args.Length)
    {
        float.TryParse(args[i + 1], out temperature);
        i++;
    }
    else if (args[i] == "-s" && i + 1 < args.Length)
    {
        systemrole = args[i + 1];
        i++;
    }
}

// Useful for piping or redirection or longer text input from file, see examples below:
// dotnet oai.dll < input.txt > out.txt
// dotnet oai.dll input.txt > out.txt
// dotnet oai.dll input.txt -s "Do The Opposite!" (input.txt content is "say hello", the answer will be "goodbye")
// If no -q was provided, try to get the question from a single filename arg or from redirected stdin
if (string.IsNullOrEmpty(question))
{
    if (File.Exists(args[0]))
    {
        question = File.ReadAllText(args[0]).Trim();
    }
    else if (Console.IsInputRedirected)
    {
        question = Console.In.ReadToEnd().Trim();
    }
}

// Load missing settings from settings.json
if (string.IsNullOrEmpty(apikey))
    apikey = LoadSettings.ReadPropertyFromJson("ApiKey");

if (string.IsNullOrEmpty(model))
    model = LoadSettings.ReadPropertyFromJson("PreferedChatCompletionModel");

if (string.IsNullOrEmpty(systemrole))
    systemrole = LoadSettings.ReadPropertyFromJson("SystemRole");

if (temperature < 0)
{
    string temperatureStr = LoadSettings.ReadPropertyFromJson("PreferedChatCompletionModel");
    if (!string.IsNullOrEmpty(temperatureStr))
        float.TryParse(temperatureStr, out temperature);
}

// Collecting errors
if (string.IsNullOrEmpty(question))
    errors += "Error: Question (-q) is required.\n";

if (string.IsNullOrEmpty(apikey))
    errors += "Error: API Key (-k) is required if not set in settings.json.\n";

if (string.IsNullOrEmpty(model))
    errors += "Error: Model (-m) is required if not set in settings.json.\n";

if (!string.IsNullOrEmpty(errors))
{
    string hint = "Please provide the necessary arguments!\n";
    hint += "Usage examples:\ndotnet oai.dll -q \"Why is the sun red at sunset?\" -m \"gpt-4o\" -k \"YOUR_API_KEY\"\n";
    hint += "dotnet oai.dll -q \"Why is the sun red at sunset?\" (if apikey and model is set in settings.json)\n";
    Console.WriteLine(errors);
    Console.WriteLine(hint);
    return;
}

// Call the API, and display the result
ChatCompletion chatCompletion = new ChatCompletion(apikey);
chatCompletion.SetModel(model);
chatCompletion.SetTemperature(temperature);
chatCompletion.SetSystemRole(systemrole);

string response = await chatCompletion.SendChat(userInput: question, throwExceptionWhenErrorOccours: true, logWhenErrorOccours: true);

Console.WriteLine(response);
