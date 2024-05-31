using static NormaLang.Lexer;

namespace NormaLang
{
    public class Define
    {
        public string Name { get; set; }
        public Line[] Lines { get; set; }
        public Variable[] Parameters { get; set; }
        public Define(string name, Line[] lines, Variable[] parameters)
        {
            Name = name;
            Lines = lines;
            Parameters = parameters;
        }
    }
}
