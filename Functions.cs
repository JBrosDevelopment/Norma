namespace NormaLang 
{
    /* 
     * This class is for any functions that want to be created.
     * public class FUNCTIONNAME : Function
     */
    public abstract class Function 
    {
        public static List<Function> AllFunctions { get; } = new List<Function>();
        public string Name { get; set; } = "";
        public int Params { get; set; } = 0;
        public bool Returns { get; set; } = false;
        public abstract object? Execute(object[] args);
        public Function()
        {
            AllFunctions.Add(this);
        }
        public static Function[] InstantiateFunctions()
        {
            // Use Reflection to get all Functions dynamically
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly(); // Create Assembly Reference 
            Type[] functionTypes = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Function))).ToArray(); // Get all Function References into an array

            foreach (Type functionType in functionTypes) // loop through each index in the functionTypes array
            {
                Function function = (Function)Activator.CreateInstance(functionType); // Create a new Function instance 
            }
            // Now, 'Function.AllFunctions' can be used because all functions have been instantiated causing them to be added to the list

            return AllFunctions.ToArray();
        }
    }
    /*
     * The print function for CLang
     * takes input and prints it to the console
     */
    public class Print : Function 
    {
        public Print()
        {
            Name = "print";
            Params = 1;
            Returns = false;
        }
        public override object? Execute(object[] args)
        {
            string text = args[0].ToString();
            Console.WriteLine(text);

            return null;
        }
    }
    /*
     * The parse function for CLang
     * Parses string and returns a float
     */
    public class Parse : Function
    {
        public Parse()
        {
            Name = "parse";
            Params = 1;
            Returns = true;
        }
        public override object? Execute(object[] args)
        {
            float result = float.Parse(args[0].ToString());

            return result;
        }
    }
    /*
     * The input function for CLang
     * Returns the input from the console
     */
    public class Input : Function
    {
        public Input()
        {
            Name = "input";
            Params = 0;
            Returns = true;
        }
        public override object? Execute(object[] args)
        {
            return Console.ReadLine();
        }
    }
    /*
     * The clear function for CLang
     * Clears the console
     */
    public class Clear : Function
    {
        public Clear()
        {
            Name = "clear";
            Params = 0;
            Returns = false;
        }
        public override object? Execute(object[] args)
        {
            Console.Clear();
            return null;
        }
    }
    /*
     * The length function for array length
     * returns the length of the array
     */
    public class Length : Function
    {
        public Length()
        {
            Name = "length";
            Params = 1;
            Returns = true;
        }
        public override object? Execute(object[] args)
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
    /*
     * The append function for array length
     * returns the array with a value appended to it
     */
    public class Append : Function
    {
        public Append()
        {
            Name = nameof(Append).ToLower();
            Params = 2;
            Returns = true;
        }
        public override object? Execute(object[] args)
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
}