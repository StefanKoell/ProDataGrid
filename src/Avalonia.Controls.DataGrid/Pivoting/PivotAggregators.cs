// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IPivotAggregator
    {
        PivotAggregateType AggregateType { get; }

        string Name { get; }

        IPivotAggregationState CreateState();
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IPivotAggregationState
    {
        void Add(object? value);

        void Merge(IPivotAggregationState other);

        object? GetResult();
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotAggregatorRegistry
    {
        private readonly Dictionary<PivotAggregateType, IPivotAggregator> _aggregators = new();

        public PivotAggregatorRegistry()
        {
            Register(new SumAggregator());
            Register(new CountAggregator());
            Register(new AverageAggregator());
            Register(new MinAggregator());
            Register(new MaxAggregator());
            Register(new ProductAggregator());
            Register(new CountNumbersAggregator());
            Register(new CountDistinctAggregator());
            Register(new StdDevAggregator());
            Register(new StdDevPAggregator());
            Register(new VarianceAggregator());
            Register(new VariancePAggregator());
            Register(new FirstAggregator());
            Register(new LastAggregator());
        }

        public void Register(IPivotAggregator aggregator)
        {
            if (aggregator == null)
            {
                throw new ArgumentNullException(nameof(aggregator));
            }

            _aggregators[aggregator.AggregateType] = aggregator;
        }

        public IPivotAggregator? Get(PivotAggregateType aggregateType)
        {
            _aggregators.TryGetValue(aggregateType, out var aggregator);
            return aggregator;
        }
    }

    internal static class PivotNumeric
    {
        public static bool TryGetDouble(object? value, out double result)
        {
            result = 0d;
            if (value == null)
            {
                return false;
            }

            try
            {
                if (value is IConvertible)
                {
                    result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }

    internal abstract class PivotAggregatorBase : IPivotAggregator
    {
        public abstract PivotAggregateType AggregateType { get; }

        public abstract string Name { get; }

        public abstract IPivotAggregationState CreateState();
    }

    internal sealed class SumAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Sum;

        public override string Name => "Sum";

        public override IPivotAggregationState CreateState() => new SumState();

        private sealed class SumState : IPivotAggregationState
        {
            private double _sum;
            private int _count;

            public void Add(object? value)
            {
                if (PivotNumeric.TryGetDouble(value, out var number))
                {
                    _sum += number;
                    _count++;
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is SumState state)
                {
                    _sum += state._sum;
                    _count += state._count;
                }
            }

            public object? GetResult()
            {
                return _count == 0 ? null : _sum;
            }
        }
    }

    internal sealed class CountAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Count;

        public override string Name => "Count";

        public override IPivotAggregationState CreateState() => new CountState();

        private sealed class CountState : IPivotAggregationState
        {
            private int _count;

            public void Add(object? value)
            {
                if (value != null)
                {
                    _count++;
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is CountState state)
                {
                    _count += state._count;
                }
            }

            public object? GetResult() => _count;
        }
    }

    internal sealed class CountNumbersAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.CountNumbers;

        public override string Name => "Count Numbers";

        public override IPivotAggregationState CreateState() => new CountNumbersState();

        private sealed class CountNumbersState : IPivotAggregationState
        {
            private int _count;

            public void Add(object? value)
            {
                if (PivotNumeric.TryGetDouble(value, out _))
                {
                    _count++;
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is CountNumbersState state)
                {
                    _count += state._count;
                }
            }

            public object? GetResult() => _count;
        }
    }

    internal sealed class AverageAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Average;

        public override string Name => "Average";

        public override IPivotAggregationState CreateState() => new AverageState();

        private sealed class AverageState : IPivotAggregationState
        {
            private double _sum;
            private int _count;

            public void Add(object? value)
            {
                if (PivotNumeric.TryGetDouble(value, out var number))
                {
                    _sum += number;
                    _count++;
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is AverageState state)
                {
                    _sum += state._sum;
                    _count += state._count;
                }
            }

            public object? GetResult()
            {
                return _count == 0 ? null : _sum / _count;
            }
        }
    }

    internal sealed class ProductAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Product;

        public override string Name => "Product";

        public override IPivotAggregationState CreateState() => new ProductState();

        private sealed class ProductState : IPivotAggregationState
        {
            private double _product = 1d;
            private int _count;

            public void Add(object? value)
            {
                if (PivotNumeric.TryGetDouble(value, out var number))
                {
                    _product *= number;
                    _count++;
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is ProductState state)
                {
                    if (state._count > 0)
                    {
                        _product *= state._product;
                        _count += state._count;
                    }
                }
            }

            public object? GetResult()
            {
                return _count == 0 ? null : _product;
            }
        }
    }

    internal sealed class MinAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Min;

        public override string Name => "Min";

        public override IPivotAggregationState CreateState() => new MinState();

        private sealed class MinState : IPivotAggregationState
        {
            private object? _min;
            private IComparable? _minComparable;

            public void Add(object? value)
            {
                if (value is IComparable comparable)
                {
                    if (_minComparable == null || comparable.CompareTo(_minComparable) < 0)
                    {
                        _minComparable = comparable;
                        _min = value;
                    }
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is MinState state && state._minComparable != null)
                {
                    Add(state._minComparable);
                }
            }

            public object? GetResult() => _min;
        }
    }

    internal sealed class MaxAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Max;

        public override string Name => "Max";

        public override IPivotAggregationState CreateState() => new MaxState();

        private sealed class MaxState : IPivotAggregationState
        {
            private object? _max;
            private IComparable? _maxComparable;

            public void Add(object? value)
            {
                if (value is IComparable comparable)
                {
                    if (_maxComparable == null || comparable.CompareTo(_maxComparable) > 0)
                    {
                        _maxComparable = comparable;
                        _max = value;
                    }
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is MaxState state && state._maxComparable != null)
                {
                    Add(state._maxComparable);
                }
            }

            public object? GetResult() => _max;
        }
    }

    internal sealed class CountDistinctAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.CountDistinct;

        public override string Name => "Distinct Count";

        public override IPivotAggregationState CreateState() => new CountDistinctState();

        private sealed class CountDistinctState : IPivotAggregationState
        {
            private readonly HashSet<object?> _values = new();

            public void Add(object? value)
            {
                if (value != null)
                {
                    _values.Add(value);
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is CountDistinctState state)
                {
                    foreach (var value in state._values)
                    {
                        _values.Add(value);
                    }
                }
            }

            public object? GetResult() => _values.Count;
        }
    }

    internal abstract class VarianceAggregatorBase : PivotAggregatorBase
    {
        internal sealed class VarianceState : IPivotAggregationState
        {
            private readonly bool _population;
            private int _count;
            private double _mean;
            private double _m2;

            public VarianceState(bool population)
            {
                _population = population;
            }

            public void Add(object? value)
            {
                if (!PivotNumeric.TryGetDouble(value, out var number))
                {
                    return;
                }

                _count++;
                var delta = number - _mean;
                _mean += delta / _count;
                var delta2 = number - _mean;
                _m2 += delta * delta2;
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is not VarianceState state || state._count == 0)
                {
                    return;
                }

                if (_count == 0)
                {
                    _count = state._count;
                    _mean = state._mean;
                    _m2 = state._m2;
                    return;
                }

                var totalCount = _count + state._count;
                var delta = state._mean - _mean;
                _m2 += state._m2 + delta * delta * _count * state._count / totalCount;
                _mean = (_mean * _count + state._mean * state._count) / totalCount;
                _count = totalCount;
            }

            public object? GetResult()
            {
                if (_count == 0)
                {
                    return null;
                }

                if (_population)
                {
                    return _m2 / _count;
                }

                return _count > 1 ? _m2 / (_count - 1) : null;
            }
        }
    }

    internal sealed class VarianceAggregator : VarianceAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Variance;

        public override string Name => "Variance";

        public override IPivotAggregationState CreateState() => new VarianceState(false);
    }

    internal sealed class VariancePAggregator : VarianceAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.VarianceP;

        public override string Name => "Variance (Population)";

        public override IPivotAggregationState CreateState() => new VarianceState(true);
    }

    internal abstract class StdDevAggregatorBase : PivotAggregatorBase
    {
        protected sealed class StdDevState : IPivotAggregationState
        {
            private readonly VarianceAggregatorBase.VarianceState _varianceState;

            public StdDevState(bool population)
            {
                _varianceState = new VarianceAggregatorBase.VarianceState(population);
            }

            public void Add(object? value)
            {
                _varianceState.Add(value);
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is StdDevState state)
                {
                    _varianceState.Merge(state._varianceState);
                }
            }

            public object? GetResult()
            {
                var variance = _varianceState.GetResult();
                if (variance is double varianceValue)
                {
                    return Math.Sqrt(varianceValue);
                }

                return null;
            }
        }
    }

    internal sealed class StdDevAggregator : StdDevAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.StdDev;

        public override string Name => "StdDev";

        public override IPivotAggregationState CreateState() => new StdDevState(false);
    }

    internal sealed class StdDevPAggregator : StdDevAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.StdDevP;

        public override string Name => "StdDev (Population)";

        public override IPivotAggregationState CreateState() => new StdDevState(true);
    }

    internal sealed class FirstAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.First;

        public override string Name => "First";

        public override IPivotAggregationState CreateState() => new FirstState();

        private sealed class FirstState : IPivotAggregationState
        {
            private object? _value;
            private bool _hasValue;

            public void Add(object? value)
            {
                if (_hasValue || value == null)
                {
                    return;
                }

                _value = value;
                _hasValue = true;
            }

            public void Merge(IPivotAggregationState other)
            {
                if (_hasValue)
                {
                    return;
                }

                if (other is FirstState state && state._hasValue)
                {
                    _value = state._value;
                    _hasValue = true;
                }
            }

            public object? GetResult() => _hasValue ? _value : null;
        }
    }

    internal sealed class LastAggregator : PivotAggregatorBase
    {
        public override PivotAggregateType AggregateType => PivotAggregateType.Last;

        public override string Name => "Last";

        public override IPivotAggregationState CreateState() => new LastState();

        private sealed class LastState : IPivotAggregationState
        {
            private object? _value;

            public void Add(object? value)
            {
                if (value != null)
                {
                    _value = value;
                }
            }

            public void Merge(IPivotAggregationState other)
            {
                if (other is LastState state && state._value != null)
                {
                    _value = state._value;
                }
            }

            public object? GetResult() => _value;
        }
    }
}
