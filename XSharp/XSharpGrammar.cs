using System;
using Irony.Parsing;

namespace XSharp
{
    [Language("X#", "0.1", "DSL for XRM")]
    public class XSharpGrammar : Grammar
    {
        public XSharpGrammar()
        {
            #region Terminals

            var number = new NumberLiteral("number") { DefaultIntTypes = new[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt } };
            var str = new StringLiteral("str", "\"");
            var entityCollection = new IdentifierTerminal("entityCollection");
            var entityName = new IdentifierTerminal("entityName");
            var attribute = new IdentifierTerminal("attribute");
            var guid = new StringLiteral("guid", "\"");
            var fetchXml = new StringLiteral("fetchXml", "|", StringOptions.AllowsLineBreak);
            var comment = new CommentTerminal("comment", "#", "\r", "\n");
            NonGrammarTerminals.Add(comment);
            #endregion Terminals

            #region Non-terminals

            // ReSharper disable InconsistentNaming
            var Program = new NonTerminal("Program");
            var StatementList = new NonTerminal("StatementList");
            var StatementLine = new NonTerminal("StatementLine");
            var Statement = new NonTerminal("Statement");
            var CommandStatement = new NonTerminal("CommandStatement");
            var UnaryCommand = new NonTerminal("UnaryCommand");
            var BinaryCommand = new NonTerminal("BinaryCommand");
            var AssignStatement = new NonTerminal("AssignStatement");
            var QueryStatement = new NonTerminal("QueryStatement");
            var OrdinalOperator = new NonTerminal("OrdinalOperator", "operator");
            var FilterStatementList = new NonTerminal("FilterStatementList");
            var FilterStatement = new NonTerminal("FilterStatement");
            var FilterOperator = new NonTerminal("FilterOperator");
            var FilterExpression = new NonTerminal("FilterExpression");
            var CreateStatement = new NonTerminal("CreateStatement");
            var CreateAndInitializeStatement = new NonTerminal("CreateAndInitializeStatement");
            var ChangeStatement = new NonTerminal("ChangeStatement");
            var SetStatementList = new NonTerminal("SetStatementList");
            var SetStatement = new NonTerminal("SetStatement");
            var ValueExpression = new NonTerminal("ValueExpression");
            var BinaryOperator = new NonTerminal("BinaryOperator", "operator");
            // ReSharper restore InconsistentNaming
            #endregion Non-terminals

            #region BNF rules

            Program.Rule = StatementList;
            StatementList.Rule = MakeStarRule(StatementList, StatementLine);
            StatementLine.Rule = Statement + NewLine | NewLine;
            Statement.Rule = ChangeStatement | AssignStatement | CommandStatement;
            ChangeStatement.Rule = "change" + entityCollection + "{" + NewLine + SetStatementList + "}";
            CommandStatement.Rule = UnaryCommand + entityCollection
                                    | BinaryCommand + entityCollection + ToTerm("to") + entityCollection;
            AssignStatement.Rule = entityCollection + "<-" + QueryStatement
                                   | entityCollection + "<-" + CreateStatement
                                   | entityCollection + "<-" + CreateAndInitializeStatement;
            UnaryCommand.Rule = ToTerm("create") | "update" | "delete";
            BinaryCommand.Rule = ToTerm("assign") | "merge";
            QueryStatement.Rule = "find" + OrdinalOperator + entityName + "where" + FilterStatementList
                                  | "find" + OrdinalOperator + entityName + "called" + guid
                                  | fetchXml;
            OrdinalOperator.Rule = ToTerm("all") | "one";
            FilterStatementList.Rule = MakeStarRule(FilterStatementList, FilterStatement);
            FilterStatement.Rule = FilterExpression | FilterExpression + FilterOperator;
            FilterOperator.Rule = ToTerm("and") | "or";
            FilterExpression.Rule = attribute + BinaryOperator + ValueExpression;
            CreateStatement.Rule = "new" + entityName;
            CreateAndInitializeStatement.Rule = "new" + entityName + "called" + guid;
            SetStatementList.Rule = MakeStarRule(SetStatementList, SetStatement);
            SetStatement.Rule = "set" + attribute + "to" + ValueExpression + NewLine;
            BinaryOperator.Rule = ToTerm("=") | "!=" | "<" | ">" | "<=" | ">=";
            ValueExpression.Rule = str | number;
            
            Root = Program;

            #endregion

            #region Operators

            RegisterOperators(1, "and", "or");

            #endregion Operators

            #region Punctuation and Transient Terms

            MarkPunctuation("{", "}");
            MarkTransient(StatementLine, BinaryOperator, OrdinalOperator, FilterOperator, ValueExpression);

            LanguageFlags = LanguageFlags.NewLineBeforeEOF | LanguageFlags.CreateAst;

            #endregion Punctuation and Transient Terms
        }
    }
}
