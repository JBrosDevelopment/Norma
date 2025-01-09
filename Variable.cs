namespace NormaLang
{
    public class Variable
    {
        public string Name { get; set; }
        public object? Value { get; set; }
        public bool Keep { get; set; }
        public Variable(string name, object? value, bool keep = false)
        {
            Name = name;
            Value = value;
            Keep = keep;
        }
    }
}
