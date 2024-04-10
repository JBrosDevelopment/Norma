using static CustomLang.Lexer;

namespace CustomLang
{
    public class Parser
    {
        /*
         * The parser will find any if/else/while statements.
         */
        public static Line[] Parse(Line[] lines)
        {
            Line[] parserLines = [];

            for (int i = 0; i < lines.Length; i++)
            {
                Line line = lines[i];
                Token[] tokens = line.Tokens;

                for (int j = 0; j < tokens.Length; j++)
                {
                    if (tokens[i].Type == TokenType.Reserved)
                    {
                        if (tokens[i].Value == "if")
                        {
                            
                        }
                    }
                }

                parserLines = [.. parserLines, new Line(lines[i].Number, tokens)];
            }

            return parserLines;
        }
    }
}