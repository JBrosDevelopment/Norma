namespace CustomLang 
{
    public static class Program
    {
        public static void Main(string[] args) 
        {
            Lexer.Line[] lines = Lexer.Tokenizer("""
                parseAdd "25", 150
                """);
            Execution.Execute(lines);
        }
    }
}