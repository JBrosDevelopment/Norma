using System.Data;
using System.Text.RegularExpressions;
using static NormaLang.Lexer;

namespace NormaLang
{
    public class Statement
    {
        public enum StatementType
        {
            If, Else, ElIf, While, For
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
        public static string[] StatementTypes = ["if", "elif", "else", "while", "for"];
        public static bool Evaluate(string input)
        {
            // Evaluate input and return bool as output

            // Convert string to number
            char[] splitter = [' ', '=', '>', '<', '!'];
            string[] parts = SplitWithDelimiters(input, splitter).Where(x => x.Trim() != "").ToArray();
            string output = "";
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.ToLower() == "true" || part.ToLower() == "false") output += part;
                else if (part == "!" || part == ">" || part == "<")
                {
                    if (parts[i + 1] == "=")
                    {
                        output += $"{part}=".Replace("!=", "<>");
                        i++;
                    }
                    else
                    {
                        output += part.Replace("!=", "<>");
                    }
                }
                else if (splitter.Contains(part[0]))
                {
                    output += part;
                }
                else if (!float.TryParse(part, out _))
                {
                    output += string.Join("", part.ToCharArray().Select(Convert.ToInt32));
                }
                else
                {
                    output += part;
                }
            }
            
            // Evaluate Number
            string expression = output;
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(bool));

            DataRow row = table.NewRow();
            row["expression"] = DBNull.Value;
            table.Rows.Add(row);

            table.Columns["expression"].Expression = expression;

            return (bool)row["expression"];
        }
        private static string[] SplitWithDelimiters(string input, char[] delimiters)
        {
            string pattern = $"({string.Join("|", delimiters.Select(c => Regex.Escape(c.ToString())))})"; 
            return Regex.Split(input, pattern); 
        }
    }
}
