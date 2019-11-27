using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Teacher
{
    public partial class MainPage : ContentPage
    {
        private static List<WordsAndpronounce> Words = new List<WordsAndpronounce>();
        private int counter = 0;

        private static readonly string EndpointUri = "https://kakans-mongodbtest.documents.azure.com:443/";
        private static readonly string PrimaryKey = "FLlAhye3J3yK10uNNG1FQDg3aeEEgaxPe9l7tJxoP2jEQtHA8S5vTTORcnypmi0CM5ta7RsWLnFKn2nPY3GitQ==";
        private static readonly string TranslationUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";//"https://kakans-translation-api.cognitiveservices.azure.com/sts/v1.0/issuetoken";
        private static readonly string TranslationKey = "48305348a60d43f6aed72e7cc244f155";
        private string _databaseId = "Test";
        private string _containerId = "MongoDbWithNuget";

        public MainPage()
        {
            InitializeComponent();

            GetWords();
        }
        private void GetWords()
        {
            var docClient = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
            var todoQuery = docClient.CreateDocumentQuery<WordList>(
                    UriFactory.CreateDocumentCollectionUri(_databaseId, _containerId),
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .Where(todo => todo.id == "arabic")
                .AsDocumentQuery();

            while (todoQuery.HasMoreResults)
            {
                var queryResults = todoQuery.ExecuteNextAsync<WordList>().Result;
                if (queryResults.ToList().FirstOrDefault() != null) Words.AddRange(queryResults.First().words);
            }

            //using (var client = new DocumentClient(new Uri(EndpointUri), PrimaryKey))
            //{
            //    var queryResults = client.ReadDocumentAsync<WordList>(UriFactory.CreateDocumentUri(_databaseId, _containerId, "arabic")).Result;
            //    if (queryResults.Document.words != null) words.AddRange(queryResults.Document.words);
            //}
        }

        private string TranslateWord(string word)
        {
            var translatedWord = "";

            HttpClient _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", TranslationKey);
            var uri = new Uri(string.Format($"{TranslationUrl}&from=ar&to=sv", string.Empty));
            var json = JsonConvert.SerializeObject(new List<dynamic> { new { Text = word } });
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _client.PostAsync(uri, data).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                translatedWord = JsonConvert.DeserializeObject<List<ApiTranslatedWord>>(content).FirstOrDefault()?.translations.FirstOrDefault()?.text;
            }

            return translatedWord;

            //var translated = TranslatedWords.FirstOrDefault(k => k.Translation.Item1.Equals(word, StringComparison.InvariantCultureIgnoreCase) || k.Translation.Item2.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            //var translatedWord = translated?.SwedishWord ?? "";
        }

        private void Translate_OnClicked(object sender, EventArgs e)
        {
            counter = 0;

            var word = Word.Text;

            var translatedWord = TranslateWord(word);
            TranslatedWord.Text = translatedWord;
            if(!string.IsNullOrEmpty(translatedWord)) DependencyService.Get<ITextToSpeech>().Speak(translatedWord, "sv-SE");
        }

        private void Random_OnClicked(object sender, EventArgs e)
        {
            counter = 0;
            Random r = new Random();
            RandomWord.Text = Words[r.Next(0, Words.Count-1)].word;
        }

        private void TranslateRandom_OnClicked(object sender, EventArgs e)
        {
            var word = RandomWord.Text;
            var translatedWord = RandomTranslatedWord.Text;

            Random r = new Random();
            var correctWord = TranslateWord(word);

            if (correctWord.Trim('.').Equals(translatedWord, StringComparison.InvariantCultureIgnoreCase) || counter == 2)
            {
                counter = 0;
                RandomTranslatedWord.Text = correctWord; 
                DependencyService.Get<ITextToSpeech>().Speak($"Rätt ord är {correctWord}", "sv-SE"); //ar-SA
            }
            else
            {
                counter++;
                DependencyService.Get<ITextToSpeech>().Speak("Testa igen", "sv-SE"); //ar-SA
            }
        }


        public class WordList
        {
            public string id { get; set; }
            public List<WordsAndpronounce> words { get; set; }
        }
        public class WordsAndpronounce
        {
            public string word { get; set; }
            public string pronunciation { get; set; }
        }
        public class ApiTranslatedWord
        {
            public List<Ord> translations { get; set; }
        }
        public class Ord
        {
            public string text { get; set; }
        }
    }
}
