namespace NormaLang
{
    public static class Interprete
    {
        public static string FilePath;
        public static void RunCode(string code, string file)
        {
            FilePath = file;
            var lexerLines = Lexer.Tokenizer(code);
            var parserLines = Parser.Parse(lexerLines);
            Execution.Execute(parserLines);
        }
    }
}
