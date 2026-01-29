// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    public sealed class ExcelFormulaTokenizer
    {
        private readonly FormulaParseOptions _options;

        public ExcelFormulaTokenizer(FormulaParseOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IReadOnlyList<FormulaToken> Tokenize(string formulaText)
        {
            if (formulaText == null)
            {
                throw new ArgumentNullException(nameof(formulaText));
            }

            var tokens = new List<FormulaToken>();
            var length = formulaText.Length;
            var index = 0;

            if (_options.AllowLeadingEquals && length > 0 && formulaText[0] == '=')
            {
                index++;
            }

            while (index < length)
            {
                var ch = formulaText[index];
                if (char.IsWhiteSpace(ch))
                {
                    var whitespaceStart = index;
                    while (index < length && char.IsWhiteSpace(formulaText[index]))
                    {
                        index++;
                    }

                    if (ShouldInsertIntersection(tokens, formulaText, index))
                    {
                        tokens.Add(new FormulaToken(
                            FormulaTokenType.Intersection,
                            " ",
                            whitespaceStart,
                            index - whitespaceStart));
                    }
                    continue;
                }

                var start = index;

                if (ch == _options.ArgumentSeparator && ch != ',' && ch != ';' && ch != _options.DecimalSeparator)
                {
                    tokens.Add(new FormulaToken(FormulaTokenType.Comma, ch.ToString(), start, 1));
                    index++;
                    continue;
                }

                if (ch == '"')
                {
                    index++;
                    var builder = new StringBuilder();
                    while (index < length)
                    {
                        var current = formulaText[index];
                        if (current == '"')
                        {
                            if (index + 1 < length && formulaText[index + 1] == '"')
                            {
                                builder.Append('"');
                                index += 2;
                                continue;
                            }
                            index++;
                            break;
                        }
                        builder.Append(current);
                        index++;
                    }
                    tokens.Add(new FormulaToken(FormulaTokenType.Text, builder.ToString(), start, index - start));
                    continue;
                }

                if (ch == '\'')
                {
                    index++;
                    var builder = new StringBuilder();
                    while (index < length)
                    {
                        var current = formulaText[index];
                        if (current == '\'')
                        {
                            if (index + 1 < length && formulaText[index + 1] == '\'')
                            {
                                builder.Append('\'');
                                index += 2;
                                continue;
                            }
                            index++;
                            break;
                        }
                        builder.Append(current);
                        index++;
                    }
                    tokens.Add(new FormulaToken(FormulaTokenType.Name, builder.ToString(), start, index - start));
                    continue;
                }

                if (ch == '#')
                {
                    index++;
                    while (index < length)
                    {
                        var current = formulaText[index];
                        if (!char.IsLetterOrDigit(current) && current != '/' && current != '!' && current != '?' && current != '_')
                        {
                            break;
                        }
                        index++;
                    }
                    tokens.Add(new FormulaToken(FormulaTokenType.Error, formulaText.Substring(start, index - start), start, index - start));
                    continue;
                }

                if (IsNumberStart(formulaText, index, _options.DecimalSeparator))
                {
                    index = ReadNumber(formulaText, index, _options.DecimalSeparator);
                    tokens.Add(new FormulaToken(FormulaTokenType.Number, formulaText.Substring(start, index - start), start, index - start));
                    continue;
                }

                if (ch == '[')
                {
                    index = ReadBracketedReference(formulaText, index, start);
                    tokens.Add(new FormulaToken(FormulaTokenType.Name, formulaText.Substring(start, index - start), start, index - start));
                    continue;
                }

                if (IsNameStart(ch))
                {
                    index++;
                    while (index < length)
                    {
                        var current = formulaText[index];
                        if (IsNamePart(current))
                        {
                            index++;
                            continue;
                        }

                        if (current == '[')
                        {
                            index = ReadBracketedReference(formulaText, index, start);
                            continue;
                        }

                        break;
                    }
                    var text = formulaText.Substring(start, index - start);
                    if (IsBooleanLiteral(text))
                    {
                        tokens.Add(new FormulaToken(FormulaTokenType.Boolean, text, start, index - start));
                    }
                    else
                    {
                        tokens.Add(new FormulaToken(FormulaTokenType.Name, text, start, index - start));
                    }
                    continue;
                }

                switch (ch)
                {
                    case ',':
                        tokens.Add(new FormulaToken(FormulaTokenType.Comma, ",", start, 1));
                        index++;
                        break;
                    case ';':
                        tokens.Add(new FormulaToken(FormulaTokenType.Semicolon, ";", start, 1));
                        index++;
                        break;
                    case ':':
                        tokens.Add(new FormulaToken(FormulaTokenType.Colon, ":", start, 1));
                        index++;
                        break;
                    case '(':
                        tokens.Add(new FormulaToken(FormulaTokenType.OpenParen, "(", start, 1));
                        index++;
                        break;
                    case ')':
                        tokens.Add(new FormulaToken(FormulaTokenType.CloseParen, ")", start, 1));
                        index++;
                        break;
                    case '{':
                        tokens.Add(new FormulaToken(FormulaTokenType.OpenBrace, "{", start, 1));
                        index++;
                        break;
                    case '}':
                        tokens.Add(new FormulaToken(FormulaTokenType.CloseBrace, "}", start, 1));
                        index++;
                        break;
                    case '!':
                        tokens.Add(new FormulaToken(FormulaTokenType.Exclamation, "!", start, 1));
                        index++;
                        break;
                    case '<':
                    case '>':
                    case '=':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '^':
                    case '%':
                    case '&':
                        index = ReadOperator(formulaText, index, tokens);
                        break;
                    default:
                        throw new FormulaParseException($"Unexpected character '{ch}'.", index);
                }
            }

            tokens.Add(new FormulaToken(FormulaTokenType.End, string.Empty, length, 0));
            return tokens;
        }

        private static bool IsNumberStart(string text, int index, char decimalSeparator)
        {
            var ch = text[index];
            if (char.IsDigit(ch))
            {
                return true;
            }

            if (ch == decimalSeparator && index + 1 < text.Length && char.IsDigit(text[index + 1]))
            {
                return true;
            }

            return false;
        }

        private static int ReadBracketedReference(string text, int index, int start)
        {
            var length = text.Length;
            var depth = 0;
            while (index < length)
            {
                var ch = text[index];
                if (ch == '[')
                {
                    depth++;
                }
                else if (ch == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        index++;
                        while (index < length && IsNamePart(text[index]))
                        {
                            index++;
                        }

                        return index;
                    }
                }
                index++;
            }

            throw new FormulaParseException("Unterminated structured reference.", start);
        }

        private static int ReadNumber(string text, int index, char decimalSeparator)
        {
            var length = text.Length;
            var hasDecimal = false;
            while (index < length)
            {
                var ch = text[index];
                if (char.IsDigit(ch))
                {
                    index++;
                    continue;
                }

                if (!hasDecimal && ch == decimalSeparator)
                {
                    hasDecimal = true;
                    index++;
                    continue;
                }

                if ((ch == 'E' || ch == 'e') && index + 1 < length)
                {
                    var next = text[index + 1];
                    if (next == '+' || next == '-' || char.IsDigit(next))
                    {
                        index += 2;
                        while (index < length && char.IsDigit(text[index]))
                        {
                            index++;
                        }
                        continue;
                    }
                }

                break;
            }

            return index;
        }

        private static bool IsNameStart(char ch)
        {
            return char.IsLetter(ch) || ch == '_' || ch == '.' || ch == '$';
        }

        private static bool IsNamePart(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' || ch == '$';
        }

        private static bool IsBooleanLiteral(string text)
        {
            return string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(text, "FALSE", StringComparison.OrdinalIgnoreCase);
        }

        private static int ReadOperator(string text, int index, List<FormulaToken> tokens)
        {
            var start = index;
            var ch = text[index];
            var length = text.Length;

            if ((ch == '<' || ch == '>') && index + 1 < length)
            {
                var next = text[index + 1];
                if (next == '=' || (ch == '<' && next == '>'))
                {
                    tokens.Add(new FormulaToken(FormulaTokenType.Operator, text.Substring(index, 2), start, 2));
                    return index + 2;
                }
            }

            tokens.Add(new FormulaToken(FormulaTokenType.Operator, ch.ToString(), start, 1));
            return index + 1;
        }

        private static bool ShouldInsertIntersection(
            IReadOnlyList<FormulaToken> tokens,
            string text,
            int nextIndex)
        {
            if (tokens.Count == 0 || nextIndex >= text.Length)
            {
                return false;
            }

            var prev = tokens[tokens.Count - 1];
            if (prev.Type != FormulaTokenType.Name &&
                prev.Type != FormulaTokenType.CloseParen)
            {
                return false;
            }

            var next = text[nextIndex];
            if (prev.Type == FormulaTokenType.Name && next == '(')
            {
                return false;
            }

            return IsNameStart(next) || next == '\'' || next == '$' || next == '(';
        }
    }
}
