using System.Dynamic;
using System.Text.RegularExpressions;
using static NormaLang.Lexer;

namespace NormaLang
{
    public class Execution
    {
        private static object? Returning = null;

        public static IFunction[] Functions = IFunction.AllFunctions.ToArray();
        public static Variable[] Variables = [];
        public static Define[] Defined = [];
        public static Struct[] Structs = [];
        /*
         * This will take a Line[] and execute the code line by line
         */
        public static int Execute(Line[] lines)
        {
            int number_line = 0,
                code_index = 0,
                error_code = 0,
                last_if_line = 0;
            bool last_if_was_true = false;
            try
            {
                foreach (Line line in lines)
                {
                    code_index++;
                    number_line = line.Number;

                    if (line.Statement != null)
                    {
                        Statement statement = line.Statement;
                         
                        string input = "";
                        if (statement.Type != Statement.StatementType.For)
                        {
                            for (int i = 0; i < statement.Input.Length; i++)
                            {
                                var item = statement.Input[i];
                                if (item.Type == TokenType.Symbol)
                                {
                                    input += item.Value + " ";
                                }
                                else
                                {
                                    Token[] tokens = [item];
                                    if (item.Type == TokenType.Identifier)
                                    {
                                        tokens = statement.Input.Skip(i).ToArray();
                                        i += statement.Input.Length - 1;
                                    }
                                    else if (item.Type == TokenType.Reserved && item.Value == "none")
                                    {
                                        input += item.Value + " ";
                                        continue;
                                    }
                                    object? val = GetValueFromTokens(tokens);
                                    char[] ar = val.ToString().ToCharArray();
                                    if (val == null) input += item.Value + " ";
                                    else if (val.ToString() == "") input += "none";
                                    else if (val.ToString().ToLower() == "true") input += "True";
                                    else if (val.ToString().ToLower() == "false") input += "False";
                                    else
                                    {
                                        val = "";
                                        foreach (var c in ar)
                                        {
                                            val += int.TryParse(c.ToString(), out int num) ? num.ToString() : c.ToString() == "." ? c.ToString() : ((int)c).ToString();
                                        }
                                        input += val + " ";
                                    }
                                }
                            }
                        }

                        switch (statement.Type)
                        {
                            case Statement.StatementType.If:
                                last_if_line = code_index;
                                last_if_was_true = Statement.Evaluate(input);
                                if (last_if_was_true)
                                {
                                    Execute(statement.Lines);
                                }
                                continue;
                            case Statement.StatementType.ElIf:
                                last_if_line = code_index;
                                if (!last_if_was_true)
                                {
                                    last_if_was_true = Statement.Evaluate(input);
                                    if (last_if_was_true)
                                    {
                                        Execute(statement.Lines);
                                    }
                                }
                                continue;
                            case Statement.StatementType.Else:
                                if (last_if_line != code_index - 1)
                                {
                                    throw new Exception("Else statement needs to be directly after an if statement");
                                }
                                if (!last_if_was_true)
                                {
                                    Execute(statement.Lines);
                                }
                                continue;
                            case Statement.StatementType.While:
                                bool isnumber = int.TryParse(input, out int number);
                                int index = 0;
                                while (isnumber ? number > index : Statement.Evaluate(input))
                                {
                                    Execute(statement.Lines);
                                    index++;
                                    input = "";
                                    for (int i = 0; i < statement.Input.Length; i++)
                                    {
                                        var item = statement.Input[i];
                                        if (item.Type == TokenType.Symbol)
                                        {
                                            input += item.Value + " ";
                                        }
                                        else
                                        {
                                            Token[] tokens = [item];
                                            if (item.Type == TokenType.Identifier)
                                            {
                                                tokens = statement.Input.Skip(i).ToArray();
                                                i += statement.Input.Length - 1;
                                            }
                                            object? _val = GetValueFromTokens(tokens);
                                            if (_val == null) input += item.Value + " ";
                                            else input += _val + " ";
                                        }
                                    }
                                }
                                continue;
                            case Statement.StatementType.For:
                                if (statement.Input.Length < 3)
                                {
                                    throw new Exception("Incorrent syntax for 'for' statement. Correct syntax, 'for index in length'");
                                }
                                if (!(statement.Input[1].Type is TokenType.Reserved && statement.Input[1].Value == "in"))
                                {
                                    throw new Exception("Incorrent syntax for 'for' statement. Correct syntax, 'for index in length'");
                                }
                                string name = statement.Input[0].Value;
                                Variable var = new Variable(name, 0);
                                Variables = Variables.Append(var).ToArray();

                                object val = GetValueFromTokens(statement.Input.Skip(2).ToArray());
                                if (val is null)
                                {
                                    throw new Exception("Error getting length from for loop");
                                }

                                if (int.TryParse(val.ToString(), out int length))
                                {
                                    for (var.Value = 0; int.Parse(var.Value.ToString()) < length; var.Value = (int.Parse(var.Value.ToString()) + 1))
                                    {
                                        Variables.FirstOrDefault(x => x.Name == name).Value = var.Value;
                                        Execute(statement.Lines);
                                    }
                                }
                                else
                                {
                                    object[] vals = 
                                        (val is object[]) ? (object[])val : 
                                        (val is string) ? val.ToString().ToCharArray().Select(x => (object)x).ToArray() : 
                                        throw new Exception($"The value '{val}' is not type array or string");

                                    foreach (var item in vals)
                                    {
                                        var.Value = item;
                                        Execute(statement.Lines);
                                    }
                                }


                                Variables = Variables.Where(x => x.Name != var.Name).ToArray();
                                continue;
                        }
                    }
                    else if (line.Defined != null)
                    {
                        Defined = [.. Defined, line.Defined];
                    }
                    else if (line.Struct != null)
                    {
                        Structs = [.. Structs, line.Struct];
                    }

                    if (line.Tokens.Length == 0)
                        continue;

                    Token token = line.Tokens[0];
                    switch (token.Type)
                    {
                        case TokenType.Identifier:
                            if (Functions.FirstOrDefault(x => x.Name == token.Value) is IFunction func)
                            {
                                ExecuteFunction(func, line.Tokens);
                            }
                            else if (Defined.FirstOrDefault(x => x.Name == token.Value) is Define def)
                            {
                                ExecuteDefinedFunction(def, line.Tokens);
                            }
                            else
                            {
                                if (Variables.FirstOrDefault(x => x.Name == token.Value) is Variable var)
                                {
                                    if (line.Tokens.Length < 3)
                                        throw new Exception("Variable operation syntax is incorrect. Correct syntax is, 'NAME = VALUE' where '=' can be any operator");
                                    if (line.Tokens[1].Type != TokenType.Symbol)
                                        throw new Exception("Variable operation syntax is incorrect. Correct syntax is, 'NAME = VALUE' where '=' can be any operator");


                                    if (var.Value is object[] obj && line.Tokens[1].Value == "[" || line.Tokens[1].Value == ".")
                                    {
                                        SetComplexVariable(line.Tokens);
                                    }
                                    else
                                    {
                                        object value = GetValueFromTokens(line.Tokens.Skip(2).ToArray());

                                        switch (line.Tokens[1].Value)
                                        {
                                            case "=":
                                                var.Value = value;
                                                break;
                                            case "+":
                                                if (float.TryParse(var.Value.ToString(), out float varValue) && float.TryParse(value.ToString(), out float setValue))
                                                    var.Value = varValue + setValue;
                                                else if (var.Value is not string && float.TryParse(value.ToString(), out _))
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
                                }
                                else
                                {
                                    throw new Exception($"'{token.Value}' is not Defined in this context");
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

                                if (Functions.Select(x => x.Name).Contains(name))
                                {
                                    throw new Exception($"A function with the name '{name}' already exists");
                                }
                                if (ReservedKeywrods.Contains(name))
                                {
                                    throw new Exception($"'{name}' is a reserved token and cannot be a name for a variable");
                                }

                                object? value = GetValueFromTokens(line.Tokens.Skip(3).ToArray());

                                Variable var = new Variable(name, value);
                                if (Variables.Any(x => x.Name == name))
                                {
                                    Variables.FirstOrDefault(x => x.Name == name).Value = value;
                                }
                                else
                                {
                                    Variables = Variables.Append(var).ToArray();
                                }
                            }
                            else if (token.Value == "return")
                            {
                                object? value = GetValueFromTokens(line.Tokens.Skip(1).ToArray());
                                Returning = value;
                            }
                            else
                            {
                                throw new Exception($"Error, the reserved keyword, '{token.Value}' is not expected");
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
                Console.WriteLine(e.Message + $" -- Line: '{number_line}'");
                error_code = 1;
            }
            return error_code;
        }
        internal static void SetComplexVariable(Token[] tokens)
        {
            string[] parts = Regex.Split(string.Join("", tokens.Select(x => x.Value)), 
                @"((?=\[)|(?<=\[))|(?=\])|(?<=\])|(?=\.)|(?<=\.)|(?=\+)|(?<=\+)|(?=\-)|(?<=\-)|(?=\*)|(?<=\*)|(?=\/)|(?<=\/)|(?=\=)|(?<=\=)"
                ).Where(x => x != "" && x != " ").ToArray();

            string name = parts[0];
            Variable variable = Variables.FirstOrDefault(x => x.Name == name);
            if (variable == null) throw new Exception($"Variable '{name}' not found");

            object value = variable.Value;
            object val = value;

            int skipToken = 0;
            List<object> parentObjects = new List<object>();
            List<int> indexes = new List<int>();
            List<string> propNames = new List<string>();

            for (int i = 1; i < parts.Length; i++)
            {
                int skip = 2;
                if (parts[i].Any(x => (new char[] { '=', '+', '-', '/', '*' }).Any(y => y == x)))
                {
                    skipToken = i;
                    break;
                }
                else if (parts.Length >= i + skip)
                {
                    if (parts[i] == "[")
                    {
                        skip = 2;

                        var lexertokens = Tokenizer(parts[i + 1])[0].Tokens;
                        var index = int.Parse(GetValueFromTokens(lexertokens, true).ToString());
                        if (parts[i + 2] != "]") throw new Exception("Expected syntax 'varName[INDEX]'");
                        i += skip;

                        parentObjects.Add(val);
                        indexes.Add(index);
                        propNames.Add("");
                        val = (val as object[])[index];
                    }
                    else if (parts[i] == ".")
                    {
                        skip = 1;

                        if (val is not ExpandoObject) throw new Exception("Cannot get property from variable if the variable is not a struct instance");

                        var propName = parts[i + 1];
                        ExpandoObject expando = val as ExpandoObject;

                        parentObjects.Add(val);
                        indexes.Add(-1); // -1 indicates a property access, not array index
                        propNames.Add(propName);
                        val = ((IDictionary<string, object>)expando)[propName];

                        i += skip;
                    }
                    else
                    {
                        throw new Exception($"Expected syntax 'varName[INDEX]'");
                    }
                }
                else
                {
                    throw new Exception("Expected syntax 'varName[INDEX]'");
                }
            }

            object set = GetValueFromTokens(tokens.Skip(skipToken + 1).ToArray());

            switch (tokens[skipToken].Value)
            {
                case "=":
                    val = set;
                    break;
                case "+":
                    if (float.TryParse(val.ToString(), out float varValue) && float.TryParse(set.ToString(), out float setValue))
                        val = varValue + setValue;
                    else if (float.TryParse(val.ToString(), out _))
                        throw new Exception("Cannot add type string to number");
                    else
                        val = val.ToString() + set.ToString();
                    break;
                case "-":
                    if (float.TryParse(val.ToString(), out varValue) && float.TryParse(set.ToString(), out setValue))
                        val = varValue - setValue;
                    else
                        throw new Exception("Cannot subtract from a variable with type string");
                    break;
                case "*":
                    if (float.TryParse(val.ToString(), out varValue) && float.TryParse(set.ToString(), out setValue))
                        val = varValue * setValue;
                    else
                        throw new Exception("Cannot multiply to a variable with type string");
                    break;
                case "/":
                    if (float.TryParse(val.ToString(), out varValue) && float.TryParse(set.ToString(), out setValue))
                        val = varValue / setValue;
                    else
                        throw new Exception("Cannot divide a variable with type string");
                    break;
                default: throw new Exception("Expects a correct operator, '=', '+', '-', '*', or '/'");
            }

            // Update the original structure
            for (int i = parentObjects.Count - 1; i >= 0; i--)
            {
                if (indexes[i] == -1)
                {
                    // property access in ExpandoObject
                    var expando = (IDictionary<string, object>)parentObjects[i];
                    var propName = propNames[i]; // Calculate the property name
                    expando[propName] = val;
                    val = expando;
                }
                else
                {
                    // array index
                    var array = (object[])parentObjects[i];
                    array[indexes[i]] = val;
                    val = array;
                }
            }

            variable.Value = value;
        }
        internal static object? ExecuteDefinedFunction(Define def, Token[] tokens)
        {
            Token[][] parameterTokens = new Token[def.Parameters.Length][];
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
            object[] obj_parameters = new object[def.Parameters.Length];
            for (int i = 0; i < parameterTokens.Length; i++)
            {
                obj_parameters[i] = GetValueFromTokens(parameterTokens[i]);
                if (obj_parameters[i] == null)
                    throw new Exception($"Could not extract value from function parameter '{def.Parameters[i].Name}' in function '{def.Name}'");
            }
            Variable[] parameters = obj_parameters.Select((x, y) => new Variable(def.Parameters[y].Name, x)).ToArray();

            Variable[] outsideOfFunc = Variables.Select(x => new Variable(x.Name, x.Value)).ToArray();

            Variables = parameters;
            Execute(def.Lines);
            object? val = Returning;
            Returning = null;

            Variables = outsideOfFunc;
            return val;
        }
        internal static object? ExecuteFunction(IFunction func, Token[] tokens)
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
                parameters[i] = GetValueFromTokens(parameterTokens[i]);
                if (parameters[i] == null)
                    throw new Exception($"Could not extract value from function parameter '{i + 1}'");
            }
            return func.Execute(parameters);
        }
        internal static object? GetValueFromTokens(Token[] tokens, bool getVariableFromIdentifier = false)
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
                            val = GetValueFromTokens([tokens[i]], true);
                            if (val == null)
                            {
                                throw new Exception($"The variable '{val}' is not defined yet or doesn't exist");
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
                            object ret = null;
                            if (tokens[i].Type == TokenType.Variable)
                            {
                                string[] parts = Regex.Split(tokens[i].Value, @"((?=\[)|(?<=\[))|(?=\])|(?<=\])|(?=\.)|(?<=\.)").Where(x => x != "" && x != " ").ToArray();
                                if (Variables.FirstOrDefault(x => x.Name == val.ToString()) is Variable var)
                                {
                                    ret = var.Value;
                                }
                                else if (parts[1] == "[" || parts[1] == ".")
                                {
                                    string name = parts[0];
                                    Variable variable = Variables.FirstOrDefault(x => x.Name == name);
                                    ret = variable.Value;

                                    for (int j = 1; j < parts.Length; j++)
                                    {
                                        int skip = 2;
                                        if (parts.Length >= j + skip)
                                        {
                                            if (parts[j] == "[")
                                            {
                                                skip = 2;

                                                var lexertokens = Tokenizer(parts[j + 1])[0].Tokens;
                                                int index = int.Parse(GetValueFromTokens(lexertokens, true).ToString());
                                                if (parts[j + 2] != "]") throw new Exception("Expected syntax 'varName[INDEX]'");
                                                j += skip;

                                                ret = (ret as object[])[index];
                                            }
                                            else if (parts[j] == ".")
                                            {
                                                skip = 1;

                                                if (ret is not ExpandoObject) throw new Exception("Can not get property from variable if the variable is not a struct instance");

                                                var lexertokens = Tokenizer(parts[j + 1])[0].Tokens;
                                                string propName = lexertokens[0].Value;
                                                ExpandoObject expando = ret as ExpandoObject;

                                                ret = ((IDictionary<string, object>)expando)[propName];

                                                j += skip;
                                            }
                                            else
                                            {
                                                throw new Exception($"Expected syntax 'varName[INDEX]'");
                                            }

                                            if (ret is Variable v)
                                            {
                                                ret = v.Value;
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Expected syntax 'varName[INDEX]'");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception($"The variable '{val}' is not defined yet or doesn't exist");
                                }

                                if (ret is object[] arr)
                                {
                                    ret = ArrayToText(arr);
                                }
                                if (ret is ExpandoObject e)
                                {
                                    ret = StructToText(e);
                                }
                                val = ret.ToString();
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
                value = float.Parse(tokens[0].Value);
            }
            else if (tokens[0].Type == TokenType.Variable)
            {
                string[] parts = Regex.Split(tokens[0].Value, @"((?=\[)|(?<=\[))|(?=\])|(?<=\])|(?=\.)|(?<=\.)").Where(x => x != "" && x != " ").ToArray();
                if (tokens.Length > 1 && !tokens.Any(x => x.Value == "." || x.Value == "[")) 
                {
                    if (tokens[1].Type == TokenType.Symbol)
                        throw new Exception("Any equations need to be done inside parenthesis");
                    else
                        throw new Exception("Cannot have value after variable value");
                }
                if (Variables.FirstOrDefault(x => x.Name == tokens[0].Value) is Variable var)
                {
                    if (var.Value is Variable v) value = v.Value;
                    else value = var.Value;
                }
                else if (parts[1] == "[" || parts[1] == ".")
                {
                    string name = parts[0];
                    Variable variable = Variables.FirstOrDefault(x => x.Name == name);
                    object val = variable.Value;
                    bool valIsString = false;

                    if (val is string)
                    {
                        if (parts[1] == "[")
                        {
                            var lexertokens = Tokenizer(parts[2])[0].Tokens;
                            int index = int.Parse(GetValueFromTokens(lexertokens, true).ToString());
                            if (parts.Length > 3)
                            {
                                if (parts[3] == "]")
                                {
                                    val = val.ToString()[index].ToString();
                                    valIsString = true;
                                }
                            }
                        }
                    }

                    for (int i = 1; i < parts.Length && !valIsString; i++)
                    {
                        int skip = 2;
                        if (parts.Length >= i + skip)
                        {
                            if (parts[i] == "[")
                            {
                                skip = 2;

                                var lexertokens = Tokenizer(parts[i + 1])[0].Tokens;
                                int index = int.Parse(GetValueFromTokens(lexertokens, true).ToString());
                                if (parts[i + 2] != "]") throw new Exception("Expected syntax 'varName[INDEX]'");
                                i += skip;

                                val = (val as object[])[index];
                            }
                            else if (parts[i] == ".")
                            {
                                skip = 1;

                                if (val is not ExpandoObject) throw new Exception("Can not get property from variable if the variable is not a struct instance");

                                var lexertokens = Tokenizer(parts[i + 1])[0].Tokens;
                                string propName = lexertokens[0].Value;
                                ExpandoObject expando = val as ExpandoObject;

                                val = ((IDictionary<string, object>)expando)[propName];

                                i += skip;
                            }
                            else
                            {
                                throw new Exception($"Expected syntax 'varName[INDEX]'");
                            }

                            if (val is Variable v)
                            {
                                val = v.Value;
                            }
                        }
                        else
                        {
                            throw new Exception("Expected syntax 'varName[INDEX]'");
                        }
                    }

                    value = val;
                }
                else
                {
                    throw new Exception($"The variable '{tokens[0].Value}' is not defined yet or doesn't exist");
                }
            }
            else if (tokens[0].Type == TokenType.Identifier)
            {
                if (Functions.FirstOrDefault(x => x.Name == tokens[0].Value) is IFunction func)
                {
                    if (func.Returns)
                    {
                        value = ExecuteFunction(func, tokens);
                    }
                    else
                    {
                        throw new Exception($"The Function '{func.Name}' does not return a value");
                    }
                }
                else if (Defined.FirstOrDefault(x => x.Name == tokens[0].Value) is Define def)
                {
                    value = ExecuteDefinedFunction(def, tokens);
                    if (value is null)
                    {
                        throw new Exception($"The Function '{def.Name}' did not return a value");
                    }
                }
                else if (Structs.FirstOrDefault(x => x.Name == tokens[0].Value) is Struct struc)
                {
                    if (tokens[1].Value == ":")
                    {
                        dynamic dyn = new ExpandoObject();
                        Token[] propValuesTok = [];

                        bool lastWasComma = true;
                        for (int i = 2; i < tokens.Length; i++)
                        {
                            Token tok = tokens[i];
                            if (lastWasComma)
                            {
                                if (tok.Value == ",")
                                {
                                    throw new Exception($"Incorrect syntax in struct '{struc.Name}', no commas were found in between property names");
                                }
                                else
                                {
                                    propValuesTok = [.. propValuesTok, tok];
                                    lastWasComma = false;
                                }
                            }
                            else
                            {
                                if (tok.Value == ",")
                                {
                                    lastWasComma = true;
                                }
                                else
                                {
                                    throw new Exception($"Incorrect syntax in struct '{struc.Name}', two commas ',,' were found with no property in between");
                                }
                            }
                        }

                        object[] propValues = propValuesTok.Select(x => GetValueFromTokens([x])).ToArray();

                        ((IDictionary<string, object>)dyn)["__struct_name"] = struc.Name;

                        for (int i = 0; i < struc.Properties.Length; i++)
                        {
                            ((IDictionary<string, object>)dyn)[struc.Properties[i]] = propValues[i];
                        }

                        value = dyn;
                    }
                    else
                    {
                        throw new Exception($"Error in struct sytnax, expected ':' to call struct properties, '{struc.Name}': someValue, ...");
                    }
                }
                else if (getVariableFromIdentifier && tokens.Length == 1 && Variables.FirstOrDefault(x => x.Name == tokens[0].Value) is Variable var)
                {
                    value = var.Value;
                }
                else
                {
                    throw new Exception($"Function '{tokens[0].Value}' does not exist");
                }
            }
            else if (tokens[0].Type == TokenType.Reserved)
            {
                if (tokens[0].Value == "true") value = true;
                else if (tokens[0].Value == "false") value = false;
                else if (tokens[0].Value == "none") value = "";
                else if (tokens[0].Value == "point")
                {
                    if (tokens[1].Value == ":" && tokens.Length == 3)
                    {
                        string name = tokens[2].Value;
                        Variable variable = Variables.FirstOrDefault(x => x.Name == name);
                        if (variable == null) throw new Exception($"Variable '{name}' does not exist");
                        value = variable;
                    }
                    else
                    {
                        throw new Exception("Incorrect syntax for point keyword. Correct syntax 'point: $varName$'");
                    }
                }
                else throw new Exception("Cannot get reserved keyword '" + tokens[0].Value + "' as value");
            }
            else if (tokens[0].Type == TokenType.Array)
            {
                string[] parts = tokens[0].Value.ToCharArray().Select(x => x.ToString()).ToArray();
                object[] values = ParseArrayFromString(parts, 0, out _);
                values = GetArrayValues(values);
                value = values;
            }
            else
            {
                throw new Exception("Cannot get unkown type '" + tokens[0].Value + "'");
            }
            return value;
        }
        internal static string ArrayToText(object[] objects)
        {
            string text = "[";
            for (int i = 0; i < objects.Length; i++)
            {
                object obj = objects[i];
                if (obj is object[] o) obj = ArrayToText(o);
                if (obj is ExpandoObject e) obj = StructToText(e);
                text += obj + (i != objects.Length - 1 ? ", " : "");
            }
            return text + "]";
        }
        internal static string StructToText(ExpandoObject expando)
        {
            string text = "";
            var properties = (IDictionary<string, object>)expando;
            for (int i = 0; i < properties.Count; i++)
            {
                object key = properties.Keys.ToArray()[i];
                object value = properties.Values.ToArray()[i];

                if (value is string s) value = s == "" ? "none" : $"\"{s}\"";
                if (value is object[] o) value = ArrayToText(o);
                if (value is ExpandoObject e) value = StructToText(e);

                object obj = $"{key}={value}";               
                text += obj + (i != properties.Count - 1 ? ", " : "");
            }

            return $"{{ {text} }}";
        }
        internal static object[] GetArrayValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is object[])
                {
                    values[i] = GetArrayValues(values[i] as object[]);
                }
                else if (values[i].ToString().StartsWith("point:"))
                {
                    string[] split = values[i].ToString().Split(":");
                    if(split.Length > 2)
                    {
                        throw new Exception("Incorrect pointer syntax, expected 'point:$varName$'");
                    }
                    string name = split[1].Remove(split[1].Length - 1, 1).Remove(0, 1);
                    Variable variable = Variables.FirstOrDefault(x => x.Name == name);
                    if (variable == null)
                    {
                        throw new Exception($"The variable '{name}' does not exist");
                    }
                    values[i] = variable;
                }
                else
                {
                    var lexertokens = Tokenizer(values[i].ToString())[0].Tokens;
                    values[i] = GetValueFromTokens(lexertokens);
                    if (values[i] is ExpandoObject expando)
                    {
                        var newExpando = new ExpandoObject();
                        var dictionary = (IDictionary<string, object>)newExpando;

                        foreach (var kvp in (IDictionary<string, object>)expando)
                        {
                            dictionary[kvp.Key] = kvp.Value;
                        }
                        values[i] = newExpando;
                    }
                }
            }
            return values;
        }
        internal static object[] ParseArrayFromString(string[] parts, int start, out int i)
        {
            object[] array = [];
            object? obj = null;
            int bracketCount = 0;
            bool lastWasComma = false;
            bool isQuote = false;
            bool isVar = false;

            for (i = start; i < parts.Length; i++)
            {
                string part = parts[i];

                if (part == "$")
                {
                    isVar = !isVar;
                    obj += "$";
                }
                else if (isVar)
                {
                    obj += part;
                }
                else if (part == "[")
                {
                    bracketCount++;
                    lastWasComma = true;
                    if (bracketCount > 1)
                    {
                        obj = ParseArrayFromString(parts, i, out i);
                        array = [.. array, obj];
                        i++;
                        obj = null;
                        lastWasComma = true;
                    }
                }
                else if (part == "]")
                {
                    bracketCount--;
                    lastWasComma = false;
                    if (array.Length != 0)
                        array = [.. array, obj];
                }
                else if (part == "\"")
                {
                    isQuote = !isQuote;
                    obj += "\"";
                }
                else if (isQuote)
                {
                    obj += part;
                }
                else if (part == ",")
                {
                    if (lastWasComma && obj == null) throw new Exception("Incorrect syntax in array, two commas were next to eachother with no values in between");
                    lastWasComma = true;
                    array = [.. array, obj];
                    obj = null;
                }
                else if (part == " " && obj != null && parts.Skip(i).TakeWhile(x => x != "," && x != "]").Where(x => x != " ").Count() != 0)
                {
                    lastWasComma = false;
                }
                else if (!lastWasComma)
                {
                    throw new Exception("Incorrect syntax in array, two values were not seperated by a comma");
                }
                else if (lastWasComma)
                {
                    obj = part == " " ? obj : obj + part;
                }

                if (bracketCount == 0) break;
            }

            return array;
        }
    }
}
