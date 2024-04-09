using static CustomLang.Lexer;

namespace CustomLang
{
    public class Parser
    {
        public static Line[] Parse(Line[] lines)
        {
            Line[] parserLines = [];

            for (int i = 0; i < lines.Length; i++)
            {
                Line lexerLine = lines[i];
                Token[] tokens = [];

                for (int j = 0; j < lexerLine.Tokens.Length; j++)
                {

                }

                parserLines = [.. parserLines, new Line(lines[i].Number, tokens)];
            }

            return parserLines;
        }
    }
}
// z = (x * y)
//
// AST Tree:
//
//        z
//       / \
//      z   *
//         / \
//        x   y