namespace CustomLang 
{
    public static class Program
    {
        public static void Main(string[] args) 
        {
            Lexer.Line[] lines = Lexer.Tokenizer("""
                var result = parse "25"
                print $result$
                """);
            Execution.Execute(lines);
        }
    }
}