using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public enum Token_Class
    {
        Int, Float, StringKw, Read, Write, Repeat, Until, If, ElseIf, Else, Then, Return, Endl, Main,
        Dot, Semicolon, Comma, LParenthesis, RParenthesis, LBrace, RBrace, End,
        Assign, EqualOp, NotEqualOp, LessThanOp, GreaterThanOp, AndOp, OrOp,
        PlusOp, MinusOp, MultiplyOp, DivideOp,
        Identifier, Number, StringLiteral, Comment,
        Unknown
    }

    public class Token
    {
        public string lex;
        public Token_Class token_type;
        public int line_no;

        public override string ToString()
        {
            return $"{line_no}: {lex} -> {token_type}";
        }
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        public List<Token> Tokensnocom = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("INT", Token_Class.Int);
            ReservedWords.Add("FLOAT", Token_Class.Float);
            ReservedWords.Add("STRING", Token_Class.StringKw);
            ReservedWords.Add("READ", Token_Class.Read);
            ReservedWords.Add("WRITE", Token_Class.Write);
            ReservedWords.Add("REPEAT", Token_Class.Repeat);
            ReservedWords.Add("UNTIL", Token_Class.Until);
            ReservedWords.Add("IF", Token_Class.If);
            ReservedWords.Add("ELSEIF", Token_Class.ElseIf);
            ReservedWords.Add("ELSE", Token_Class.Else);
            ReservedWords.Add("THEN", Token_Class.Then);
            ReservedWords.Add("RETURN", Token_Class.Return);
            ReservedWords.Add("ENDL", Token_Class.Endl);
            ReservedWords.Add("END", Token_Class.End);
            ReservedWords.Add("MAIN", Token_Class.Main);
            Operators.Add(":=", Token_Class.Assign);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add(".", Token_Class.Dot);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParenthesis);
            Operators.Add(")", Token_Class.RParenthesis);
            Operators.Add("{", Token_Class.LBrace);
            Operators.Add("}", Token_Class.RBrace);
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            int i = 0;
            int lineNo = 1;
            int n = SourceCode.Length;

            while (i < n)
            {
                char c = SourceCode[i];

                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n') lineNo++;
                    i++;
                    continue;
                }

                if (c == '/' && i + 1 < n && SourceCode[i + 1] == '*')
                {
                    int start = i;
                    i += 2;
                    while (i < n && !(SourceCode[i] == '*' && i + 1 < n && SourceCode[i + 1] == '/'))
                    {
                        if (SourceCode[i] == '\n') lineNo++;
                        i++;
                    }
                    bool closed = false;
                    if (i < n && SourceCode[i] == '*' && i + 1 < n && SourceCode[i + 1] == '/')
                    {
                        i += 2;
                        closed = true;
                    }

                    string lex = SourceCode.Substring(start, i - start);

                    if (closed)
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Comment, line_no = lineNo });

                    }
                    else
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });
                        Errors.Error_List.Add($"Unclosed comment at line {lineNo}");
                    }
                    continue;

                }

                if (c == '"')
                {
                    int start = i;
                    i++;
                    while (i < n && SourceCode[i] != '"' && SourceCode[i] != '\n')
                    {
                        if (i + 1 < n && SourceCode[i] == '\\')
                        {
                            i += 2;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    bool closed = false;
                    if (i < n && SourceCode[i] == '"')
                    {
                        i++;
                        closed = true;
                    }
                    if (i < n && SourceCode[i] == '\n') lineNo++;


                    string lex = SourceCode.Substring(start, i - start);

                    if (closed)
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.StringLiteral, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = Token_Class.StringLiteral, line_no = lineNo });

                    }
                    else
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });

                        Errors.Error_List.Add($"Unclosed string literal at line {lineNo}");
                    }
                    continue;

                }

                if (char.IsLetter(c))
                {
                    int start = i;
                    i++;
                    while (i < n && (char.IsLetterOrDigit(SourceCode[i])))
                        i++;
                    string lex = SourceCode.Substring(start, i - start);
                    Token_Class tc = Token_Class.Identifier;
                    if (ReservedWords.TryGetValue(lex.ToUpperInvariant(), out tc))
                    {
                        Tokens.Add(new Token { lex = lex, token_type = tc, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = tc, line_no = lineNo });

                    }
                    else if (isIdentifier(lex))
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Identifier, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = Token_Class.Identifier, line_no = lineNo });
                    }
                    else
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });
                        Errors.Error_List.Add($"Invalid identifier '{lex}' at line {lineNo}");
                    }
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int start = i;


                    while (i < n && !char.IsWhiteSpace(SourceCode[i]) && !(SourceCode[i] == ';'))
                        i++;


                    string lex = SourceCode.Substring(start, i - start);

                    if (isConstant(lex))
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Number, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = Token_Class.Number, line_no = lineNo });
                    }
                    else
                    {
                        Tokens.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = lex, token_type = Token_Class.Unknown, line_no = lineNo });
                        Errors.Error_List.Add($"Invalid number format '{lex}' at line {lineNo}");
                    }

                    continue;
                }


                if (i + 1 < n)
                {
                    string two = SourceCode.Substring(i, 2);
                    if (Operators.ContainsKey(two))
                    {
                        Tokens.Add(new Token { lex = two, token_type = Operators[two], line_no = lineNo });
                        Tokensnocom.Add(new Token { lex = two, token_type = Operators[two], line_no = lineNo });

                        i += 2;
                        continue;
                    }
                }

                string one = SourceCode.Substring(i, 1);
                if (Operators.ContainsKey(one))
                {
                    Tokens.Add(new Token { lex = one, token_type = Operators[one], line_no = lineNo });
                    Tokensnocom.Add(new Token { lex = one, token_type = Operators[one], line_no = lineNo });
                    i++;
                    continue;
                }

                Tokens.Add(new Token { lex = one, token_type = Token_Class.Unknown, line_no = lineNo });
                Tokensnocom.Add(new Token { lex = one, token_type = Token_Class.Unknown, line_no = lineNo });
                Errors.Error_List.Add($"Unrecognized symbol '{one}' at line {lineNo}");
                i++;
            }

            int size = Tokens.Count;


           // MessageBox.Show($"Size = {size}");
            for (int index = 0; i < size; index++)
            {
                if (Tokens[index].token_type == Token_Class.Comment)
                {
                    continue;
                }
                Tokensnocom.Add(Tokens[index]);

            }
            JASON_Compiler.TokenStream = Tokensnocom;

        }

        public bool isIdentifier(string lex)
        {
            if (string.IsNullOrEmpty(lex)) return false;
            if (!char.IsLetter(lex[0])) return false;
            if (!char.IsLower(lex[0]) && !char.IsUpper(lex[0]) && !(lex[0] == '_')) return false;
            for (int i = 1; i < lex.Length; i++)
                if (!char.IsLetterOrDigit(lex[i]) && lex[i] != '_') return false;
            return true;
        }

        public bool isConstant(string lex)
        {
            if (string.IsNullOrEmpty(lex)) return false;
            int i = 0, n = lex.Length;
            while (i < n && char.IsDigit(lex[i])) i++;
            if (i == n) return true;
            if (i < n && lex[i] == '.')
            {
                i++;
                if (i >= n) return false;
                while (i < n && char.IsDigit(lex[i])) i++;
                return i == n;
            }
            return false;
        }
    }

}