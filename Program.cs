namespace NormaLang 
{
    public static class Program
    {
        public static void Main(string[] args) 
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            var code = File.ReadAllText("./Code.norm");
            var lexerLines = Lexer.Tokenizer(code);
            var parserLines = Parser.Parse(lexerLines);

            Execution.Execute(parserLines);

            // To time how fast Norm is
            Console.WriteLine("------------------\nMiliseconds: " + sw.ElapsedMilliseconds.ToString());
        }
    }
}