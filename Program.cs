namespace ElasticSearchDemo
{
    using Nest;

    public class Program
    {
        private static Uri node;
        private static ConnectionSettings connectionSettings;
        private static ElasticClient client;

        public static void Main()
        {
            node = new Uri("http://localhost:9200");
            connectionSettings = new ConnectionSettings(node).DefaultIndex("simply-recipes");
            client = new ElasticClient(connectionSettings);

            var indexSettings = new IndexSettings
            {
                NumberOfReplicas = 1,
                NumberOfShards = 1
            };

            client.Indices.Create(
                "simply-recipes",
                index => index
                    .InitializeUsing(new IndexState
                    {
                        Settings = indexSettings
                    })
                    .Map<Article>(p => p.AutoMap()));

            // Uncomment these methods to perform operations.

            // InsertData();
            PerformTermQuery();
            PerformMatchPhrase();
            PerformFilter();
        }

        private static void InsertData()
        {
            var newArticle = new Article
            {
                UserId = 1,
                ArticleDate = DateTime.Now,
                ArticleText = "This is an article text."
            };

            var pastArticle = new Article
            {
                UserId = 2,
                ArticleDate = DateTime.Now.AddDays(-2),
                ArticleText = "This is an article text from the past."
            };

            var futureArticle = new Article
            {
                UserId = 2,
                ArticleDate = DateTime.Now.AddDays(5),
                ArticleText = "This is an article text from the future."
            };

            client.IndexDocument(newArticle);
            client.IndexDocument(pastArticle);
            client.IndexDocument(futureArticle);

            Console.WriteLine("Data inserted.");
        }

        private static void PerformTermQuery()
        {
            Console.WriteLine("Term query results:");

            var result = client
                .Search<Article>(a => a
                    .Query(x => x.Term(q => q.ArticleText, "article")));

            result
                .Documents
                .ToList()
                .ForEach(x => Console.WriteLine(x.ArticleText));

            Console.WriteLine(new string('-', 40));
        }

        private static void PerformMatchPhrase()
        {
            Console.WriteLine("Match query results:");

            var result = client
                .Search<Article>(a => a
                    .Query(x => x
                        .MatchPhrase(m => m.Field(p => p.ArticleText)
                        .Query("past"))));

            result
                .Documents
                .ToList()
                .ForEach(x => Console.WriteLine(x.ArticleText));

            Console.WriteLine(new string('-', 40));
        }

        private static void PerformFilter()
        {
            Console.WriteLine("Filter query results:");

            var result = client
                .Search<Article>(a => a
                    .Query(x => x.Bool(b => b
                        .Must(m => m
                            .Match(m => m
                                .Field(f => f.ArticleText).Query("article")))
                        .Filter(f => f
                            .DateRange(r => r
                                .Field(p => p.ArticleDate).GreaterThan(DateTime.Now))))));

            result
                .Documents
                .ToList()
                .ForEach(x => Console.WriteLine(x.ArticleText));

            Console.WriteLine(new string('-', 40));
        }
    }
}