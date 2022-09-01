using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.TextAnalytics;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Lab1NlpAzureAi
{
    internal class Program
    {
        static string key;
        static string endpoint;
        static string keytwo;
        static string endpointtwo;
        static readonly string location = "northeurope";
        static async Task Main(string[] args)
        {

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
           
            endpoint = configuration["endpoint"];
            key = configuration["key"];

            endpointtwo = configuration["endpointtwo"];
            Uri endpointtwourl = new Uri(endpointtwo);
            keytwo = configuration["keytwo"];
            AzureKeyCredential credential = new AzureKeyCredential(keytwo);

            string projectName = "Sample-project";
            string deploymentName = "production";

            Console.WriteLine("to ask the bot a question!");
            Console.WriteLine("Write QUIT to cancel the program");
            var run = true;
            do
            {
                Console.Write("You: ");

                string route = "/translate?api-version=3.0&to=en";
                string textToTranslate = Console.ReadLine();
                if (textToTranslate != "QUIT")
                {
                    object[] body = new object[] { new { Text = textToTranslate } };
                    var requestBody = JsonConvert.SerializeObject(body);

                    var client = new HttpClient();
                    var request = new HttpRequestMessage();


                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    string result = await response.Content.ReadAsStringAsync();
                    string question = result;

                    QuestionAnsweringClient clienttwo = new QuestionAnsweringClient(endpointtwourl, credential);
                    QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

                    Response<AnswersResult> responetwo = clienttwo.GetAnswers(question, project);

                    foreach (KnowledgeBaseAnswer answer in responetwo.Value.Answers)
                    {
                        Console.WriteLine($"Bot: {answer.Answer}\n--------------------------------------------------");
                    }

                    TextAnalyticsClient CogClient = new TextAnalyticsClient(endpointtwourl, credential);

                    DetectedLanguage language = CogClient.DetectLanguage(textToTranslate);
                    Console.WriteLine($"Question asked in the language: {language.Name}");

                    DocumentSentiment sentilaung = CogClient.AnalyzeSentiment(textToTranslate);
                    Console.WriteLine($"Sentiment is: {sentilaung.Sentiment}\n--------------------------------------------------");
                }
                else
                {
                    run = false;
                }


            } while (run);

        }
    }
}
