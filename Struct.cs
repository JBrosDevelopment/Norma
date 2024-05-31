namespace NormaLang
{
    public class Struct
    {
        public string Name { get; set; }
        public string[] Properties { get; set; }
        public Struct(string name, string[] properties)
        {
            Name = name;
            Properties = properties;
        }
    }
}
