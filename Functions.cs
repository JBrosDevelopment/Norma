namespace CustomLang 
{
    public abstract class Function 
    {
        public string Name { get; set; } = "";
        public int Params { get; set; } = 0;
        public bool Returns { get; set; } = false;
        public abstract object? Execute(object[] args);
    }
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
            string text = string.Join("", args.Select(x => x.ToString()));
            Console.WriteLine(text);

            return null;
        }
    }
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
}