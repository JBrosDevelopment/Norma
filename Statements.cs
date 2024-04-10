using System.Data;
using System.Linq.Expressions;
using static CustomLang.Lexer;

namespace CustomLang
{
    public abstract class Statement
    {
        public static Statement[] Statements { get; } = [
            new If(),

            ];
        public abstract string Name { get; set; }
        public abstract Line[] Lines { get; set; }
        public abstract void Execute(string input, Function[] functions, Variable[] variables);
        public static bool Evaluate(string input)
        {
            // Evaluate input and return bool as output
            string expression = input.Replace("||", "or").Replace("&&", "and").Replace("&", "and").Replace("|", "or");
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(bool));

            DataRow row = table.NewRow();
            row["expression"] = DBNull.Value;
            table.Rows.Add(row);

            table.Columns["expression"].Expression = expression;

            return (bool)row["expression"];
        }
    }
    public class If : Statement
    {
        public If() { }
        public If(Line[] lines)
        {
            Lines = lines;
        }
        public override Line[] Lines { get; set; } = [];
        public override string Name { get; set; } = "if";
        public override void Execute(string input, Function[] functions, Variable[] variables)
        {
            if (Evaluate(input))
            {
                Execution.Execute(Lines, functions, variables);
            }
        }
    }
    public class Else : Statement
    {
        public Else() { }
        public Else(Line[] lines)
        {
            Lines = lines;
        }
        public override Line[] Lines { get; set; } = [];
        public override string Name { get; set; } = "else";
        public override void Execute(string input, Function[] functions, Variable[] variables)
        {
            Execution.Execute(Lines, functions, variables);
        }
    }
}
