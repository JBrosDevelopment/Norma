using static NormaLang.Lexer;

namespace NormaLang
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

                if (tokens.Length > 0 && tokens[0].Type == TokenType.Reserved)
                {
                    line = ReturnLineWithReservedToken(tokens, ref lines, ref i);
                }
                if (tokens.Length != 0)
                {
                    parserLines = [.. parserLines, line];
                }
            }

            return parserLines;
        }
        internal static Line ReturnLineWithReservedToken(Token[] tokens, ref Line[] lines, ref int i)
        {
            if (Statement.StatementTypes.Any(x => x == tokens[0].Value))
            {
                return ReturnStatement(tokens, ref lines, ref i);
            }
            else if (tokens[0].Value == "def")
            {
                return ReturnDefine(tokens, ref lines, ref i);
            }
            else if (tokens[0].Value == "struct")
            {
                return ReturnStruct(tokens, ref lines, ref i);
            }
            else
            {
                return lines[i];
            }
        }
        internal static Line ReturnStatement(Token[] tokens, ref Line[] lines, ref int i)
        {
            Line[] statementLines = [];

            Statement.StatementType statementType =
                tokens[0].Value == "if" ? Statement.StatementType.If :
                tokens[0].Value == "elif" ? Statement.StatementType.ElIf :
                tokens[0].Value == "else" ? Statement.StatementType.Else :
                tokens[0].Value == "for" ? Statement.StatementType.For :
                Statement.StatementType.While;

            Token[] tokenInput = statementType != Statement.StatementType.Else ?
                tokens.Skip(1).TakeWhile(x => x.Value != "{").ToArray() : [];

            statementLines = GetLinesInBrackets(tokens, ref lines, ref i);

            Statement statement = new Statement(tokenInput, statementLines, statementType);
            return new Line(lines[i].Number, [], statement: statement);
        }
        internal static Line ReturnDefine(Token[] tokens, ref Line[] lines, ref int i)
        {
            Line line = lines[i];
            string name = tokens[1].Value;
            Line[] defLines = [];
            Variable[] parameters = [];

            bool containsParameters = line.Tokens.Any(x => x.Value == ":");
            if (containsParameters)
            {
                string paramtersAll = string.Join("", tokens.Skip(3).TakeWhile(x => x.Value != "{").Select(x => x.Value));
                string[] parameterParts = paramtersAll.Split(',');
                parameters = parameterParts.Select(x => new Variable(x, null)).ToArray();
            }

            defLines = GetLinesInBrackets(tokens, ref lines, ref i);

            Define def = new Define(name, defLines, parameters);
            return new Line(line.Number, [], define: def);
        }
        internal static Line ReturnStruct(Token[] tokens, ref Line[] lines, ref int i)
        {
            string name = tokens[1].Value;
            Line[] structLines = [];
            string[] properties = [];

            structLines = GetLinesInBrackets(tokens, ref lines, ref i);

            Token[] tokenAr = [];
            structLines.Select(x =>
            {
                tokenAr = tokenAr.Concat(x.Tokens).ToArray();
                return x;
            }).ToArray();
            string[] stringAr = tokenAr.Select(x => x.Value).ToArray();

            bool lastWasComma = true;
            for (int k = 0; k < stringAr.Length; k++)
            {
                string str = stringAr[k];
                if (lastWasComma)
                {
                    if (str == ",")
                    {
                        throw new Exception($"Incorrect syntax in struct '{name}', no commas were found in between property names");
                    }
                    else
                    {
                        properties = [.. properties, str];
                        lastWasComma = false;
                    }
                }
                else
                {
                    if (str == ",")
                    {
                        lastWasComma = true;
                    }
                    else
                    {
                        throw new Exception($"Incorrect syntax in struct '{name}', two commas ',,' were found with no property in between");
                    }
                }
            }

            Struct struc = new Struct(name, properties);
            return new Line(lines[i].Number, [], @struct: struc);
        }
        internal static Line[] GetLinesInBrackets(Token[] tokens, ref Line[] lines, ref int i)
        {
            Line[] returnLines = [];
            Line line = lines[i];

            bool openbracketOnSameLine = tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "{");
            bool closebracketOnSameLine = tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "}");

            if (openbracketOnSameLine && closebracketOnSameLine)
            {
                var tokensInBrackets = tokens.SkipWhile(x => x.Value != "{").TakeWhile(x => x.Value != "}").Skip(1).ToArray();
                returnLines = [new Line(line.Number, tokensInBrackets)];
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
                        returnLines = [new Line(line.Number, tokensInBrackets)];
                    }
                }
                else if (lines[startIndex].Tokens.Length > 0 && lines[startIndex].Tokens[0].Type != TokenType.Symbol && lines[startIndex].Tokens[0].Value == "{")
                {
                    throw new Exception("Expected next line to start with opening bracket in line " + line.Number);
                }
                int bracketCount = 1;
                for (i = startIndex; i < lines.Length; i++)
                {
                    Line l = lines[i];

                    if (l.Tokens.Length > 0 && l.Tokens[0].Type is TokenType.Reserved)
                    {
                        l = ReturnLineWithReservedToken(l.Tokens, ref lines, ref i);
                    }
                    if (l.Tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "{")) bracketCount++;
                    if (l.Tokens.Any(x => x.Type == TokenType.Symbol && x.Value == "}"))
                    {
                        bracketCount--;
                        if (bracketCount == 0 && l.Tokens.LastOrDefault().Value == "}" && l.Tokens.Length > 1)
                        {
                            l.Tokens = l.Tokens.Take(l.Tokens.Length - 1).ToArray();
                        }
                    }

                    if (bracketCount == 0) break;

                    returnLines = [.. returnLines, l];
                }
            }
            return returnLines;
        }
    }
}