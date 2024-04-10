namespace CustomLang 
{
    public static class Program
    {
        public static void Main(string[] args) 
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            var lexerLines = Lexer.Tokenizer("""
                // CLang Testing
                
                var result = input
                if $result$ = "hi" {
                    result = 50
                }
                elif $result$ = "ho" {
                    result = 60
                }
                elif $result$ = "he" 
                {
                    result = 65
                }
                else { result = 70 }
                var i = 0
                while $i$ < 50
                {
                    print "$i$:$result$"
                    i + 1
                }

                """);
            var parserLines = Parser.Parse(lexerLines);

            Execution.Execute(parserLines);

            // To time how fast CLang is
            Console.WriteLine("------------------\nMiliseconds: " + sw.ElapsedMilliseconds.ToString());
        }
    }
}