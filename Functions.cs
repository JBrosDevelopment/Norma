using System.Dynamic;

namespace NormaLang 
{
    /* 
     * This interface is for any functions that want to be created:
     * 
     * public class FUNCTION_NAME : IFunction {
     * 
     *     public string Name { get; } = nameof(FUNCTION_NAME).ToLower();
     *     public int Params { get; } = NUMBER_OF_PARAMETERS;
     *     public bool Returns { get; } = IF_THE_FUNCTION_RETURNS_A_VALUE;
     *     
     *     public object? Execute(object[] args)
     *     {
     *         var param1 = args[0];
     *         return null;
     *     }
     * }
     */
    public interface IFunction 
    {
        public static List<IFunction> AllFunctions { get; set; } = new List<IFunction>()
        {
            // console:
            new FPrint(),
            new FInput(),
            new FClear(),
            // utitlity:
            new FExit(),
            new FRunCode(),
            new FReadFile(),
            new FWriteFile(),
            new FCreateFile(),
            new FDeleteFile(),
            new FEnvPath(),
            new FEnvDir(),
            new FIntToChar(),
            new FRandom(),
            // stirng:
            new FSubstring(),
            new FIndexOf(),
            new FToLower(),
            new FToUpper(),
            new FTrim(),
            new FReverse(),
            // array:
            new FLength(),
            new FAppend(),
            new FRemove(),
            new FInsert(),
            new FSlice(),
            new FConcat(),
            new FSort(),
        };
        public string Name { get; }
        public int Params { get; }
        public bool Returns { get; }
        public abstract object? Execute(object[] args);
        internal static string ValueToString(object value)
        {
            if (value is object[] o)
            {
                return Execution.ArrayToText(o);
            }
            if (value is ExpandoObject e)
            {
                return Execution.StructToText(e);
            }
            else
            {
                return value.ToString()!;
            }
        }
    }
    #region Console
    /*
     * The print function for Norma
     * takes input and prints it to the console
     */
    public class FPrint : IFunction 
    {
        public string Name { get; } = "print";
        public int Params { get; } = 1;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            string text = IFunction.ValueToString(args[0]);
            Console.WriteLine(text);

            return null;
        }
    }
    /*
     * The input function for Norma
     * Returns the input from the console
     */
    public class FInput : IFunction
    {
        public string Name { get; } = "input";
        public int Params { get; } = 0;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            return Console.ReadLine();
        }
    }
    /*
     * The clear function for Norma
     * Clears the console
     */
    public class FClear : IFunction
    {
        public string Name { get; } = "clear";
        public int Params { get; } = 0;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            Console.Clear();
            return null;
        }
    }
    #endregion
    #region Utility
    public class FExit : IFunction
    {
        public string Name { get; } = "exit";
        public int Params { get; } = 0;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            Environment.Exit(0);
            return null;
        }
    }
    public class FRunCode : IFunction
    {
        public string Name { get; } = "runCode";
        public int Params { get; } = 1;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            var code = IFunction.ValueToString(args[0]);

            Interprete.RunCode(code, Interprete.FilePath);

            return null;
        }
    }
    public class FReadFile : IFunction
    {
        public string Name { get; } = "readFile";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var file = IFunction.ValueToString(args[0]);
            return File.ReadAllText(file);
        }
    }
    public class FWriteFile : IFunction
    {
        public string Name { get; } = "writeFile";
        public int Params { get; } = 2;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            var file = IFunction.ValueToString(args[0]);
            var text = IFunction.ValueToString(args[1]);
            File.WriteAllText(file, text);
            return null;
        }
    }
    public class FCreateFile : IFunction
    {
        public string Name { get; } = "createFile";
        public int Params { get; } = 1;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            var file = IFunction.ValueToString(args[0]);
            File.Create(file).Close();
            return null;
        }
    }
    public class FDeleteFile : IFunction
    {
        public string Name { get; } = "deleteFile";
        public int Params { get; } = 1;
        public bool Returns { get; } = false;
        public object? Execute(object[] args)
        {
            var file = IFunction.ValueToString(args[0]);
            File.Delete(file);
            return null;
        }
    }
    public class FEnvPath : IFunction
    {
        public string Name { get; } = "envPath";
        public int Params { get; } = 0;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            return Interprete.FilePath;
        }
    }
    public class FEnvDir : IFunction
    {
        public string Name { get; } = "envDir";
        public int Params { get; } = 0;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            return Directory.GetParent(Interprete.FilePath).FullName;
        }
    }
    public class FIntToChar : IFunction
    {
        public string Name { get; } = "intToChar";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            string hex = IFunction.ValueToString(args[0]);
            return (char)Convert.ToInt32(hex, 16);
        }
    }
    public class FRandom : IFunction
    {
        public string Name { get; } = "random";
        public int Params { get; } = 2;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var min = int.Parse(IFunction.ValueToString(args[0]));
            var max = int.Parse(IFunction.ValueToString(args[1]));
            return new Random().Next(min, max);
        }
    }
    #endregion
    #region String
    public class FSubstring : IFunction
    {
        public string Name { get; } = "substring";
        public int Params { get; } = 3;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var text = IFunction.ValueToString(args[0]);
            var start = int.Parse(IFunction.ValueToString(args[1]));
            var length = int.Parse(IFunction.ValueToString(args[2]));
            return text?.Substring(start, length);
        }
    }
    public class FIndexOf : IFunction
    {
        public string Name { get; } = "indexOf";
        public int Params { get; } = 2;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var text = IFunction.ValueToString(args[0]);
            var name = IFunction.ValueToString(args[1]);
            return text?.IndexOf(name!);
        }
    }
    public class FToUpper : IFunction
    {
        public string Name { get; } = "toUpper";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var text = IFunction.ValueToString(args[0]);
            return text?.ToUpper();
        }
    }
    public class FToLower : IFunction
    {
        public string Name { get; } = "toLower";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var text = IFunction.ValueToString(args[0]);
            return text?.ToLower();
        }
    }
    public class FTrim : IFunction
    {
        public string Name { get; } = "trim";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            var text = IFunction.ValueToString(args[0]);
            return text?.Trim();
        }
    }
    public class FReverse : IFunction
    {
        public string Name { get; } = "reverse";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array)
            {
                object[] ar = array.Reverse().ToArray();
                return ar;
            }
            else if (args[0] is string str)
            {
                return new string(str.Reverse().ToArray());
            }
            else
            {
                throw new Exception("Argument must be an array or a string");
            }
        }
    }
    #endregion
    #region Array
    public class FLength : IFunction
    {
        public string Name { get; } = "length";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is Array array)
            {
                return array.Length;
            }
            else if (args[0] is string str)
            {
                return str.Length;
            } 
            else
            {
                throw new Exception("Can not get length of variable that isn't type array or string");
            }
        }
    }
    public class FAppend : IFunction
    {
        public string Name { get; } = "append";
        public int Params { get; } = 2;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array)
            {
                return array.Append(args[1]).ToArray();
            }
            else
            {
                throw new Exception("Can not append to variable that is not array");
            }
        }
    }
    public class FRemove : IFunction
    {
        public string Name { get; } = "remove";
        public int Params { get; } = 2;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array)
            {
                var index = int.Parse(IFunction.ValueToString(args[1]));
                if (index < 0 || index >= array.Length)
                {
                    throw new Exception("Index out of bounds");
                }
                return array.Where((_, i) => i != index).ToArray();
            }
            else
            {
                throw new Exception("Cannot remove from a variable that is not an array");
            }
        }
    }
    public class FInsert : IFunction
    {
        public string Name { get; } = "insert";
        public int Params { get; } = 3;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array)
            {
                var index = int.Parse(IFunction.ValueToString(args[1]));
                if (index < 0 || index > array.Length)
                {
                    throw new Exception("Index out of bounds");
                }
                var list = array.ToList();
                list.Insert(index, args[2]);
                return list.ToArray();
            }
            else
            {
                throw new Exception("Cannot insert into a variable that is not an array");
            }
        }
    }
    public class FSlice : IFunction
    {
        public string Name { get; } = "slice";
        public int Params { get; } = 3;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array)
            {
                var start = int.Parse(IFunction.ValueToString(args[1]));
                var length = int.Parse(IFunction.ValueToString(args[2]));
                if (start < 0 || start >= array.Length || length < 0 || start + length > array.Length)
                {
                    throw new Exception("Invalid start or length for slice operation");
                }
                return array.Skip(start).Take(length).ToArray();
            }
            else
            {
                throw new Exception("Cannot slice a variable that is not an array");
            }
        }
    }
    public class FConcat : IFunction
    {
        public string Name { get; } = "concat";
        public int Params { get; } = 2;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array1 && args[1] is object[] array2)
            {
                return array1.Concat(array2).ToArray();
            }
            else
            {
                throw new Exception("Both arguments must be arrays");
            }
        }
    }

    public class FSort : IFunction
    {
        public string Name { get; } = "sort";
        public int Params { get; } = 1;
        public bool Returns { get; } = true;
        public object? Execute(object[] args)
        {
            if (args[0] is object[] array)
            {
                var sortedArray = array.OrderBy(x => x).ToArray();
                return sortedArray;
            }
            else
            {
                throw new Exception("Argument must be an array");
            }
        }
    }
    #endregion
}