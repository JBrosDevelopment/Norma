namespace NormaLang 
{
    public static class Program
    {
        public static void Main(string[] args) 
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            var code = File.ReadAllText("./Code.clang");
            var lexerLines = Lexer.Tokenizer(code);
            var parserLines = Parser.Parse(lexerLines);

            Execution.Execute(parserLines);

            // To time how fast CLang is
            Console.WriteLine("------------------\nMiliseconds: " + sw.ElapsedMilliseconds.ToString());
        }
    }
}