using AurelsOpenAIClient.Chat.Parameters;
using AurelsOpenAIClient.Chat.Request;
using AurelsOpenAIClient.Chat.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AurelsOpenAIClient.Chat
{
    public class ChatCompletion : OpenAiCommonBase
    {
        private List<PreviousQuestionAnswerPair> _previousQuestionsAndAnswers;
        private ChatResponse _openAiChatCompletionResponse;
        private string _systemRole;
        private float _temperature;

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
        /// <exception cref="ApplicationException"></exception>
        public ChatCompletion(string apiKey, string organization = null, string project = null) : base(apiKey, organization, project)
        {
            _endpoint = "https://api.openai.com/v1/chat/completions";
            _systemRole = "You are a helpful assistant.";
            _model = "gpt-4o"; // default model
            _temperature = 0.7f; // default temperature
            _maxTokens = 5000; // default max tokens
            _previousQuestionsAndAnswers = new List<PreviousQuestionAnswerPair>();
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
                _systemRole = "You are a helpful assistant.";
                throw new ArgumentException("SystemRole must not be empty! Now it is set to default value: \"You are a helpful assistant.\" ");
            }

            _systemRole = systemRole;
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

            temperature = temperature;
        }
        #endregion

        #region QuestionAnswerPairs

        /// <summary>
        /// Use this function to clear all previous question and answer pairs.
        /// </summary>
        public void ClearPreviousQuestionAndAnswerPairs()
        {
            _previousQuestionsAndAnswers = new List<PreviousQuestionAnswerPair>();
        }

        /// <summary>
        /// Retrieve the most recent question and answer pairs, up to the specified count.
        /// </summary>
        /// <param name="count">Number of most recent Q&A pairs</param>
        /// <returns>most recent Q&A pairs as List<PreviousQuestionAnswerPair></returns>
        public List<PreviousQuestionAnswerPair> GetMostRecentQuestionAnswerPairs(int count)
        {
            List<PreviousQuestionAnswerPair> previousQAs = _previousQuestionsAndAnswers
                .OrderByDescending(pair => pair.Timestamp)
                .Take(count)
                .OrderBy(pair => pair.Timestamp)
                .ToList();

            return previousQAs;
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

            foreach (PreviousQuestionAnswerPair qaPair in _previousQuestionsAndAnswers)
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
                    _previousQuestionsAndAnswers.RemoveAll(qa => qa.Timestamp < firstMatch.Timestamp);
                }

                List<PreviousQuestionAnswerPair> previousRelevantQAs = _previousQuestionsAndAnswers
                    .Where(t => t.Timestamp >= firstMatch.Timestamp)
                    .ToList();

                return previousRelevantQAs;
            }

            return new List<PreviousQuestionAnswerPair>();
        }
        #endregion

        #region SendChat overloads and SendChatAdvanced

        /// <summary>
        /// ChatCompletetion with only user input argument.
        /// No previous questions and answers pairs will be included in the request.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(string userInput, bool throwExceptionWhenErrorOccours = false, bool logWhenErrorOccours = false)
        {
            ChatCompletionsMessage userInputMessage = new ChatCompletionsMessage(role: "user", content: userInput);
            ChatCompletionsMessage roleInputMessage = new ChatCompletionsMessage(role: "system", content: _systemRole);
            List<ChatCompletionsMessage> messages = new List<ChatCompletionsMessage> { roleInputMessage, userInputMessage };

            ChatCompletionsParameters chatCompletetionParameters =
                new ChatCompletionsParameters(model: _model, messages: messages, temperature: _temperature, max_tokens: _maxTokens);

            string response = await GetResponse(chatCompletetionParameters, throwExceptionWhenErrorOccours, logWhenErrorOccours);
            return response;
        }

        /// <summary>
        /// ChatCompletetion with only user input argument.
        /// The given number of previous questions and answers pairs will be included in the request.
        /// </summary>
        /// <param name="userInput">Current question or instruction that will be sent to the endpoint.</param>
        /// <param name="numberOfPreviousQA">The given amount of question and answer pairs will be included in the request</param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(string userInput, int numberOfPreviousQA, bool throwExceptionWhenErrorOccours = false, bool logWhenErrorOccours = false)
        {
            List<ChatCompletionsMessage> messages = new List<ChatCompletionsMessage>()
            {
                new ChatCompletionsMessage(role: "system", content: _systemRole)
            };

            List<PreviousQuestionAnswerPair> prevQAs = GetMostRecentQuestionAnswerPairs(numberOfPreviousQA);

            foreach (PreviousQuestionAnswerPair qa in prevQAs)
            {
                messages.Add(qa.Question);
                messages.Add(qa.Answer);
            }

            messages.Add(new ChatCompletionsMessage(role: "user", content: userInput));

            string response = await SendChat(messages, throwExceptionWhenErrorOccours, logWhenErrorOccours);
            return response;
        }

        /// <summary>
        /// You can use this function to search for keywords in previous Q&A pairs.
        /// Only Q&A pairs containing at least one of the given keywords will be included in the request.
        /// Optional: Q&As can be deleted before any keyword found in conversation
        /// </summary>
        /// <param name="userInput">Current question or instruction that will be sent to the endpoint.</param>
        /// <param name="keywords">At least one of the given given keywords must be in the question&answer pair. Only from this Q&A pair will be passed to the ChatCompletetion endpoint.</param>
        /// <param name="deleteQAsBefore">Optional parameter. When it is true, the Q&As will be deleted before the first occurrence. (It can be useful when the topic has changed in the conversation)</param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(string userInput, List<string> keywords, bool deleteQAsBefore = false, bool throwExceptionWhenErrorOccours = false, bool logWhenErrorOccours = false)
        {
            List<ChatCompletionsMessage> messages = new List<ChatCompletionsMessage>()
            {
                new ChatCompletionsMessage(role: "system", content: _systemRole)
            };

            List<PreviousQuestionAnswerPair> prevQAs = GetLastQuestionAnswerPairsContainingKeywords(keywords, deleteQAsBefore);

            foreach (PreviousQuestionAnswerPair qa in prevQAs)
            {
                messages.Add(qa.Question);
                messages.Add(qa.Answer);
            }

            messages.Add(new ChatCompletionsMessage(role: "user", content: userInput));

            string response = await SendChat(messages, throwExceptionWhenErrorOccours, logWhenErrorOccours);
            return response;
        }

        /// <summary>
        /// ChatCompletetion with the  List of messages as argument.
        /// You have to include the current message and optionally the system role in the List of messages.
        /// </summary>
        /// <param name="messages">List<ChatCompletionsMessage></param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChat(List<ChatCompletionsMessage> messages, bool throwExceptionWhenErrorOccours = false, bool logWhenErrorOccours = false)
        {
            ChatCompletionsParameters chatCompletetionParameters =
                new ChatCompletionsParameters(_model, messages, _maxTokens, _temperature);

            string response = await GetResponse(chatCompletetionParameters, throwExceptionWhenErrorOccours, logWhenErrorOccours);
            return response;
        }

        /// <summary>
        /// More advanced ChatCompletetion with full control over all parameters
        /// </summary>
        /// <param name="chatCompletetionParameters"></param>
        /// <returns>Chat response as string</returns>
        public async Task<string> SendChatAdvanced(ChatCompletionsParameters chatCompletetionParameters, bool throwExceptionWhenErrorOccours = false, bool logWhenErrorOccours = false)
        {
            return await GetResponse(chatCompletetionParameters, throwExceptionWhenErrorOccours, logWhenErrorOccours);
        }
        #endregion

        #region ResponseSpecificFunctions

        public ChatResponse GetFullChatResponse() => _openAiChatCompletionResponse;

        public int GetTotalTokens() => _openAiChatCompletionResponse?.usage?.total_tokens ?? 0;

        public int GetPromtTokens() => _openAiChatCompletionResponse?.usage?.prompt_tokens ?? 0;

        public int GetCompletionTokens() => _openAiChatCompletionResponse?.usage?.completion_tokens ?? 0;

        public string GetLastAnswer() => _openAiChatCompletionResponse?.choices?[0]?.message?.content;
        #endregion

        #region Private Functions

        private async Task<string> GetResponse(ChatCompletionsParameters chatCompletetionParameters, bool throwExceptionWhenErrorOccours = false, bool logWhenErrorOccours = false)
        {
            try
            {
                string question = GetQuestion(chatCompletetionParameters);

                if (string.IsNullOrEmpty(chatCompletetionParameters.model))
                    throw new ApplicationException("LLM ChatCompletetion model is not set.");

                _jsonRequest = JsonSerializer.Serialize(chatCompletetionParameters, new JsonSerializerOptions { WriteIndented = true });

                StringContent content = new StringContent(_jsonRequest, Encoding.UTF8, "application/json");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, content);
                response.EnsureSuccessStatusCode();

                _responseTime = (int)(DateTime.Now - start).TotalMilliseconds;
                _jsonResponse = string.Empty;

                _jsonResponse = await response.Content.ReadAsStringAsync();

                _openAiChatCompletionResponse = 
                    JsonSerializer.Deserialize<ChatResponse>(_jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _jsonRequest = JsonSerializer.Serialize(_openAiChatCompletionResponse, new JsonSerializerOptions { WriteIndented = true });

                string chatResponse = _openAiChatCompletionResponse?.choices[0]?.message?.content?.ToString();

                StorePreviousQuestionsAndAnswers(question, chatResponse);

                return chatResponse;
            }
            catch (Exception ex)
            {
                if (logWhenErrorOccours)
                {
                    string error = ex.Message + Environment.NewLine;
                    error += "Request:" + Environment.NewLine + _jsonRequest + Environment.NewLine;
                    error += "Response:" + Environment.NewLine + _jsonResponse + Environment.NewLine;
                    string timestamp = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss");
                    File.WriteAllText($"GetResponse_Exception_{timestamp}.txt", error);
                }
                if (throwExceptionWhenErrorOccours)
                    throw;
                else
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
            _previousQuestionsAndAnswers.Add(qaPair);
        }
        #endregion

    }
}