namespace CustomLang 
{
    public static class Program
    {
        public static void Main(string[] args) 
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            var lexerLines = Lexer.Tokenizer("""
                // CLang Testing

                var result = parse "25"
                print $result$
                

                """);
            var parserLines = Parser.Parse(lexerLines);

            Execution.Execute(parserLines);

            // To time how fast CLang is
            Console.WriteLine("------------------\nMiliseconds: " + sw.ElapsedMilliseconds.ToString());
        }
    }
}