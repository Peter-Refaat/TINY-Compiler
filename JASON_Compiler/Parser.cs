using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;
        public string Lexeme;
        public Node(string N)
        {
            this.Name = N;
            this.Lexeme = ""; // أو null
        }
        public Node(string N, string lex) { this.Name = N; this.Lexeme = lex; }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        // ----------- Utilities -----------
        Token_Class Peek()
        {
            // Return End token when we're past the stream (safer than 0).
            if (InputPointer < TokenStream.Count) return TokenStream[InputPointer].token_type;
            return Token_Class.End;
        }

        bool IsToken(Token_Class t)
        {
            if (InputPointer < TokenStream.Count) return TokenStream[InputPointer].token_type == t;
            return false;
        }

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            // Show success only if no errors (you can inspect Errors.Error_List)
            if (Errors.Error_List.Count == 0)
                MessageBox.Show("Parsing Completed Successfully!");
            else
                MessageBox.Show("Parsing completed with " + Errors.Error_List.Count + " errors.");
            return root;
        }

        // Program → FunctionList MainFunction
        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(FunctionList());
            program.Children.Add(MainFunction());
            return program;
        }

        // MainFunction → Datatype main ( ) FunctionBody
        Node MainFunction()
        {
            Node mainFunction = new Node("MainFunction");
            mainFunction.Children.Add(Datatype());
            mainFunction.Children.Add(match(Token_Class.Main));
            mainFunction.Children.Add(match(Token_Class.LParenthesis));
            mainFunction.Children.Add(match(Token_Class.RParenthesis));
            mainFunction.Children.Add(FunctionBody());
            return mainFunction;
        }

        // FunctionList → FunctionStatement FunctionList | ε
        Node FunctionList()
        {
            Node functionList = new Node("FunctionList");
            Token_Class t = Peek();

            if (t == Token_Class.Int || t == Token_Class.Float || t == Token_Class.StringKw)
            {
                // Look ahead to check if it's not main (i.e. it's a function)
                if (InputPointer + 1 < TokenStream.Count &&
                    TokenStream[InputPointer + 1].token_type != Token_Class.Main)
                {
                    functionList.Children.Add(FunctionStatement());
                    functionList.Children.Add(FunctionList());
                }
            }
            // else epsilon
            return functionList;
        }

        // FunctionStatement → FunctionDeclaration FunctionBody
        Node FunctionStatement()
        {
            Node functionStatement = new Node("FunctionStatement");
            functionStatement.Children.Add(FunctionDeclaration());
            functionStatement.Children.Add(FunctionBody());
            return functionStatement;
        }

        // FunctionDeclaration → Datatype FunctionName ( ParameterList )
        Node FunctionDeclaration()
        {
            Node functionDecl = new Node("FunctionDeclaration");
            functionDecl.Children.Add(Datatype());
            functionDecl.Children.Add(FunctionName());
            functionDecl.Children.Add(match(Token_Class.LParenthesis));
            functionDecl.Children.Add(ParameterList());
            functionDecl.Children.Add(match(Token_Class.RParenthesis));
            return functionDecl;
        }

        // ParameterList → Parameter ParameterList' | ε
        Node ParameterList()
        {
            Node paramList = new Node("ParameterList");
            Token_Class t = Peek();

            if (t == Token_Class.Int || t == Token_Class.Float || t == Token_Class.StringKw)
            {
                paramList.Children.Add(Parameter());
                paramList.Children.Add(ParameterListdash());
            }
            // else epsilon
            return paramList;
        }

        // ParameterList' → , Parameter ParameterList' | ε
        Node ParameterListdash()
        {
            Node paramListdash = new Node("ParameterList'");

            if (IsToken(Token_Class.Comma))
            {
                paramListdash.Children.Add(match(Token_Class.Comma));
                paramListdash.Children.Add(Parameter());
                paramListdash.Children.Add(ParameterListdash());
            }
            // else epsilon
            return paramListdash;
        }

        // Parameter → Datatype Identifier
        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());
            parameter.Children.Add(match(Token_Class.Identifier));
            return parameter;
        }

        // FunctionBody → { StatementsList ReturnStatement }
        Node FunctionBody()
        {
            Node functionBody = new Node("FunctionBody");
            functionBody.Children.Add(match(Token_Class.LBrace));
            functionBody.Children.Add(Statementslist());
            functionBody.Children.Add(ReturnStatement());
            functionBody.Children.Add(match(Token_Class.RBrace));
            return functionBody;
        }

        // ReturnStatement → return Expression ;
        Node ReturnStatement()
        {
            Node returnStatement = new Node("ReturnStatement");
            returnStatement.Children.Add(match(Token_Class.Return));
            returnStatement.Children.Add(Expression());
            returnStatement.Children.Add(match(Token_Class.Semicolon));
            return returnStatement;
        }

        // Statementslist → Statement Statementslist | ε
        Node Statementslist()
        {
            Node statementslist = new Node("Statements");
            Token_Class t = Peek();

            if (t == Token_Class.Int || t == Token_Class.Float || t == Token_Class.StringKw ||
                t == Token_Class.Identifier || t == Token_Class.Write || t == Token_Class.Read ||
                t == Token_Class.If || t == Token_Class.Repeat )
            {
                statementslist.Children.Add(Statement());
                statementslist.Children.Add(Statementslist());
            }
            // else epsilon
            return statementslist;
        }

        // Statement → DeclarationStatement | AssignmentStatement | WriteStatement |
        //             ReadStatement | IfStatement | RepeatStatement
        Node Statement()
        {
            Node statement = new Node("Statement");
            Token_Class t = Peek();

            if (t == Token_Class.Int || t == Token_Class.Float || t == Token_Class.StringKw)
            {
                statement.Children.Add(DeclarationStatement());
            }
            else if (t == Token_Class.Identifier)
            {
                statement.Children.Add(AssignmentStatement());
            }
            else if (t == Token_Class.Write)
            {
                statement.Children.Add(WriteStatement());
            }
            else if (t == Token_Class.Read)
            {
                statement.Children.Add(ReadStatement());
            }
            else if (t == Token_Class.If)
            {
                statement.Children.Add(IfStatement());
            }
            else if (t == Token_Class.Repeat)
            {
                statement.Children.Add(RepeatStatement());
            }
           

            else
            {
                Errors.Error_List.Add("Parsing Error: Unexpected token in Statement: " + t);
                // attempt to recover by consuming the token
                if (InputPointer < TokenStream.Count) InputPointer++;
            }
            return statement;
        }

        // AssignmentStatement → Identifier := Expression ;
        Node AssignmentStatement()
        {
            Node assignStatement = new Node("AssignmentStatement");
            assignStatement.Children.Add(match(Token_Class.Identifier));
            assignStatement.Children.Add(match(Token_Class.Assign));
            assignStatement.Children.Add(Expression());
            assignStatement.Children.Add(match(Token_Class.Semicolon));
            return assignStatement;
        }

        // RepeatStatement → repeat Statements until ConditionStatement
        Node RepeatStatement()
        {
            Node repeatStatement = new Node("RepeatStatement");
            repeatStatement.Children.Add(match(Token_Class.Repeat));
            repeatStatement.Children.Add(Statementslist());
            repeatStatement.Children.Add(match(Token_Class.Until));
            repeatStatement.Children.Add(ConditionStatement());
            return repeatStatement;
        }

        // IfStatement → if ConditionStatement then Statementslist IfTail end
        Node IfStatement()
        {
            Node ifStatement = new Node("IfStatement");
            ifStatement.Children.Add(match(Token_Class.If));
            ifStatement.Children.Add(ConditionStatement());
            ifStatement.Children.Add(match(Token_Class.Then));
            ifStatement.Children.Add(Statementslist());
            ifStatement.Children.Add(IfTail());
            ifStatement.Children.Add(match(Token_Class.End));
            return ifStatement;
        }

        // IfTail → ElseIfStatement | ElseStatement  | ε
        Node IfTail()
        {
            Node ifTail = new Node("IfTail");
            Token_Class t = Peek();

            if (t == Token_Class.ElseIf)
            {
                ifTail.Children.Add(ElseIfStatement());
            }
            else if (t == Token_Class.Else)
            {
                ifTail.Children.Add(ElseStatement());
            }
           

            return ifTail;
        }

        // ElseIfStatement → elseif ConditionStatement then Statementslist IfTail
        Node ElseIfStatement()
        {
            Node elseIfStatement = new Node("ElseIfStatement");
            elseIfStatement.Children.Add(match(Token_Class.ElseIf));
            elseIfStatement.Children.Add(ConditionStatement());
            elseIfStatement.Children.Add(match(Token_Class.Then));
            elseIfStatement.Children.Add(Statementslist());
            elseIfStatement.Children.Add(IfTail());
            return elseIfStatement;
        }

        // ElseStatement → else Statementslist
        Node ElseStatement()
        {
            Node elseStatement = new Node("ElseStatement");
            elseStatement.Children.Add(match(Token_Class.Else));
            elseStatement.Children.Add(Statementslist());
           
            return elseStatement;
        }

        // ConditionStatement → Condition ConditionStatement'
        Node ConditionStatement()
        {
            Node conditionStatement = new Node("ConditionStatement");
            conditionStatement.Children.Add(Condition());
            conditionStatement.Children.Add(ConditionStatementPrime());
            return conditionStatement;
        }

        // ConditionStatement' → BooleanOperator Condition ConditionStatement' | ε
        Node ConditionStatementPrime()
        {
            Node conditionStatementPrime = new Node("ConditionStatement'");

            if (IsToken(Token_Class.AndOp) || IsToken(Token_Class.OrOp))
            {
                conditionStatementPrime.Children.Add(BooleanOperator());
                conditionStatementPrime.Children.Add(Condition());
                conditionStatementPrime.Children.Add(ConditionStatementPrime());
            }
            // else epsilon
            return conditionStatementPrime;
        }

        // Condition → Identifier ConditionOperator Term
        Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(Token_Class.Identifier));
            condition.Children.Add(ConditionOperator());
            condition.Children.Add(Term());
            return condition;
        }

        // ConditionOperator → < | > | = | <>
        Node ConditionOperator()
        {
            Node conditionOperator = new Node("ConditionOperator");

            if (IsToken(Token_Class.LessThanOp))
                conditionOperator.Children.Add(match(Token_Class.LessThanOp));
            else if (IsToken(Token_Class.GreaterThanOp))
                conditionOperator.Children.Add(match(Token_Class.GreaterThanOp));
            else if (IsToken(Token_Class.EqualOp))
                conditionOperator.Children.Add(match(Token_Class.EqualOp));
            else if (IsToken(Token_Class.NotEqualOp))
                conditionOperator.Children.Add(match(Token_Class.NotEqualOp));
            else
                Errors.Error_List.Add("Parsing Error: Expected condition operator (<, >, =, <>)");

            return conditionOperator;
        }

        // BooleanOperator → && | ||
        Node BooleanOperator()
        {
            Node booleanOperator = new Node("BooleanOperator");

            if (IsToken(Token_Class.AndOp))
                booleanOperator.Children.Add(match(Token_Class.AndOp));
            else if (IsToken(Token_Class.OrOp))
                booleanOperator.Children.Add(match(Token_Class.OrOp));
            else
                Errors.Error_List.Add("Parsing Error: Expected boolean operator (&&, ||)");

            return booleanOperator;
        }

        // ReadStatement → read Identifier ;
        Node ReadStatement()
        {
            Node readStatement = new Node("ReadStatement");
            readStatement.Children.Add(match(Token_Class.Read));
            readStatement.Children.Add(match(Token_Class.Identifier));
            readStatement.Children.Add(match(Token_Class.Semicolon));
            return readStatement;
        }

        Node DeclarationStatement()
        {
            Node declarationStatement = new Node("DeclarationStatement");
            declarationStatement.Children.Add(Datatype());
            declarationStatement.Children.Add(DeclList());
            declarationStatement.Children.Add(match(Token_Class.Semicolon));
            return declarationStatement;
        }

        Node DeclList()
        {
            Node declList = new Node("DeclList");
            declList.Children.Add(DeclItem());
            declList.Children.Add(DeclListPrime());
            return declList;
        }

        Node DeclListPrime()
        {
            Node declListPrime = new Node("DeclList'");

            if (IsToken(Token_Class.Comma))
            {
                declListPrime.Children.Add(match(Token_Class.Comma));
                declListPrime.Children.Add(DeclItem());
                declListPrime.Children.Add(DeclListPrime());
            }
            // else epsilon
            return declListPrime;
        }

        Node DeclItem()
        {
            Node declItem = new Node("DeclItem");
            declItem.Children.Add(match(Token_Class.Identifier));
            declItem.Children.Add(DeclItemPrime());
            return declItem;
        }

        Node DeclItemPrime()
        {
            Node declItemPrime = new Node("DeclItem'");
            // If we see := then parse assignment
            if (IsToken(Token_Class.Assign))
            {
                declItemPrime.Children.Add(match(Token_Class.Assign));
                declItemPrime.Children.Add(Expression());
            }
            // else epsilon (variable declared without initialization)
            return declItemPrime;
        }

        // WriteStatement → write WriteArg ;
        Node WriteStatement()
        {
            Node writeStatement = new Node("WriteStatement");
            writeStatement.Children.Add(match(Token_Class.Write));
            writeStatement.Children.Add(WriteArg());
            writeStatement.Children.Add(match(Token_Class.Semicolon));
            return writeStatement;
        }

        // WriteArg → Expression | endl
        Node WriteArg()
        {
            Node writeArg = new Node("WriteArg");

            if (IsToken(Token_Class.Endl))
            {
                writeArg.Children.Add(match(Token_Class.Endl));
            }
            else
            {
                writeArg.Children.Add(Expression());
            }
            return writeArg;
        }

        // Expression → String | Equation 
        Node Expression()
        {
            Node expression = new Node("Expression");
            Token_Class t = Peek();

            if (t == Token_Class.StringLiteral)
            {
                expression.Children.Add(match(Token_Class.StringLiteral));
            }
            else if (t == Token_Class.Number || t == Token_Class.Identifier ||
                     t == Token_Class.LParenthesis)
            {
                // Parse as Equation (handles Term as well)
                expression.Children.Add(Equation());
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Invalid expression start: " + t);
            }
            return expression;
        }

        // Equation → EquationTerm Equation'
        Node Equation()
        {
            Node equation = new Node("Equation");
            equation.Children.Add(EquationTerm());
            equation.Children.Add(EquationPrime());
            return equation;
        }

        // Equation' → ArithmeticOperator EquationTerm Equation' | ε
        Node EquationPrime()
        {
            Node equationPrime = new Node("Equation'");

            if (IsToken(Token_Class.PlusOp) || IsToken(Token_Class.MinusOp) ||
                IsToken(Token_Class.MultiplyOp) || IsToken(Token_Class.DivideOp))
            {
                equationPrime.Children.Add(ArithmeticOperator());
                equationPrime.Children.Add(EquationTerm());
                equationPrime.Children.Add(EquationPrime());
            }
            // else epsilon
            return equationPrime;
        }

        // EquationTerm → ( Equation ) | Term
        Node EquationTerm()
        {
            Node equationTerm = new Node("EquationTerm");

            if (IsToken(Token_Class.LParenthesis))
            {
                equationTerm.Children.Add(match(Token_Class.LParenthesis));
                equationTerm.Children.Add(Equation());
                equationTerm.Children.Add(match(Token_Class.RParenthesis));
            }
            else
            {
                equationTerm.Children.Add(Term());
            }
            return equationTerm;
        }

        // Term → Number | Identifier | FunctionCall
        Node Term()
        {
            Node term = new Node("Term");
            Token_Class t = Peek();

            if (t == Token_Class.Number)
            {
                term.Children.Add(match(Token_Class.Number));
            }
            else if (t == Token_Class.Identifier)
            {
                // Look ahead to check if it's a function call
                if (InputPointer + 1 < TokenStream.Count &&
                    TokenStream[InputPointer + 1].token_type == Token_Class.LParenthesis)
                {
                    term.Children.Add(FunctionCall());
                }
                else
                {
                    term.Children.Add(match(Token_Class.Identifier));
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected Number or Identifier in Term");
                // attempt to recover
                if (InputPointer < TokenStream.Count) InputPointer++;
            }
            return term;
        }

        // FunctionCall → Identifier ( ArgList )
        Node FunctionCall()
        {
            Node funcCall = new Node("FunctionCall");
            funcCall.Children.Add(match(Token_Class.Identifier));
            funcCall.Children.Add(match(Token_Class.LParenthesis));
            funcCall.Children.Add(ArgList());
            funcCall.Children.Add(match(Token_Class.RParenthesis));
            return funcCall;
        }

        // ArgList → Expression ArgListdash | ε
        Node ArgList()
        {
            Node argList = new Node("ArgList");
            Token_Class t = Peek();

            if (t == Token_Class.StringLiteral || t == Token_Class.Number ||
                t == Token_Class.Identifier || t == Token_Class.LParenthesis)
            {
                argList.Children.Add(Expression());
                argList.Children.Add(ArgListdash());
            }
            // else epsilon
            return argList;
        }

        // ArgListdash → , Expression ArgListdash | ε
        Node ArgListdash()
        {
            Node argListTail = new Node("ArgListdash");
            if (IsToken(Token_Class.Comma))
            {
                argListTail.Children.Add(match(Token_Class.Comma));
                argListTail.Children.Add(Expression());
                argListTail.Children.Add(ArgListdash());
            }
            // else epsilon
            return argListTail;
        }

        // ArithmeticOperator → + | - | * | /
        Node ArithmeticOperator()
        {
            Node arithmeticOperator = new Node("ArithmeticOperator");

            if (IsToken(Token_Class.PlusOp))
                arithmeticOperator.Children.Add(match(Token_Class.PlusOp));
            else if (IsToken(Token_Class.MinusOp))
                arithmeticOperator.Children.Add(match(Token_Class.MinusOp));
            else if (IsToken(Token_Class.MultiplyOp))
                arithmeticOperator.Children.Add(match(Token_Class.MultiplyOp));
            else if (IsToken(Token_Class.DivideOp))
                arithmeticOperator.Children.Add(match(Token_Class.DivideOp));
            else
                Errors.Error_List.Add("Parsing Error: Expected arithmetic operator (+, -, *, /)");

            return arithmeticOperator;
        }

        // FunctionName → Identifier
        Node FunctionName()
        {
            Node functionName = new Node("FunctionName");
            functionName.Children.Add(match(Token_Class.Identifier));
            return functionName;
        }

        // Datatype → int | float | string
        Node Datatype()
        {
            Node datatype = new Node("Datatype");

            if (IsToken(Token_Class.Int))
                datatype.Children.Add(match(Token_Class.Int));
            else if (IsToken(Token_Class.Float))
                datatype.Children.Add(match(Token_Class.Float));
            else if (IsToken(Token_Class.StringKw))
                datatype.Children.Add(match(Token_Class.StringKw));
            else
                Errors.Error_List.Add("Parsing Error: Expected datatype (int, float, string)");

            return datatype;
        }

        // ------------ match helper ------------
        public Node match(Token_Class ExpectedToken)
        {
            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                   
                    string lexeme = TokenStream[InputPointer].lex;
                    Node newNode = new Node(ExpectedToken.ToString(), lexeme);
                    InputPointer++;
                    return newNode;
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " but found " +
                        TokenStream[InputPointer].token_type.ToString());
                    // Attempt single-token recovery by consuming the unexpected token
                    //InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected " + ExpectedToken.ToString() + " but found end of input");
                return null;
            }
        }

       
        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }

        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            // Display both name and lexeme if lexeme exists
            string displayName = root.Name;
            if (!string.IsNullOrEmpty(root.Lexeme))
            {
                displayName += " (" + root.Lexeme + ")";
            }

            TreeNode tree = new TreeNode(displayName);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
