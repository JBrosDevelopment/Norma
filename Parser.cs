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
                    if (tokens[j].Type == TokenType.Reserved)
                    {
                        if (tokens[j].Value == "if"|| tokens[j].Value == "elif" || tokens[j].Value == "else" || tokens[j].Value == "while")
                        {
                            Line[] statementLines = [];

                            Statement.StatementType statementType =
                                tokens[j].Value == "if" ? Statement.StatementType.If :
                                tokens[j].Value == "elif" ? Statement.StatementType.ElIf :
                                tokens[j].Value == "else" ? Statement.StatementType.Else :
                                Statement.StatementType.While;

                            Token[] tokenInput = statementType != Statement.StatementType.Else ?
                                tokens.Skip(1).TakeWhile(x => x.Value != "{").ToArray() : [];

                            bool openbracketOnSameLine = tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "{");
                            bool closebracketOnSameLine = tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "}");

                            if (openbracketOnSameLine && closebracketOnSameLine)
                            {
                                var tokensInBrackets = tokens.SkipWhile(x => x.Value != "{").TakeWhile(x => x.Value != "}").Skip(1).ToArray();
                                statementLines = [new Line(line.Number, tokensInBrackets)];
                            }
                            else if (!openbracketOnSameLine && closebracketOnSameLine)
                            {
                                throw new Exception("Closing bracket without opening bracket for statement in line " + line.Number);
                            }
                            else
                            {
                                int startIndex = i + 2;
                                if (openbracketOnSameLine)
                                {
                                    startIndex = i + 1;
                                    var tokensInBrackets = tokens.SkipWhile(x => x.Value != "{").Skip(1).ToArray();
                                    if (tokensInBrackets.Length != 0)
                                    {
                                        statementLines = [new Line(line.Number, tokensInBrackets)];
                                    }
                                }
                                else if (lines[startIndex].Tokens.Length > 0 && lines[startIndex].Tokens[0].Type != TokenType.Symbol && lines[startIndex].Tokens[0].Value == "{")
                                {
                                    throw new Exception("Expected next line to start with opening bracket in line " + line.Number);
                                }
                                int bracketCount = 1;
                                for (i = startIndex; i < lines.Length; i++)
                                {
                                    if (lines[i].Tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "{")) bracketCount++;
                                    if (lines[i].Tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "}"))
                                    {
                                        bracketCount--;
                                        if (bracketCount == 0 && lines[i].Tokens.LastOrDefault().Value == "}" && lines[i].Tokens.Length > 1)
                                        {
                                            lines[i].Tokens = lines[i].Tokens.Take(lines[i].Tokens.Length - 1).ToArray();
                                        }
                                    }

                                    if (bracketCount == 0) break;
                                    
                                    statementLines = [.. statementLines, lines[i]];
                                }
                            }


                            Statement statement = new Statement(tokenInput, statementLines, statementType);
                            line = new Line(line.Number, [], statement);
                        }
                    }
                }

                parserLines = [.. parserLines, line];
            }

            return parserLines;
        }
    }
}