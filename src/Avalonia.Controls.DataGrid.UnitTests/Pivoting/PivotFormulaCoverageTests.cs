using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotFormulaCoverageTests
{
    [Fact]
    public void FormulaEvaluator_HasFormulas_And_FieldLookup_Coverage()
    {
        var evaluatorType = Nested("PivotFormulaEvaluator");

        var withFormula = new List<PivotValueField>
        {
            new() { Header = "A", Formula = "1+1" }
        };
        var evaluator = Activator.CreateInstance(evaluatorType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { withFormula },
            null);

        var hasFormulas = (bool)evaluatorType.GetProperty("HasFormulas")!.GetValue(evaluator)!;
        Assert.True(hasFormulas);

        var noFormula = new List<PivotValueField>
        {
            new() { Header = "B" },
            null!
        };
        var evaluatorNoFormula = Activator.CreateInstance(evaluatorType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { noFormula },
            null);

        var hasFormulasFalse = (bool)evaluatorType.GetProperty("HasFormulas")!.GetValue(evaluatorNoFormula)!;
        Assert.False(hasFormulasFalse);
    }

    [Fact]
    public void FormulaCellContext_Caches_And_ParentPaths()
    {
        var evaluatorType = Nested("PivotFormulaEvaluator");
        var contextType = Nested("PivotFormulaCellContext");

        var fields = new List<PivotValueField>
        {
            new() { Header = "Amount" }
        };

        var evaluator = Activator.CreateInstance(evaluatorType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { fields },
            null);

        var cellStates = CreateCellStatesDictionary();
        var rowPath = new object?[] { "A" };
        var colPath = new object?[] { "B" };
        var context = evaluatorType.GetMethod("CreateContext")!.Invoke(evaluator,
            new object?[] { cellStates, rowPath, colPath, null, null });

        var resolveValue = contextType.GetMethod("ResolveValue")!;
        Assert.Null(resolveValue.Invoke(context, new object?[] { -1 }));
        Assert.Null(resolveValue.Invoke(context, new object?[] { 0 }));
        Assert.Null(resolveValue.Invoke(context, new object?[] { 0 }));

        var getParentPath = contextType.GetMethod("GetParentPath", BindingFlags.Static | BindingFlags.NonPublic)!;
        var emptyPath = Array.Empty<object?>();
        Assert.Same(emptyPath, getParentPath.Invoke(null, new object?[] { emptyPath, null }));

        var parentPath = (object?[])getParentPath.Invoke(null, new object?[] { new object?[] { "x", "y" }, null })!;
        Assert.Equal(1, parentPath.Length);
    }

    [Fact]
    public void FormulaUsage_Merge_And_Token_Evaluation_Errors()
    {
        var usageType = Nested("PivotFormulaUsage");
        var tokenType = Nested("PivotFormulaToken");
        var formulaType = Nested("PivotFormula");
        var contextType = Nested("PivotFormulaCellContext");
        var evaluatorType = Nested("PivotFormulaEvaluator");

        var usage = Activator.CreateInstance(usageType)!
            ;
        usageType.GetMethod("Merge")!.Invoke(usage, new object?[] { null });

        var tokens = Array.CreateInstance(tokenType, 0);
        var formula = Activator.CreateInstance(formulaType,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new object?[] { tokens, usage },
            null);

        var evaluator = Activator.CreateInstance(evaluatorType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { new List<PivotValueField>() },
            null);

        var context = evaluatorType.GetMethod("CreateContext")!.Invoke(evaluator,
            new object?[] { CreateCellStatesDictionary(), Array.Empty<object?>(), Array.Empty<object?>(), null, null });

        Assert.Null(formulaType.GetMethod("Evaluate")!.Invoke(formula, new[] { context }));

        var negate = tokenType.GetMethod("Negate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, Array.Empty<object?>());
        var add = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Add") });
        var subtract = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Subtract") });
        var multiply = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Multiply") });
        var divide = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Divide") });
        var none = tokenType.GetField("None", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(null);

        Assert.Null(EvaluateTokens(formulaType, usage, context, new[] { negate }));
        Assert.Null(EvaluateTokens(formulaType, usage, context, new[] { add }));
        Assert.Null(EvaluateTokens(formulaType, usage, context, new[] { subtract }));
        Assert.Null(EvaluateTokens(formulaType, usage, context, new[] { multiply }));
        Assert.Null(EvaluateTokens(formulaType, usage, context, new[] { divide }));
        Assert.Null(EvaluateTokens(formulaType, usage, context, new[] { none! }));
    }

    [Fact]
    public void FormulaParser_Covers_Error_Paths()
    {
        var parserType = Nested("PivotFormulaParser");
        var usageType = Nested("PivotFormulaUsage");
        var tokenType = Nested("PivotFormulaToken");

        var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Amount"] = 0
        };

        Assert.False(InvokeParser(parserType, usageType, lookup, " "));
        Assert.False(InvokeParser(parserType, usageType, lookup, "1+)"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "("));
        Assert.False(InvokeParser(parserType, usageType, lookup, "RowTotal("));
        Assert.False(InvokeParser(parserType, usageType, lookup, "RowTotal(Amount"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "UnknownFunc(Amount)"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "MissingField"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "[Missing"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "[Missing]"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "RowTotal(1)"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "@"));
        Assert.False(InvokeParser(parserType, usageType, lookup, "."));
        Assert.False(InvokeParser(parserType, usageType, lookup, "RowTotal([Amount"));
        Assert.True(InvokeParser(parserType, usageType, lookup, "RowTotal([Amount])"));
        Assert.True(InvokeParser(parserType, usageType, lookup, "1e12"));
        Assert.True(InvokeParser(parserType, usageType, lookup, "(1)"));
        Assert.True(InvokeParser(parserType, usageType, lookup, "1+2*3"));
        Assert.True(InvokeParser(parserType, usageType, lookup, "1*2+3"));

        _ = tokenType.GetMethod("LeftParen", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, Array.Empty<object?>());
        _ = tokenType.GetMethod("RightParen", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, Array.Empty<object?>());

        var shouldPop = parserType.GetMethod("ShouldPopOperator", BindingFlags.Static | BindingFlags.NonPublic)!;
        var opAdd = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Add") });
        var opMultiply = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Multiply") });
        Assert.True((bool)shouldPop.Invoke(null, new[] { opAdd, opMultiply })!);
        var opSubtract = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Subtract") });
        Assert.True((bool)shouldPop.Invoke(null, new[] { opAdd, opSubtract })!);

        var getPrecedence = parserType.GetMethod("GetPrecedence", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.Equal(3, getPrecedence.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Negate") }));
        Assert.Equal(2, getPrecedence.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Multiply") }));
        Assert.Equal(2, getPrecedence.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Divide") }));
        Assert.Equal(1, getPrecedence.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Add") }));
        Assert.Equal(1, getPrecedence.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Subtract") }));
        Assert.Equal(0, getPrecedence.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "None") }));

        var isLeftAssoc = parserType.GetMethod("IsLeftAssociative", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.False((bool)isLeftAssoc.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Negate") })!);
        Assert.True((bool)isLeftAssoc.Invoke(null, new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Add") })!);

        var scannerType = Nested("PivotFormulaScanner");
        var createToken = scannerType.GetMethod("TryCreateFunctionToken", BindingFlags.Static | BindingFlags.NonPublic)!;
        var usage = Activator.CreateInstance(usageType)!;
        var args = new object?[] { "RowTotal", -1, usage, null };
        Assert.False((bool)createToken.Invoke(null, args)!);
    }

    [Fact]
    public void FormulaParser_HandleToken_Default_Returns_False()
    {
        var parserType = Nested("PivotFormulaParser");
        var tokenType = Nested("PivotFormulaToken");
        var tokenKindType = Nested("PivotFormulaTokenKind");

        var invalidKind = Enum.ToObject(tokenKindType, 999);
        var token = Activator.CreateInstance(tokenType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { invalidKind, 0d, -1 },
            null)!;

        var listType = typeof(List<>).MakeGenericType(tokenType);
        var stackType = typeof(Stack<>).MakeGenericType(tokenType);
        var output = Activator.CreateInstance(listType)!;
        var operators = Activator.CreateInstance(stackType)!;

        var args = new object?[] { token, output, operators, true };
        var handleToken = parserType.GetMethod("HandleToken", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.False((bool)handleToken.Invoke(null, args)!);
    }

    [Fact]
    public void Formula_TryParse_Whitespace_And_Subtract_Evaluate()
    {
        var formulaType = Nested("PivotFormula");
        var evaluatorType = Nested("PivotFormulaEvaluator");
        var tokenType = Nested("PivotFormulaToken");
        var usageType = Nested("PivotFormulaUsage");

        var tryParse = formulaType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        var lookup = new Dictionary<string, int>();
        var args = new object?[] { " ", lookup, null };
        Assert.False((bool)tryParse.Invoke(null, args)!);

        var evaluator = Activator.CreateInstance(evaluatorType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { new List<PivotValueField>() },
            null);
        var context = evaluatorType.GetMethod("CreateContext")!.Invoke(evaluator,
            new object?[] { CreateCellStatesDictionary(), Array.Empty<object?>(), Array.Empty<object?>(), null, null });

        var usage = Activator.CreateInstance(usageType)!;
        var constant = tokenType.GetMethod("Constant", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        var subtract = tokenType.GetMethod("Operator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null,
            new object?[] { Enum.Parse(Nested("PivotFormulaTokenKind"), "Subtract") });

        var tokens = new object?[]
        {
            constant.Invoke(null, new object?[] { 3d }),
            constant.Invoke(null, new object?[] { 1d }),
            subtract
        };

        var result = EvaluateTokens(formulaType, usage, context!, tokens);
        Assert.Equal(2d, result);
    }

    private static object? EvaluateTokens(Type formulaType, object usage, object context, object?[] tokens)
    {
        var tokenType = Nested("PivotFormulaToken");
        var array = Array.CreateInstance(tokenType, tokens.Length);
        for (var i = 0; i < tokens.Length; i++)
        {
            array.SetValue(tokens[i], i);
        }

        var formula = Activator.CreateInstance(formulaType,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new object?[] { array, usage },
            null);

        return formulaType.GetMethod("Evaluate")!.Invoke(formula, new[] { context });
    }

    private static bool InvokeParser(Type parserType, Type usageType, Dictionary<string, int> lookup, string formula)
    {
        var usage = Activator.CreateInstance(usageType)!;
        var tokenType = Nested("PivotFormulaToken");
        var tokens = Array.CreateInstance(tokenType, 0);
        var args = new object?[] { formula, lookup, usage, tokens };
        var method = parserType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        var result = (bool)method.Invoke(null, args)!;
        return result;
    }

    private static Type Nested(string name)
    {
        return typeof(PivotTableBuilder).GetNestedType(name, BindingFlags.NonPublic)!;
    }

    private static object CreateCellStatesDictionary()
    {
        var cellKeyType = Nested("PivotCellKey");
        var cellStateType = Nested("PivotCellState");
        var dictType = typeof(Dictionary<,>).MakeGenericType(cellKeyType, cellStateType);
        return Activator.CreateInstance(dictType)!;
    }
}
