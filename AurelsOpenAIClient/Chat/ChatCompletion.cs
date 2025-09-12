using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using AurelsOpenAIClient.Chat.Parameters;
using AurelsOpenAIClient.Chat.Response;
using AurelsOpenAIClient.Chat.Request;

namespace AurelsOpenAIClient.Chat
{
    public class ChatCompletion : OpenAiCommonBase
    {
        public List<PreviousQuestionAnswerPair> PreviousQuestionsAndAnswers = new List<PreviousQuestionAnswerPair>();
        private ChatResponse openAiChatCompletionResponse { get; set; } = default(ChatResponse);
        private string systemRole;
        private float temperature = 0.7f;

        /// <summary>
        /// ChatCompletetion class to interact with chat endpoint.
        /// New models: gpt-4o, gpt-4.1, gpt-4.1-mini, gpt-4.1-nano
        /// Cost effective: gpt-3.5-turbo, gpt-3.5-turbo-16k
        /// Most recent: gpt-5, gpt-5-mini, gpt-5-nano
        /// https://platform.openai.com/docs/api-reference/completions/create
        /// https://platform.openai.com/docs/models
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="organization">Optional</param>
        /// <param name="project">Optional</param>
        /// <exception cref="Exception"></exception>
        public ChatCompletion(string apiKey, string organization = null, string project = null)
        {
            this.httpClient = new HttpClient();
            this.endpoint = "https://api.openai.com/v1/chat/completions";
            this.systemRole = "You are a helpful assistant.";
            this.model = "gpt-4o"; // default model
            this.temperature = 0.7f; // default temperature

            if (!string.IsNullOrEmpty(apiKey))
                this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            else
                throw new ApplicationException("API key is not set.");

            if (!string.IsNullOrEmpty(organization))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", organization);

            if (!string.IsNullOrEmpty(project))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Project", project);

            this.PreviousQuestionsAndAnswers = new List<PreviousQuestionAnswerPair>();
        }
        #region Settings

        /// <summary>
        /// You can set the role of the assistant, for example: "You are a helpful assistant.", "You are a professional travel guide.", etc...
        /// </summary>
        /// <param name="systemRole"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetSystemRole(string systemRole)
        {
            if (string.IsNullOrEmpty(systemRole))
            {
                this.systemRole = "You are a helpful assistant.";
                throw new ArgumentException("SystemRole must not be empty! Now it is set to default value: \"You are a helpful assistant.\" ");
            }

            this.systemRole = systemRole;
        }

        /// <summary>
        /// You can set the temperature from 0.0 to 2.0 (default: 0.7)
        /// Under the hood, large language models try to predict the next best word given a prompt.One word at a time.They assign a probability to each word in their vocabulary, and then picks a word among those.
        /// A temperature of 0 means roughly that the model will always select the highest probability word.
        /// A higher temperature means that the model might select a word with slightly lower probability, leading to more variation, randomness and creativity.
        /// A very high temperature therefore increases the risk of “hallucination”, meaning that the AI can start selecting words that will make no sense or be offtopic.
        /// </summary>
        /// <param name="role"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetTemperature(float temperature)
        {
            if (temperature < 0 || temperature > 2)
                throw new ApplicationException("Temperature must be a value between 0.0 and 2.0");

            this.temperature = temperature;
        }

        public void ClearPreviousQuestionAndAnswerPairs()
        {
            this.PreviousQuestionsAndAnswers = new List<PreviousQuestionAnswerPair>();
        }

        /// <summary>
        /// Add keywords to search for in previous Q&A pairs. The search is case insensitive and checks both questions and answers.
        /// </summary>
        /// <param name="keywords">List of strings</param>
        /// <param name="deleteQAsBefore">Set to true if you want to delete the previous conversation, for example when changing the conversation topic</param>
        /// <returns></returns>
        public List<PreviousQuestionAnswerPair> GetLastQuestionAnswerPairsContainingKeywords(List<string> keywords, bool deleteQAsBefore = false)
        {
            if (keywords == null || keywords.Count == 0)
                return new List<PreviousQuestionAnswerPair>();

            // Search for the oldest QA pair
            PreviousQuestionAnswerPair firstMatch = null;

            foreach (PreviousQuestionAnswerPair qaPair in this.PreviousQuestionsAndAnswers)
            {
                if (keywords.Any(kw =>
                    (qaPair.Question.content?.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (qaPair.Answer.content?.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    firstMatch = qaPair;
                    break;
                }
            }

            if (firstMatch != null)
            {
                if (deleteQAsBefore)
                {
                    this.PreviousQuestionsAndAnswers.RemoveAll(qa => qa.Timestamp < firstMatch.Timestamp);
                }

                List<PreviousQuestionAnswerPair> previousRelevantQAs = this.PreviousQuestionsAndAnswers
                    .Where(t => t.Timestamp >= firstMatch.Timestamp)
                    .ToList();

                return previousRelevantQAs;
            }

            return new List<PreviousQuestionAnswerPair>();
        }
        #endregion

        /// <summary>
        /// ChatCompletetion with only user input, using default system role and temperature, no previous questions and answers will be included in the request.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(string userInput)
        {
            try
            {
                ChatCompletionsMessage userInputMessage = new ChatCompletionsMessage(role: "user", content: userInput);
                ChatCompletionsMessage roleInputMessage = new ChatCompletionsMessage(role: "system", content: this.systemRole);
                List<ChatCompletionsMessage> messages = new List<ChatCompletionsMessage> { roleInputMessage, userInputMessage };

                ChatCompletionsParameters chatCompletetionParameters =
                    new ChatCompletionsParameters(model: this.model, messages: messages, temperature: this.temperature, max_tokens: this.maxTokens);

                string response = await GetResponse(chatCompletetionParameters);
                return response;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInput">Current question or instruction that will be sent to the endpoint.</param>
        /// <param name="numberOfPreviousQA">The given amount of question and answer pairs will be included in the request</param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(string userInput, int numberOfPreviousQA = 0)
        {
            List<ChatCompletionsMessage> messages = new List<ChatCompletionsMessage>()
            {
                new ChatCompletionsMessage(role: "system", content: this.systemRole)
            };

            List<PreviousQuestionAnswerPair> prevQAs = GetMostRecentQuestionAnswerPairs(numberOfPreviousQA);

            foreach (PreviousQuestionAnswerPair qa in prevQAs)
            {
                messages.Add(qa.Question);
                messages.Add(qa.Answer);
            }

            messages.Add(new ChatCompletionsMessage(role: "user", content: userInput));

            string response = await SendChat(messages);
            return response;
        }

        /// <summary>
        /// For Q&A + remembering previous questions and answers containing at least one of the given keywords, optional: Q&As can be deleted before any keyword found in conversation
        /// </summary>
        /// <param name="userInput">Current question or instruction that will be sent to the endpoint.</param>
        /// <param name="keywords">At least one of the given given keywords must be in the question&answer pair. Only from this Q&A pair will be passed to the ChatCompletetion endpoint.</param>
        /// <param name="deleteQAsBefore">Optional parameter. When it is true, the Q&As will be deleted before the first occurrence. (It can be useful when the topic has changed in the conversation)</param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(string userInput, List<string> keywords, bool deleteQAsBefore = false)
        {
            List<ChatCompletionsMessage> messages = new List<ChatCompletionsMessage>()
            {
                new ChatCompletionsMessage(role: "system", content: this.systemRole)
            };

            List<PreviousQuestionAnswerPair> prevQAs = GetLastQuestionAnswerPairsContainingKeywords(keywords, deleteQAsBefore);

            foreach (PreviousQuestionAnswerPair qa in prevQAs)
            {
                messages.Add(qa.Question);
                messages.Add(qa.Answer);
            }

            messages.Add(new ChatCompletionsMessage(role: "user", content: userInput));

            string response = await SendChat(messages);
            return response;
        }

        /// <summary>
        /// ChatCompletetion with a List of messages, using default temperature.
        /// You have to include the current message and optionally the system role in the List of messages.
        /// </summary>
        /// <param name="messages">List<ChatCompletionsMessage></param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(List<ChatCompletionsMessage> messages)
        {
            ChatCompletionsParameters chatCompletetionParameters =
                new ChatCompletionsParameters(this.model, messages, this.maxTokens, this.temperature);

            string response = await GetResponse(chatCompletetionParameters);
            return response;
        }

        /// <summary>
        /// More advanced ChatCompletetion with full control over all parameters
        /// </summary>
        /// <param name="chatCompletetionParameters"></param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChatAdvanced(ChatCompletionsParameters chatCompletetionParameters)
        {
            return await GetResponse(chatCompletetionParameters);
        }

        #region Functions
        private async Task<string> GetResponse(ChatCompletionsParameters chatCompletetionParameters, bool logWhenErrorOccours = false)
        {
            try
            {
                string question = GetQuestion(chatCompletetionParameters);

                if (string.IsNullOrEmpty(chatCompletetionParameters.model))
                    throw new ApplicationException("LLM ChatCompletetion model is not set.");

                this.jsonRequest = JsonConvert.SerializeObject(chatCompletetionParameters);

                StringContent content = new StringContent(this.jsonRequest, Encoding.UTF8, "application/json");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                this.responseTime = (int)(DateTime.Now - start).TotalMilliseconds;
                this.jsonResponse = string.Empty;

                this.jsonResponse = await response.Content.ReadAsStringAsync();
                this.openAiChatCompletionResponse = JsonConvert.DeserializeObject<ChatResponse>(this.jsonResponse);

                JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };
                string formattedJsonResponse = JsonConvert.SerializeObject(this.openAiChatCompletionResponse, serializerSettings);

                string chatResponse = openAiChatCompletionResponse?.choices[0]?.message?.content?.ToString();

                StorePreviousQuestionsAndAnswers(question, chatResponse);

                return chatResponse;
            }
            catch (Exception ex)
            {
                if (logWhenErrorOccours)
                {
                    string error = ex.Message + Environment.NewLine;
                    error += "Request:" + Environment.NewLine + this.jsonRequest + Environment.NewLine;
                    error += "Response:" + Environment.NewLine + this.jsonRequest + Environment.NewLine;
                    string timestamp = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss");
                    File.WriteAllText($"GetResponse_Exception_{timestamp}.txt", error);
                }
                return ex.Message;
            }
        }

        private string GetQuestion(ChatCompletionsParameters chatCompletetionParameters)
        {
            return chatCompletetionParameters?.messages?.LastOrDefault()?.content?.ToString();
        }

        private void StorePreviousQuestionsAndAnswers(string question, string answer)
        {
            PreviousQuestionAnswerPair qaPair = new PreviousQuestionAnswerPair(question, answer);
            this.PreviousQuestionsAndAnswers.Add(qaPair);
        }

        /// <summary>
        /// For debugging purpose. Saves messages and answers to files with timestamp.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="choices"></param>
        /// <param name="log"></param>
        private void Log(List<ChatCompletionsMessage> messages, List<Choice> choices, bool log = false)
        {
            string timestamp = DateTime.Now.ToString("yyyy_MM_dd__hh_mm_ss");
            string jsonResponseMessages = JsonConvert.SerializeObject(messages, formatting: Formatting.Indented);
            string jsonResponseChoices = JsonConvert.SerializeObject(choices, formatting: Formatting.Indented);
            File.WriteAllText($"messages_{timestamp}", jsonResponseMessages);
            File.WriteAllText($"choices_{timestamp}", jsonResponseChoices);
        }

        public List<PreviousQuestionAnswerPair> GetMostRecentQuestionAnswerPairs(int count)
        {
            List<PreviousQuestionAnswerPair> previousQAs = this.PreviousQuestionsAndAnswers
                .OrderByDescending(pair => pair.Timestamp)
                .Take(count)
                .OrderBy(pair => pair.Timestamp)
                .ToList();

            return previousQAs;
        }
        #endregion

        #region ResponseSpecificFunctions
        public ChatResponse GetFullChatResponse() => openAiChatCompletionResponse;

        public int GetTotalTokens() => openAiChatCompletionResponse?.usage?.total_tokens ?? 0;

        public int GetPromtTokens() => openAiChatCompletionResponse?.usage?.prompt_tokens ?? 0;

        public int GetCompletionTokens() => openAiChatCompletionResponse?.usage?.completion_tokens ?? 0;
        #endregion
    }
}
