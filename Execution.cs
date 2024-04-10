using static CustomLang.Lexer;

namespace CustomLang
{
    public class Execution
    {
        /* 
         * This will take a Line[] and execute the code line by line
         */
        public static int Execute(Line[] lines)
        {
            Function[] functions = Function.InstantiateFunctions();
            Variable[] variables = [];

            int number_line = 0,
                error_code = 0;
            try
            {
                foreach (Line line in lines)
                {
                    number_line = line.Number;
                    
                    if (line.Tokens.Length == 0)
                        continue;

                    Token token = line.Tokens[0];
                    switch (token.Type)
                    {
                        case TokenType.Identifier:
                            if (functions.FirstOrDefault(x => x.Name == token.Value) is Function func)
                            {
                                ExecuteFunction(func, line.Tokens, variables, functions);
                            }
                            else
                            {
                                if (variables.FirstOrDefault(x => x.Name == token.Value) is Variable var)
                                {
                                    if (line.Tokens.Length < 3)
                                        throw new Exception("Variable operation syntax is incorrect. Correct syntax is, 'NAME = VALUE' where '=' can be any operator");
                                    if (line.Tokens[1].Type != TokenType.Symbol)
                                        throw new Exception("Variable operation syntax is incorrect. Correct syntax is, 'NAME = VALUE' where '=' can be any operator");

                                    object value = GetValueFromTokens(line.Tokens.Skip(2).ToArray(), variables, functions);

                                    switch(line.Tokens[1].Value)
                                    {
                                        case "=":
                                            var.Value = value;
                                            break;
                                        case "+":
                                            if (float.TryParse(var.Value.ToString(), out float varValue) && float.TryParse(value.ToString(), out float setValue))
                                                var.Value = varValue + setValue;
                                            if (float.TryParse(var.Value.ToString(), out varValue))
                                                throw new Exception("Can not add type string to number");
                                            else
                                                var.Value = var.Value.ToString() + value.ToString();
                                            break;
                                        case "-":
                                            if (float.TryParse(var.Value.ToString(), out varValue) && float.TryParse(value.ToString(), out setValue))
                                                var.Value = varValue - setValue;
                                            else
                                                throw new Exception("Can not subtract from a variable with type string");
                                            break;
                                        case "*":
                                            if (float.TryParse(var.Value.ToString(), out varValue) && float.TryParse(value.ToString(), out setValue))
                                                var.Value = varValue * setValue;
                                            else
                                                throw new Exception("Can not multiply to a variable with type string");
                                            break;
                                        case "/":
                                            if (float.TryParse(var.Value.ToString(), out varValue) && float.TryParse(value.ToString(), out setValue))
                                                var.Value = varValue / setValue;
                                            else
                                                throw new Exception("Can not divide a variable with type string");
                                            break;
                                        default: throw new Exception("Expects a correct operator, '=', '+', '-', '*', or '/'");
                                    }
                                }
                                else
                                {
                                    throw new Exception($"'{token.Value}' is not defined in this context");
                                }
                            }
                            break;
                        case TokenType.Reserved:
                            if (token.Value == "var")
                            {
                                if (line.Tokens.Length < 4)
                                {
                                    throw new Exception("Variable declaration is incorrect. Correct syntax is, 'var NAME = VALUE'");
                                }
                                if (line.Tokens[2].Value != "=")
                                {
                                    throw new Exception("Variable declaration is incorrect. Correct syntax is, 'var NAME = VALUE'");
                                }

                                string name = line.Tokens[1].Value;

                                if (variables.Select(x => x.Name).Contains(name))
                                {
                                    throw new Exception($"A variable with the name '{name}' already exists");
                                }
                                if (functions.Select(x => x.Name).Contains(name))
                                {
                                    throw new Exception($"A function with the name '{name}' already exists");
                                }
                                if (ReservedKeywrods.Contains(name))
                                {
                                    throw new Exception($"'{name}' is a reserved token and cannot be a name for a variable");
                                }

                                object? value = GetValueFromTokens(line.Tokens.Skip(3).ToArray(), variables, functions);

                                Variable var = new Variable(name, value);

                                variables = variables.Append(var).ToArray();
                            }
                            else
                            {
                                throw new Exception($"Error in tokenization, '{token.Value}' is not a reserved keyword");
                            }
                            break;
                        case TokenType.Symbol:
                        case TokenType.Variable:
                        case TokenType.Number:
                        case TokenType.String:
                            throw new Exception("Expects first token to be identifier or keyword");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + $". In line '{number_line}'");
                error_code = 1;
            }
            return error_code;
        }
        internal static object? ExecuteFunction(Function func, Token[] tokens, Variable[] variables, Function[] functions)
        {
            Token[][] parameterTokens = new Token[func.Params][];
            int paramsCount = 0;
            for (int i = 1; i < tokens.Length; i++)
            {
                if (tokens[i].Type == TokenType.Symbol && tokens[i].Value == ",")
                {
                    paramsCount++;
                    continue;
                }

                if (parameterTokens[paramsCount] == null)
                    parameterTokens[paramsCount] = [tokens[i]];
                else
                    parameterTokens[paramsCount] = [.. parameterTokens[paramsCount], tokens[i]];

            }
            object[] parameters = new object[func.Params];
            for (int i = 0; i < parameterTokens.Length; i++)
            {
                parameters[i] = GetValueFromTokens(parameterTokens[i], variables, functions);
                if (parameters[i] == null)
                    throw new Exception($"Could not extract value from function parameter '{i + 1}'");
            }
            return func.Execute(parameters);
        }
        internal static object? GetValueFromTokens(Token[] tokens, Variable[] variables, Function[] functions)
        {
            object? value = null; 

            if (tokens[0].Type == TokenType.Symbol)
            {
                if (tokens[0].Value == "(")
                {
                    int parenthesis_close = 1;
                    string equation = "(";
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        if (tokens[i].Value == ")")
                            parenthesis_close--;
                        if (tokens[i].Value == "(")
                            parenthesis_close++;

                        object? val = tokens[i].Value;
                        if (tokens[i].Type == TokenType.Variable)
                        {
                            if (variables.FirstOrDefault(x => x.Name == val.ToString()) is Variable var)
                            {
                                val = var.Value;
                            }
                            else
                            {
                                throw new Exception($"The variable '{val}' is not defined yest or doesn't exist");
                            }
                        }
                        equation += val;
                    }

                    // Use built in way to solve equation and return it's value as a float
                    System.Data.DataTable dt = new System.Data.DataTable();
                    dt.Columns.Add("expression", typeof(string), equation);
                    System.Data.DataRow row = dt.NewRow();
                    dt.Rows.Add(row);

                    value = float.Parse((string)row["expression"]);
                }
            }
            else if (tokens[0].Type == TokenType.String)
            {
                value = tokens[0].Value;
                if (tokens.Length > 1)
                {
                    if (tokens.Any(x => x.Type != TokenType.Variable && x.Type != TokenType.String))
                        throw new Exception("Cannot have value after string value");
                    else
                    {
                        string text = "";
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            string val = tokens[i].Value;
                            if (tokens[i].Type == TokenType.Variable)
                            {
                                if (variables.FirstOrDefault(x => x.Name == val.ToString()) is Variable var)
                                {
                                    val = var.Value.ToString();
                                }
                                else
                                {
                                    throw new Exception($"The variable '{val}' is not defined yest or doesn't exist");
                                }
                            }
                            text += val;
                        }
                        value = text;
                    }
                }
            }
            else if (tokens[0].Type == TokenType.Number)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[1].Type == TokenType.Symbol)
                        throw new Exception("Any equations need to be done inside parenthesis");
                    else
                        throw new Exception("Cannot have value after variable value");
                }
                value = tokens[0].Value;
            }
            else if (tokens[0].Type == TokenType.Variable)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[1].Type == TokenType.Symbol)
                        throw new Exception("Any equations need to be done inside parenthesis");
                    else
                        throw new Exception("Cannot have value after variable value");
                }
                if (variables.FirstOrDefault(x => x.Name == tokens[0].Value) is Variable var)
                {
                    value = var.Value.ToString();
                }
                else
                {
                    throw new Exception($"The variable '{tokens[0].Value}' is not defined yest or doesn't exist");
                }
            }
            else if (tokens[0].Type == TokenType.Identifier)
            {
                if (functions.FirstOrDefault(x => x.Name == tokens[0].Value) is Function func)
                {
                    if (func.Returns)
                    {
                        value = ExecuteFunction(func, tokens, variables, functions);
                    }
                    else
                    {
                        throw new Exception($"The Function '{func.Name}' does not return a value");
                    }
                }
                else
                {
                    throw new Exception($"Function '{tokens[0].Value}' does not exist");
                }
            }
            else
            {
                throw new Exception("Cannot set variable to unkown type '" + tokens[0].Value + "'");
            }
            return value;
        }
    }
}
