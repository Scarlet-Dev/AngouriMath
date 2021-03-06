
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


using System.Collections.Generic;
using System.Globalization;

namespace AngouriMath
{
    class RuleList : Dictionary<Pattern, Entity> { }
    internal static class Patterns
    {
        internal static readonly Pattern any1 = new Pattern(100, Entity.PatType.COMMON, tree => true);
        internal static readonly Pattern any2 = new Pattern(101, Entity.PatType.COMMON, tree => true);
        internal static readonly Pattern any3 = new Pattern(102, Entity.PatType.COMMON, tree => true);
        internal static readonly Pattern any4 = new Pattern(103, Entity.PatType.COMMON, tree => true);
        internal static readonly Pattern const1 = new Pattern(200, Entity.PatType.NUMBER, tree => tree.entType == Entity.EntType.NUMBER);
        internal static readonly Pattern const2 = new Pattern(201, Entity.PatType.NUMBER, tree => tree.entType == Entity.EntType.NUMBER);
        internal static readonly Pattern const3 = new Pattern(202, Entity.PatType.NUMBER, tree => tree.entType == Entity.EntType.NUMBER);
        internal static readonly Pattern var1 = new Pattern(300, Entity.PatType.VARIABLE, tree => tree.entType == Entity.EntType.VARIABLE);
        internal static readonly Pattern func1 = new Pattern(400, Entity.PatType.FUNCTION, tree => tree.entType == Entity.EntType.FUNCTION);
        internal static readonly Pattern func2 = new Pattern(401, Entity.PatType.FUNCTION, tree => tree.entType == Entity.EntType.FUNCTION);
        private static int InternNumber = 10000;
        internal static Pattern Num(decimal a) => new Pattern(++InternNumber, Entity.PatType.NUMBER, tree => tree.entType == Entity.EntType.NUMBER, a.ToString(CultureInfo.InvariantCulture));

        internal static readonly RuleList DivisionPreparingRules = new RuleList
        {
            { any1 * (Num(1) / any2), any1 / any2 },
            { (const1 * any1) / any2, const1 * (any1 / any2) },
            { (const1 / any1) * any2, const1 * (any2 / any1) },
        };

        internal static readonly RuleList TrigonometricRules = new RuleList
        {
            // sin({}) * cos({}) = 1/2 * sin(2{})
            { Sinf.PHang(any1) * Cosf.PHang(any1), Num(1.0m/2) * Sinf.PHang(Num(2) * any1) },
            { Cosf.PHang(any1) * Sinf.PHang(any1), Num(1.0m/2) * Sinf.PHang(Num(2) * any1) },

            // arc1({}) + arc2({}) = pi/2
            { Arcsinf.PHang(any1) + Arccosf.PHang(any1), MathS.pi / 2 },
            { Arccosf.PHang(any1) + Arcsinf.PHang(any1), MathS.pi / 2 },
            { Arctanf.PHang(any1) + Arccotanf.PHang(any1), MathS.pi / 2 },
            { Arccotanf.PHang(any1) + Arctanf.PHang(any1), MathS.pi / 2 },

            // arcfunc(func(x)) = x
            { Arcsinf.PHang(Sinf.PHang(any1)), any1 },
            { Arccosf.PHang(Cosf.PHang(any1)), any1 },
            { Arctanf.PHang(Tanf.PHang(any1)), any1 },
            { Arccotanf.PHang(Cotanf.PHang(any1)), any1 },

            // func(arcfunc(x)) = x
            { Sinf.PHang(Arcsinf.PHang(any1)), any1 },
            { Cosf.PHang(Arccosf.PHang(any1)), any1 },
            { Tanf.PHang(Arctanf.PHang(any1)), any1 },
            { Cotanf.PHang(Arccotanf.PHang(any1)), any1 },

            // sin(:)^2 + cos(:)^2 = 1
            { Powf.PHang(Sinf.PHang(any1), Num(2)) + Powf.PHang(Cosf.PHang(any1), Num(2)), 1 },
            { Powf.PHang(Cosf.PHang(any1), Num(2)) + Powf.PHang(Sinf.PHang(any1), Num(2)), 1 },

            { Powf.PHang(Sinf.PHang(any1), Num(2)) - Powf.PHang(Cosf.PHang(any1), Num(2)), -1 * (Powf.PHang(Cosf.PHang(any1), Num(2)) - Powf.PHang(Sinf.PHang(any1), Num(2))) },
            { Powf.PHang(Cosf.PHang(any1), Num(2)) - Powf.PHang(Sinf.PHang(any1), Num(2)), Cosf.PHang(2 * any1) },
        };

        internal static readonly RuleList ExpandTrigonometricRules = new RuleList {
            { Num(1.0m/2) * Sinf.PHang(Num(2) * any1), Sinf.PHang(any1) * Cosf.PHang(any1) },
            { Num(1.0m/2) * Sinf.PHang(Num(2) * any1), Cosf.PHang(any1) * Sinf.PHang(any1) },

            { Cosf.PHang(Num(2) * any1), Powf.PHang(Cosf.PHang(any1), Num(2)) - Powf.PHang(Sinf.PHang(any1), Num(2)) },
        };

        internal static readonly RuleList PowerRules = new RuleList
        {
            // x * {} ^ {} = {} ^ {} * x
            { var1 * Powf.PHang(any1, any2), Powf.PHang(any1, any2) * var1 },

            // {} ^ n * {}
            { Powf.PHang(any1, any2) * any1, Powf.PHang(any1, any2 + Num(1)) },
            { any1 * Powf.PHang(any1, any2), Powf.PHang(any1, any2 + Num(1)) },

            // {} ^ n * {} ^ m = {} ^ (n + m)
            { Powf.PHang(any1, any2) * Powf.PHang(any1, any3), Powf.PHang(any1, any2 + any3) },

            // {} ^ n / {} ^ m = {} ^ (n - m)
            { Powf.PHang(any1, any2) / Powf.PHang(any1, any3), Powf.PHang(any1, any2 - any3) },

            // ({} ^ {}) ^ {} = {} ^ ({} * {})
            { Powf.PHang(Powf.PHang(any1, any2), any3), Powf.PHang(any1, any2 * any3) },

            // {1} ^ n * {2} ^ n = ({1} * {2}) ^ n
            { Powf.PHang(any1, any3) * Powf.PHang(any2, any3), Powf.PHang(any1 * any2, any3) },
            { Powf.PHang(any1, any3) / Powf.PHang(any2, any3), Powf.PHang(any1 / any2, any3) },

            // x / x^n
            { any1 / Powf.PHang(any1, any2), Powf.PHang(any1, Num(1) - any2) },
            // x^n / x
            { Powf.PHang(any1, any2) / any1, Powf.PHang(any1, any2 - Num(1)) },
            // x^n / x^m

            // c ^ log(c, a) = a
            { Powf.PHang(const1, Logf.PHang(any1, const1)), any1 },

            { Powf.PHang(any1, any3) * (any1 * any2), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            { Powf.PHang(any1, any3) * (any2 * any1), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            { (any1 * any2) * Powf.PHang(any1, any3), (Powf.PHang(any1, any3 + Num(1))) * any2 },
            { (any2 * any1) * Powf.PHang(any1, any3), (Powf.PHang(any1, any3 + Num(1))) * any2 },

            // (a * x) ^ c = a^c * x^c
            { Powf.PHang(const1 * any1, const2), Powf.PHang(const1, const2) * Powf.PHang(any1, const2) },

            // {1} ^ (-1) = 1 / {1}
            { Powf.PHang(any1, Num(-1)), 1 / any1 },

            // (1 / {}) ^ 2 * {}
            { Powf.PHang(const1 / any1, const2) * any1, Powf.PHang(any1, Num(1) - const2) },
            { Powf.PHang(const1 / any1, const2) * Powf.PHang(any1, const3), Powf.PHang(any1, const3 - const2) },

            // {1} / {2} / {2}
            { any1 / any2 / any2, any1 / Powf.PHang(any2, Num(2)) },
            { any1 / Powf.PHang(any2, any3) / any2, any1 / Powf.PHang(any2, any3 + Num(1)) },
            { any1 / any2 / Powf.PHang(any2, any3), any1 / Powf.PHang(any2, any3 + Num(1)) },
            { any1 / Powf.PHang(any2, any4) / Powf.PHang(any2, any3), any1 / Powf.PHang(any2, any3 + any4) },
        };

        internal static readonly RuleList CommonRules = new RuleList {
            // (a * f(x)) * g(x) = a * (f(x) * g(x))
            { (const1 * func1) * func2, (func1 * func2) * const1 },

            // a / (b / c) = a * c / b
            { any1 / (any2 / any3), any1 * any3 / any2 },

            // a / (b / c) = a * c / b
            { any1 / any2 / any3, any1 / (any2 * any3) },

            // a * (b / c) = (a * b) / c
            { any1 * (any2 / any3), (any1 * any2) / any3 },

            // (a * f(x)) * b = (a * b) * f(x)
            { (const1 * func1) * const2, (const1 * const2) * func1 },

            // (a * f(x)) * (b * g(x)) = (a * b) * (f(x) * g(x))
            { (const1 * func1) * (const2 * func2), (func1 * func2) * (const1 * const2) },

            // (f(x) + {}) + g(x) = (f(x) + g(x)) + {}
            { (func1 + any1) + func2, (func1 + func2) + any1 },

            // g(x) + (f(x) + {}) = (f(x) + g(x)) + {}
            { func2 + (func1 + any1), (func1 + func2) + any1 },

            // x * a = a * x
            { var1 * const1, const1 * var1},

            // a + x = x + a
            { const1 + var1, var1 + const1},

            // f(x) * a = a * f(x)
            { func1 * const1, const1 * func1},

            // a + f(x) = f(x) + a
            { const1 + func1, func1 + const1},

            // a * x + b * x = (a + b) * x
            { const1 * var1 + const2 * var1, (const1 + const2) * var1 },

            // a * x - b * x = (a - b) * x
            { const1 * var1 - const2 * var1, (const1 - const2) * var1 },

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            { any1 * any2 + any1 * any3, any1 * (any2 + any3) },
            { any1 * any2 + any3 * any1, any1 * (any2 + any3) },
            { any2 * any1 + any3 * any1, any1 * (any2 + any3) },
            { any2 * any1 + any1 * any3, any1 * (any2 + any3) },
            { any1 + any1 * any2, any1 * (Num(1) + any2) },
            { any1 + any2 * any1, any1 * (Num(1) + any2) },
            { any1 * any2 + any1, any1 * (Num(1) + any2) },
            { any2 * any1 + any1, any1 * (Num(1) + any2) },
            { any1 + any1, Num(2) * any1 },

            { any1 * any2 - any1 * any3, any1 * (any2 - any3) },
            { any1 * any2 - any3 * any1, any1 * (any2 - any3) },
            { any2 * any1 - any3 * any1, any1 * (any2 - any3) },
            { any2 * any1 - any1 * any3, any1 * (any2 - any3) },
            { any1 - any1 * any2, any1 * (Num(1) - any2) },
            { any1 - any2 * any1, any1 * (Num(1) - any2) },
            { any1 * any2 - any1, any1 * (any2 - Num(1)) },
            { any2 * any1 - any1, any1 * (any2 - Num(1)) },

            // {1} / {2} + {1} * {3} = {1} * (1 / {2} + {3})
            { any1 / any2 + any1 * any3, any1 * (Num(1) / any2 + any3) },
            { any1 / any2 + any3 * any1, any1 * (Num(1) / any2 + any3) },
            { any2 * any1 + any1 / any3, any1 * (any2 + Num(1) / any3) },
            { any1 * any2 + any1 / any3, any1 * (any2 + Num(1) / any3) },
            { any1 + any1 / any2, any1 * (Num(1) + Num(1) / any2) },
            { any1 / any2 + any1, any1 * (Num(1) + Num(1) / any2) },

            // {1} * {2} - {1} * {3} = {1} * ({2} - {3})
            { any1 * any2 - any1 * any3, any1 * (any2 - any3) },

            // x * x = x ^ 2
            { any1 * any1, Powf.PHang(any1, Num(2)) },            

            // (a * x) * b
            { (const1 * var1) * const2, (const1 * const2) * var1 },
            { const2 * (const1 * var1), (const1 * const2) * var1 },
            { (const1 * func1) * const2, (const1 * const2) * func1 },
            { const2 * (const1 * func1), (const1 * const2) * func1 },

            // (x + a) + b
            { (var1 + const1) + const2, var1 + (const1 + const2) },

            // b + (x + a)
            { const2 + (var1 + const1), var1 + (const1 + const2) },

            // x * a * x
            { any1 * (any1 * any2), (any1 * any1) * any2 },
            { any1 * (any2 * any1), (any1 * any1) * any2 },
            { (any1 * any2) * any1, (any1 * any1) * any2 },
            { (any2 * any1) * any1, (any1 * any1) * any2 },
            
            
            // -1 * {1} + {2} = {2} - {1}
            { Num(-1) * any1 + any2, any2 - any1 },
            { any1 + Num(-1) * any2, any1 - any2 },

            // (x - {}) (x + {}) = x2 - {}2
            { (var1 - any1) * (var1 + any1), Powf.PHang(var1, Num(2)) - Powf.PHang(any1, Num(2)) },
            { (var1 + any1) * (var1 - any1), Powf.PHang(var1, Num(2)) - Powf.PHang(any1, Num(2)) },

            // a / a
            { any1 / any1, 1},

            // (a * c) / c
            { (any1 * any2) / any2, any1 },
            { (any2 * any1) / any2, any1 },
            { (any1 * any2) / (any2 * any3), any1 / any3 },
            { (any1 * any2) / (any3 * any2), any1 / any3 },
            { (any2 * any1) / (any2 * any3), any1 / any3 },
            { (any2 * any1) / (any3 * any2), any1 / any3 },

            // ({1} - {2}) / ({2} - {1})
            { (any1 - any2) / (any2 - any1), -1 },

            // ({1} + {2}) / ({2} + {1})
            { (any1 + any2) / (any2 + any1), 1 },

            // a / (b * {1})
            { const1 / (const2 * any1), (const1 / const2) / any1 },
            { const1 / (any1 * const2), (const1 / const2) / any1 },

            // a * (b * {}) = (a * b) * {}
            { const1 * (const2 * any1), (const1 * const2) * any1 },

            { any1 - any1, Num(0) },

            // {1} - {2} * {1}
            { any1 - any2 * any1, any1 * (Num(1) - any2) },
            { any1 - any1 * any2, any1 * (Num(1) - any2) },

            // a / {} * b
            { any1 / any2 * any3, any1 * any3 / any2},

            // a * {1} / b
            { (const1 * any1) / const2, any1 * (const1 / const2) },
        };

        internal static readonly RuleList ExpandRules = new RuleList
        {
            // (any1 + any2)2
            { Powf.PHang(any1, Num(1)), any1 },
            { Powf.PHang(any1, Num(2)), any1 * any1 },
            { Powf.PHang(any1, Num(3)), any1 * any1 * any1 },
            { Powf.PHang(any1, Num(4)), any1 * any1 * any1 * any1 },

            // ({1} - {2}) ({1} + {2}) = x2 - {}2
            { (any1 - any2) * (any1 + any2), Powf.PHang(any1, Num(2)) - Powf.PHang(any2, Num(2)) },
            { (any1 + any2) * (any1 - any2), Powf.PHang(any1, Num(2)) - Powf.PHang(any2, Num(2)) },

            // ({1} + {2}) * ({3} + {4}) = {1}{3} + {1}{4} + {2}{3} + {2}{4}
            { (any1 + any2) * (any3 + any4), any1 * any3 + any1 * any4 + any2 * any3 + any2 * any4 },
            { (any1 - any2) * (any3 + any4), any1 * any3 + any1 * any4 - any2 * any3 - any2 * any4 },
            { (any1 + any2) * (any3 - any4), any1 * any3 - any1 * any4 + any2 * any3 - any2 * any4 },
            { (any1 - any2) * (any3 - any4), any1 * any3 - any1 * any4 - any2 * any3 + any2 * any4 },
            
            // {1} * ({2} + {3})
            { any1 * (any2 + any3), any1 * any2 + any1 * any3 },
            { any1 * (any2 - any3), any1 * any2 - any1 * any3 },
            { (any2 + any3) * any1, any1 * any2 + any1 * any3 },
            { (any2 - any3) * any1, any1 * any2 - any1 * any3 },

            // ({1} +- {2}) / {3} == {1} / {3} +- {2} / {3}
            { (any1 + any2) / any3, any1 / any3 + any2 / any3 },
            { (any1 - any2) / any3, any1 / any3 - any2 / any3 },

            { Sinf.PHang(any1 + any2), Sinf.PHang(any1) * Cosf.PHang(any2) + Sinf.PHang(any2) * Cosf.PHang(any1) },
            { Sinf.PHang(any1 - any2), Sinf.PHang(any1) * Cosf.PHang(any2) - Sinf.PHang(any2) * Cosf.PHang(any1) },
        };

        internal static readonly RuleList CollapseRules = new RuleList
        {
            // {1}2 - {2}2
            { Powf.PHang(any1, const1) - Powf.PHang(any2, const2), 
                (Powf.PHang(any1, const1 / Num(2)) - Powf.PHang(any2, const2 / Num(2))) *
                (Powf.PHang(any1, const1 / Num(2)) + Powf.PHang(any2, const2 / Num(2))) },

            
            {
                Powf.PHang(any1, Num(2)) - const1,
                (any1 - Powf.PHang(const1, Num(0.5m))) * (any1 + Powf.PHang(const1, Num(0.5m)))
            },

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            { any1 * any2 + any1 * any3, any1 * (any2 + any3) },
            { any1 * any2 + any3 * any1, any1 * (any2 + any3) },
            { any2 * any1 + any3 * any1, any1 * (any2 + any3) },
            { any2 * any1 + any1 * any3, any1 * (any2 + any3) },
            { any1 + any1 * any2, any1 * (Num(1) + any2) },
            { any1 + any2 * any1, any1 * (Num(1) + any2) },
            { any1 * any2 + any1, any1 * (Num(1) + any2) },
            { any2 * any1 + any1, any1 * (Num(1) + any2) },
            { any1 + any1, Num(2) * any1 },

            { any1 * any2 - any1 * any3, any1 * (any2 - any3) },
            { any1 * any2 - any3 * any1, any1 * (any2 - any3) },
            { any2 * any1 - any3 * any1, any1 * (any2 - any3) },
            { any2 * any1 - any1 * any3, any1 * (any2 - any3) },
            { any1 - any1 * any2, any1 * (Num(1) - any2) },
            { any1 - any2 * any1, any1 * (Num(1) - any2) },
            { any1 * any2 - any1, any1 * (any2 - Num(1)) },
            { any2 * any1 - any1, any1 * (any2 - Num(1)) },

            // a ^ b * c ^ b = (a * c) ^ b
            { Powf.PHang(any1, any2) * Powf.PHang(any3, any2), Powf.PHang(any1 * any3, any2) },
        };
    }
}
