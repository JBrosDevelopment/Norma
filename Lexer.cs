namespace CustomLang 
{
    public class Lexer
    {
        public enum TokenType
        {
            Identifier, Reserved, Symbol, Variable, Number, String
        }
        public class Token
        {
            public string Value { get; set; }
            public TokenType Type { get; set; }
            public Token[] Children { get; set; }
            public Token(string value, TokenType type, Token[]? child = null)
            {
                Value = value;
                Type = type;
                Children = child ?? [];
            }
        }
        public class Line
        {
            public Statement? Statement { get; set; } = null;
            public int Number { get; set; }
            public Token[] Tokens { get; set; }
            public Line(int number, Token[] tokens, Statement? statement = null)
            {
                Number = number;
                Tokens = tokens;
                Statement = statement;
            }
        }
        internal static string[] ReservedKeywrods = ["var", .. Statement.Statements.Select(x => x.Name)];
        /*
         * This is the Lexer part of the interpreter. It takes a string and outputs lines of code that contain tokens
         */
        public static Line[] Tokenizer(string code) 
        {
            string[] lines = code.Split('\n').Select(x => x.Trim() ).ToArray();
            Line[] lexerLines = [];

            for (int i = 0; i < lines.Length; i++)
            {
                char[] chars = lines[i].ToCharArray();
                Token[] tokens = [];

                bool 
                    isQuotation = false,
                    isNumber = false,
                    isVariable = false,
                    isIdentifier = false,
                    isSymbolChar = false,
                    tempWasQuote = false;
                string
                    number = "",
                    quote = "",
                    variable = "",
                    identifier = "";

                for (int j = 0; j < chars.Length; j++)
                {
                    char c = chars[j];

                    if (chars.Length > j + 1 && c == '/' && chars[j + 1] == '/')
                        break;

                    if (isWhiteSpace(c) && !isQuotation)
                    {
                        isNumber = false;
                        if (isVariable) throw new Exception("Can not have whitespace in between '$' in line" + (i + 1)); // added 1 to 'i' to remove line '0' from occuring
                        isIdentifier = false;
                    }
                    else if (isNum(c))
                    {
                        isNumber = true;
                        number += c;
                    }
                    else if (isSymbol(c))
                    {
                        if (c == '"')
                        {
                            isQuotation = !isQuotation;
                        }
                        else if (c == '$')
                        {
                            tempWasQuote = isQuotation || (isVariable && tempWasQuote);
                            isVariable = !isVariable;
                            isQuotation = !isQuotation && tempWasQuote;
                        }
                        else if(!isQuotation)
                        {
                            isSymbolChar = true;
                            isNumber = false;
                            isVariable = false;
                            isIdentifier = false;
                        }
                        else if (isQuotation)
                        {
                            quote += c;
                        }
                    }
                    else if (isQuotation)
                    {
                        quote += c;
                    }
                    else if (isVariable)
                    {
                        variable += c;
                    }
                    else if (isAlpha(c))
                    {
                        identifier += c;
                        isIdentifier = true;
                    }
                    else
                    {
                        throw new Exception("Character '" + c + "' is not valid in line " + (i + 1));
                    }

                    if (!isNumber && number != "")
                    {
                        tokens = [.. tokens, new Token(number, TokenType.Number)];
                        number = "";
                    }
                    if (!isVariable && variable != "")
                    {
                        tokens = [.. tokens, new Token(variable, TokenType.Variable)];
                        variable = "";
                    }
                    if (!isQuotation && quote != "")
                    {
                        tokens = [.. tokens, new Token(quote, TokenType.String)];
                        quote = "";
                    }
                    if (!isIdentifier && identifier != "")
                    {
                        TokenType tokenType = ReservedKeywrods.Contains(identifier) ?
                            TokenType.Reserved :
                            TokenType.Identifier; 

                        tokens = [.. tokens, new Token(identifier, tokenType)];
                        identifier = "";
                    }
                    if (isSymbolChar)
                    {
                        tokens = [.. tokens, new Token(c.ToString(), TokenType.Symbol)];
                        isSymbolChar = false;
                    }
                }
                if (isNumber) tokens = [.. tokens, new Token(number, TokenType.Number)];
                else if (isVariable) tokens = [.. tokens, new Token(variable, TokenType.Variable)];
                else if (isQuotation) tokens = [.. tokens, new Token(quote, TokenType.String)];
                else if (isIdentifier)
                {
                    TokenType tokenType = ReservedKeywrods.Contains(identifier) ?
                        TokenType.Reserved :
                        TokenType.Identifier;
                    tokens = [.. tokens, new Token(identifier, tokenType)];
                }

                lexerLines = [.. lexerLines, new(i + 1, tokens)];
            }

            return lexerLines;
        }
        internal static bool isWhiteSpace(char c)
        {
            return string.IsNullOrWhiteSpace(c.ToString());
        }
        internal static bool isNum(char c)
        {
            return int.TryParse(c.ToString(), out _) || c == '.';
        }
        internal static bool isSymbol(char c)
        {
            return (new[] { '=', '+', '-', '*', '/', '$', '"', ',', '(', ')' }).Any(x => x == c);
        }
        internal static bool isAlpha(char c)
        {
            return !isNum(c) && !isWhiteSpace(c) && !isSymbol(c);
        }
    }
}