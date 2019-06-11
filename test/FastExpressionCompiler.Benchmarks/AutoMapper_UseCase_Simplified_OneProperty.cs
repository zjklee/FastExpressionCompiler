﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using static System.Linq.Expressions.Expression;
using L = FastExpressionCompiler.LightExpression.Expression;

namespace FastExpressionCompiler.Benchmarks
{
    public class AutoMapper_UseCase_Simplified_OneProperty
    {
        /*
        ## Initial results with not yet released v2.1
        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.765 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156250 Hz, Resolution=463.7681 ns, Timer=TSC
        .NET Core SDK=3.0.100-preview3-010431
          [Host]     : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT
          DefaultJob : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT


                              Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ---------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                             Compile | 285.788 us | 1.7830 us | 1.6679 us | 22.66 |    0.29 |      1.9531 |      0.9766 |           - |            10.93 KB |
                         CompileFast |  12.613 us | 0.1554 us | 0.1454 us |  1.00 |    0.00 |      0.7477 |      0.3662 |      0.0305 |             3.44 KB |
         CompileFast_LightExpression |   9.845 us | 0.0541 us | 0.0451 us |  0.78 |    0.01 |      0.7477 |      0.3662 |      0.0305 |             3.44 KB |

        ## Degradation after adding block / try-catch collection + added WithoutClosure for comparison

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 254.680 us | 1.0914 us | 1.0209 us | 13.38 |    0.13 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  19.031 us | 0.1987 us | 0.1858 us |  1.00 |    0.00 |      0.9766 |      0.4883 |      0.0305 |             4.61 KB |
                 CompileFast_WithoutClosure |   5.373 us | 0.0222 us | 0.0207 us |  0.28 |    0.00 |      0.6256 |           - |           - |             2.91 KB |
                CompileFast_LightExpression |  15.243 us | 0.1002 us | 0.0937 us |  0.80 |    0.01 |      0.9918 |      0.4883 |      0.0458 |             4.61 KB |
 CompileFast_LightExpression_WithoutClosure |   3.882 us | 0.0735 us | 0.0787 us |  0.20 |    0.01 |      0.6294 |           - |           - |             2.91 KB |

        ## Got it back a bit
                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 292.247 us | 2.0617 us | 1.9285 us | 15.18 |    0.09 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  19.261 us | 0.1001 us | 0.0887 us |  1.00 |    0.00 |      0.9460 |      0.4578 |      0.0305 |             4.34 KB |
                 CompileFast_WithoutClosure |  11.520 us | 0.0803 us | 0.0712 us |  0.60 |    0.00 |      0.7019 |      0.3510 |      0.0305 |             3.24 KB |
                CompileFast_LightExpression |  15.705 us | 0.0879 us | 0.0822 us |  0.82 |    0.01 |      0.9460 |      0.4578 |      0.0305 |             4.34 KB |
 CompileFast_LightExpression_WithoutClosure |   9.457 us | 0.0890 us | 0.0744 us |  0.49 |    0.00 |      0.7019 |      0.3510 |      0.0305 |             3.24 KB |
        */

        [MemoryDiagnoser]
        public class Compile_only
        {
            private static readonly Expression<Func<Source, Dest, ResolutionContext, Dest>> _expression = CreateExpression();
            private static readonly LightExpression.Expression<Func<Source, Dest, ResolutionContext, Dest>> _lightExpression = CreateLightExpression();

            [Benchmark]
            public object Compile() => _expression.Compile();

            [Benchmark(Baseline = true)]
            public object CompileFast() => _expression.CompileFast();

            [Benchmark]
            public object CompileFast_WithoutClosure() => 
                _expression.TryCompileWithoutClosure<Func<Source, Dest, ResolutionContext, Dest>>();

            [Benchmark]
            public object CompileFast_LightExpression() => 
                LightExpression.ExpressionCompiler.CompileFast(_lightExpression);

            [Benchmark]
            public object CompileFast_LightExpression_WithoutClosure() => 
                LightExpression.ExpressionCompiler.TryCompileWithoutClosure<Func<Source, Dest, ResolutionContext, Dest>>(_lightExpression);
        }

        /*
        ## Initial results with not yet released v2.1

        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.765 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156250 Hz, Resolution=463.7681 ns, Timer=TSC
        .NET Core SDK=3.0.100-preview3-010431
          [Host]     : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT
          DefaultJob : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT


                                       Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ------------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                             Create_n_Compile | 314.09 us | 1.6548 us | 1.5479 us | 11.49 |    0.08 |      2.9297 |      1.4648 |           - |            13.82 KB |
                         Create_n_CompileFast |  27.34 us | 0.1675 us | 0.1566 us |  1.00 |    0.00 |      1.3733 |      0.6714 |      0.0305 |             6.38 KB |
         Create_n_CompileFast_LightExpression |  12.53 us | 0.0818 us | 0.0765 us |  0.46 |    0.00 |      1.2512 |      0.6256 |      0.0458 |             5.78 KB |
        */
        [MemoryDiagnoser]
        public class Create_and_Compile
        {
            [Benchmark]
            public object Create_n_Compile() => CreateExpression().Compile();

            [Benchmark(Baseline = true)]
            public object Create_n_CompileFast() => CreateExpression().CompileFast();

            [Benchmark]
            public object Create_n_CompileFast_LightExpression() => LightExpression.ExpressionCompiler.CompileFast(CreateLightExpression());
        }

        /*
                                      Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ------------------------------------ |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
                             Invoke_Compiled | 8.094 ns | 0.0358 ns | 0.0335 ns |  0.98 |      0.0051 |           - |           - |                24 B |
                         Invoke_CompiledFast | 8.235 ns | 0.0529 ns | 0.0469 ns |  1.00 |      0.0051 |           - |           - |                24 B |
         Invoke_CompiledFast_LightExpression | 8.219 ns | 0.0264 ns | 0.0247 ns |  1.00 |      0.0051 |           - |           - |                24 B |
        */
        [MemoryDiagnoser]
        public class Invoke_compiled_delegate
        {
            private static readonly Func<Source, Dest, ResolutionContext, Dest> _compiled = CreateExpression().Compile();
            private static readonly Func<Source, Dest, ResolutionContext, Dest> _compiledFast = CreateExpression().CompileFast();
            private static readonly Func<Source, Dest, ResolutionContext, Dest> _compiledFastLE = LightExpression.ExpressionCompiler.CompileFast(CreateLightExpression());

            private static readonly Source _source = new Source { Value = 42 };

            [Benchmark]
            public Dest Invoke_Compiled() => _compiled(_source, null, null);

            [Benchmark]
            public Dest Invoke_CompiledFast() => _compiledFast(_source, null, null);

            [Benchmark(Baseline = true)]
            public Dest Invoke_CompiledFast_LightExpression() => _compiledFastLE(_source, null, null);
        }

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        public class ResolutionContext { }

        public class AutoMapperException : Exception
        {
            public AutoMapperException(string message, Exception innerException) : base(message, innerException) { }
        }

        private static Expression<Func<Source, Dest, ResolutionContext, Dest>> CreateExpression()
        {
            var srcParam = Parameter(typeof(Source), "source");
            var destParam = Parameter(typeof(Dest), "dest");

            var typeMapDestVar   = Parameter(typeof(Dest), "d");
            var resolvedValueVar = Parameter(typeof(int), "val");
            var exceptionVar     = Parameter(typeof(Exception), "ex");

            var expression = Lambda<Func<Source, Dest, ResolutionContext, Dest>>(
                Block(
                    Condition(
                        Equal(srcParam, Constant(null)),
                        Default(typeof(Dest)),
                        Block(typeof(Dest), new[] { typeMapDestVar },
                            Assign(
                                typeMapDestVar,
                                Coalesce(destParam, New(typeof(Dest).GetTypeInfo().DeclaredConstructors.First()))),
                            TryCatch(
                                /* Assign src.Value */
                                Block(new[] { resolvedValueVar },
                                    Block(
                                        Assign(resolvedValueVar,
                                            Condition(Or(Equal(srcParam, Constant(null)), Constant(false)),
                                                Default(typeof(int)),
                                                Property(srcParam, "Value"))
                                        ),
                                        Assign(Property(typeMapDestVar, "Value"), resolvedValueVar)
                                    )
                                ),
                                Catch(exceptionVar,
                                    Throw(
                                        New(typeof(AutoMapperException).GetTypeInfo().DeclaredConstructors.First(),
                                            Constant("Error mapping types."),
                                            exceptionVar),
                                        typeof(int))) // should skip this, cause does no make sense after the throw
                            ),
                            typeMapDestVar))
                ),
                srcParam, destParam, Parameter(typeof(ResolutionContext), "_")
            );

            return expression;
        }

        private static LightExpression.Expression<Func<Source, Dest, ResolutionContext, Dest>> CreateLightExpression()
        {
            var srcParam  = L.Parameter(typeof(Source), "source");
            var destParam = L.Parameter(typeof(Dest), "dest");

            var exceptionVar     = L.Parameter(typeof(Exception), "ex");
            var typeMapDestVar   = L.Parameter(typeof(Dest), "d");
            var resolvedValueVar = L.Parameter(typeof(int), "val");

            var expression = L.Lambda<Func<Source, Dest, ResolutionContext, Dest>>(
                L.Block(
                    L.Condition(
                        L.Equal(srcParam, L.Constant(null)),
                        L.Default(typeof(Dest)),
                        L.Block(typeof(Dest), new[] { typeMapDestVar },
                            L.Assign(
                                typeMapDestVar,
                                L.Coalesce(destParam, L.New(typeof(Dest).GetTypeInfo().DeclaredConstructors.First()))),
                            L.TryCatch(
                                /* Assign src.Value */
                                L.Block(new[] { resolvedValueVar },
                                    L.Block(
                                        L.Assign(resolvedValueVar,
                                            L.Condition(L.Or(L.Equal(srcParam, L.Constant(null)), L.Constant(false)),
                                                L.Default(typeof(int)),
                                                L.Property(srcParam, "Value"))
                                        ),
                                        L.Assign(L.Property(typeMapDestVar, "Value"), resolvedValueVar)
                                    )
                                ),
                                L.Catch(exceptionVar,
                                    L.Throw(
                                        L.New(typeof(AutoMapperException).GetTypeInfo().DeclaredConstructors.First(),
                                            L.Constant("Error mapping types."),
                                            exceptionVar),
                                        typeof(int))) // should skip this, cause does no make sense after the throw
                            ),
                            typeMapDestVar))
                ),
                srcParam, destParam, L.Parameter(typeof(ResolutionContext), "_")
            );

            return expression;
        }
    }
}
