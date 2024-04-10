using System.Data;
using static CustomLang.Lexer;

namespace CustomLang
{
    public class Statement
    {
        public enum StatementType
        {
            If, Else, ElIf, While
        }
        public Statement(Token[] input, Line[] lines, StatementType type)
        {
            Input = input;
            Lines = lines;
            Type = type;
        }
        public StatementType Type { get; set; }
        public Token[] Input { get; set; }
        public Line[] Lines { get; set; }
        public static bool Evaluate(string input)
        {
            // Evaluate input and return bool as output
            
            // Convert string to number
            var allchars = input.Replace("and", "&").Replace("or", "|").Replace("not", "!").ToCharArray();
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (isAlpha(allchars[i]))
                {
                    output += ((int)allchars[i]).ToString();
                }
                else
                {
                    output += allchars[i];
                }
            }
            
            // Evaluate Number
            string expression = output.Replace("!", "<>").Replace("||", "or").Replace("&&", "and").Replace("&", "and").Replace("|", "or");
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(bool));

            DataRow row = table.NewRow();
            row["expression"] = DBNull.Value;
            table.Rows.Add(row);

            table.Columns["expression"].Expression = expression;

            return (bool)row["expression"];
        }
    }
}
