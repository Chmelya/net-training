using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Reflection.Tasks
{
    public class CodeGeneration
    {
        /// <summary>
        /// Returns the functions that returns vectors' scalar product:
        /// (a1, a2,...,aN) * (b1, b2, ..., bN) = a1*b1 + a2*b2 + ... + aN*bN
        /// Generally, CLR does not allow to implement such a method via generics to have one function for various number types:
        /// int, long, float. double.
        /// But it is possible to generate the method in the run time! 
        /// See the idea of code generation using Expression Tree at: 
        /// http://blogs.msdn.com/b/csharpfaq/archive/2009/09/14/generating-dynamic-methods-with-expression-trees-in-visual-studio-2010.aspx
        /// </summary>
        /// <typeparam name="T">number type (int, long, float etc)</typeparam>
        /// <returns>
        ///   The function that return scalar product of two vectors
        ///   The generated dynamic method should be equal to static MultuplyVectors (see below).   
        /// </returns>
        public static Func<T[], T[], T> GetVectorMultiplyFunction<T>() where T : struct 
        {
            ParameterExpression firstVector = Expression.Parameter(typeof(T[]));
            ParameterExpression secondVector = Expression.Parameter(typeof(T[]));

            ParameterExpression result = Expression.Parameter(typeof(T));
            ParameterExpression index = Expression.Parameter(typeof(int));

            LabelTarget label = Expression.Label(typeof(T));

            BlockExpression block = Expression.Block
                (
                    //variables
                    new[] { result, index }

                    //body
                    , Expression.Assign(index, Expression.Constant(0))
                    , Expression.Loop
                    (
                        Expression.IfThenElse
                        (
                            //condition
                            Expression.LessThan(index, Expression.ArrayLength(firstVector))

                            //if true
                            , Expression.AddAssign(
                                  result
                                , Expression.Multiply(
                                      Expression.ArrayAccess(firstVector, index)
                                    , Expression.ArrayAccess(secondVector, Expression.PostIncrementAssign(index))
                                )
                            )

                            //if flase
                            , Expression.Break(label, result)
                        )

                        //return
                        , label
                    )
                );


            return Expression.Lambda<Func<T[], T[], T>>(block, firstVector, secondVector).Compile();
        } 


        // Static solution to check performance benchmarks
        public static int MultuplyVectors(int[] first, int[] second) {
            int result = 0;
            for (int i = 0; i < first.Length; i++) {
                result += first[i] * second[i];
            }
            return result;
        }

    }
}
