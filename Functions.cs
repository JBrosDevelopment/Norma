namespace CustomLang 
{
    public abstract class Function 
    {
        public string Name { get; set; } = "";
        public int Params { get; set; } = 0;
        public abstract void Execute(object[] args);
    }
    public class Print : Function 
    {
        public Print()
        {
            Name = "print";
            Params = 1;
        }
        public override void Execute(object[] args)
        {
            string text = string.Join("", args.Select(x => x.ToString()));
            Console.WriteLine(text);
        }
    }
    public class ParseAdd : Function
    {
        public ParseAdd()
        {
            Name = "parseAdd";
            Params = 2;
        }
        public override void Execute(object[] args)
        {
            float text = float.Parse(args[0].ToString());
            float add = float.Parse(args[1].ToString());

            float result = text + add;

            Console.WriteLine(result);
        }
    }
}