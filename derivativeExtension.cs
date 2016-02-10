/*
Katherine Kjeer
CS 251 Final Project
May 19, 2015
derivativeExtension.cs
Extension methods to find the n-th derivative of a real-valued function
	with respect to any variable
Treats mathematical functions as Expression trees
*/

namespace DerivativeExtension {

	using System; 
	using System.Linq.Expressions; 

	public static class DerivativeExtension {
		//*****MAIN DIFFERENTIATION FUNCTIONS*****//

		/*
		Derivative()
		returns the derivative of the given lambda expression w.r.t the variable represented by the given string
		*/
		public static Expression Derivative (this Expression expr, String withRespectTo) {
			LambdaExpression lambda = (LambdaExpression)expr;
			return Expression.Lambda(DerivativeBody(lambda.Body, withRespectTo), lambda.Parameters);
		}

		/*
		Derivative() (overloaded)
		returns the nth derivative of the given lambda expression w.r.t the variable represented by the given string
		*/
		public static Expression Derivative (this Expression expr, String withRespectTo, int n) {
			LambdaExpression lambda = (LambdaExpression)expr;
			Expression resultBody = lambda.Body;
			for (int i = 1; i <= n; i++) {
				resultBody = DerivativeBody(resultBody, withRespectTo);
			}
			return Expression.Lambda(resultBody, lambda.Parameters);
		}

		/*
		DerivativeBody()
		workhorse function for Derivative()
		returns the body of the derivative expression w.r.t the given string
		*/
		public static Expression DerivativeBody (this Expression currentBody, String withRespectTo) {
			//constant rule: dc/dx = 0
			if (currentBody.NodeType == ExpressionType.Constant) {
				return Expression.Constant(0.0, typeof(double));
			}
			
			//variable rule, treating non-w.r.t variables as constants
			if (currentBody.NodeType == ExpressionType.Parameter) {
				return dVariable(currentBody, withRespectTo);
			}

			//negation rule: (-x)' = -1 * x'
			if (currentBody.NodeType == ExpressionType.Negate) {
				return dNegate(currentBody, withRespectTo);
			}

			//addition rule: (f + g)' = f' + g'
			if (currentBody.NodeType == ExpressionType.Add) {
				return dSum(currentBody, withRespectTo);
			} 

			//subtraction rule: (f - g)' = f' - g'
			if (currentBody.NodeType == ExpressionType.Subtract) {
				return dDifference(currentBody, withRespectTo);
			}

			//product rule: (f * g)' = fg' + gf'
			if (currentBody.NodeType == ExpressionType.Multiply) {
				return dProduct(currentBody, withRespectTo);
			} 

			//quotient rule: (f / g)' = (gf' - fg')/g^2
			if (currentBody.NodeType == ExpressionType.Divide) {
				return dQuotient(currentBody, withRespectTo);
			}

			//power rule: (f ^ g)' = (f^g) * (g'*ln(f) + g*f'/f)
			if (currentBody.NodeType == ExpressionType.Power) {
				return dPower(currentBody, withRespectTo);
			}

			//math functions
			if (currentBody.NodeType == ExpressionType.Call) {
				return dMath(currentBody, withRespectTo);
			}

			return currentBody;
		}

		//******END MAIN DIFFERENTIATION FUNCTIONS******//

		//******DIFFERENTIATION HELPERS*****//

		/*
		dMath()
		Returns the derivative of an Expression resulting from calling a mathematical function (log, exp, trig, etc.)
		*/
		private static Expression dMath (Expression currentBody, String withRespectTo) {
			MethodCallExpression methodBody = (MethodCallExpression)currentBody;

			//natural log rule: ln(x)' = 1/x
			if (methodBody.Method.Name == "Log") {
				return dLog(methodBody, withRespectTo);
			} 

			//e^x rule: (e^x)' = e^x
			if (methodBody.Method.Name == "Exp") {
				return dExp(methodBody, withRespectTo);
			} 

			//power rule (for the Math.Pow call rather than Expression.Power used above)
			if (methodBody.Method.Name == "Pow") {
				return dPower(methodBody, withRespectTo);
			} 

			//absolute value rule: |x|' = x/|x|
			if (methodBody.Method.Name == "Abs") {
				return dAbs(methodBody, withRespectTo);
			}

			//square root rule: sqrt(x)' = 1/(2*sqrt(x))
			if (methodBody.Method.Name == "Sqrt") {
				return dSqrt(methodBody, withRespectTo);
			} 

			//sine rule: sin(x)' = cos(x)
			if (methodBody.Method.Name == "Sin") {
				return dSin(methodBody, withRespectTo);
			} 

			//cosine rule: cos(x)' = -sin(x)
			if (methodBody.Method.Name == "Cos") {
				return dCos(methodBody, withRespectTo);
			} 

			//tangent rule: tan(x)' = 1/cos(x)^2
			if (methodBody.Method.Name == "Tan") {
				return dTan(methodBody, withRespectTo);
			} 

			//inverse sine rule: asin(x)' = 1/sqrt(1 - x^2)
			if (methodBody.Method.Name == "Asin") {
				return dAsin(methodBody, withRespectTo);
			}

			//inverse cosine rule: acos(x)' = -1/sqrt(1 - x^2)
			if (methodBody.Method.Name == "Acos") {
				return dAcos(methodBody, withRespectTo);
			}

			//inverse tangent rule: atan(x)' = 1/(1 + x^2)
			if (methodBody.Method.Name == "Atan") {
				return dAtan(methodBody, withRespectTo);
			}

			return DerivativeBody(methodBody.Arguments[0], withRespectTo);
		}

		//******BASIC DIFFERENTIATION******//

		/*
		dVariable()
		Variable rule, treating non-w.r.t variables as constants
		*/
		private static Expression dVariable (Expression currentBody, String withRespectTo) {
			ParameterExpression paramBody = (ParameterExpression)currentBody;
			if (paramBody.Name == withRespectTo) {
				return Expression.Constant(1.0, typeof(double));
			}
			return Expression.Constant(0.0, typeof(double));
		}

		/*
		dNegate()
		Negation rule: (-x)' = -1 * x'
		*/
		private static Expression dNegate (Expression currentBody, String withRespectTo) {
			UnaryExpression unaryBody = (UnaryExpression)currentBody;
			return Expression.Multiply(
							Expression.Constant(-1.0, typeof(double)),
							DerivativeBody(unaryBody.Operand, withRespectTo)
						);
		}

		/*
		dSum()
		Addition rule: (f + g)' = f' + g'
		*/
		private static Expression dSum (Expression currentBody, String withRespectTo) {
			BinaryExpression binaryBody = (BinaryExpression)currentBody;
			return Expression.Add(DerivativeBody(binaryBody.Left, withRespectTo), DerivativeBody(binaryBody.Right, withRespectTo));
		}

		/*
		dDifference()
		Subtraction rule: (f - g)' = f' - g'
		*/
		private static Expression dDifference (Expression currentBody, String withRespectTo) {
			BinaryExpression binaryBody = (BinaryExpression)currentBody;
			return Expression.Subtract(DerivativeBody(binaryBody.Left, withRespectTo), DerivativeBody(binaryBody.Right, withRespectTo));
		}

		/*
		dProduct()
		Product rule: (f * g)' = fg' + gf'
		*/
		private static Expression dProduct (Expression currentBody, String withRespectTo) {
			BinaryExpression binaryBody = (BinaryExpression)currentBody;
			return Expression.Add(
							Expression.Multiply(
								binaryBody.Left, 
								DerivativeBody(binaryBody.Right, withRespectTo)
							), 
							Expression.Multiply(
								binaryBody.Right, 
								DerivativeBody(binaryBody.Left, withRespectTo)
							)
						);
		}

		/*
		dQuotient()
		Quotient rule: (f / g)' = (gf' - fg')/g^2
		*/
		private static Expression dQuotient (Expression currentBody, String withRespectTo) {
			BinaryExpression binaryBody = (BinaryExpression)currentBody;
			Expression f = binaryBody.Left;
			Expression g = binaryBody.Right;
			Expression fPrime = DerivativeBody(f, withRespectTo);
			Expression gPrime = DerivativeBody(g, withRespectTo);
			return Expression.Divide(
							Expression.Subtract(
								Expression.Multiply(g, fPrime), 
								Expression.Multiply(f, gPrime)
							), 
							Expression.Power(g, Expression.Constant(2.0, typeof(double)))
						);
		}

		/*
		dPower()
		Power rule: (f ^ g)' = (f^g) * (g'*ln(f) + g*f'/f)
		*/
		private static Expression dPower (Expression currentBody, String withRespectTo) {
			Expression f;
			Expression g;

			//a power expression could come from either Expression.Power or Expression.Call(Math.Pow)
			if (currentBody.NodeType == ExpressionType.Power) {
				BinaryExpression binaryBody = (BinaryExpression)currentBody;
				f = binaryBody.Left;
				g = binaryBody.Right;
			} else {
				MethodCallExpression methodBody = (MethodCallExpression)currentBody;
				f = methodBody.Arguments[0];
				g = methodBody.Arguments[1];
			}
			
			Expression fPrime = DerivativeBody(f, withRespectTo);
			Expression gPrime = DerivativeBody(g, withRespectTo);

			return Expression.Multiply(
							Expression.Power(f, g),
							Expression.Add(
								Expression.Multiply(
									gPrime, 
									Expression.Call(typeof(Math).GetMethod("Log", new Type[] {typeof(double)}), f)
								),
								Expression.Divide(
									Expression.Multiply(g, fPrime),
									f
								)
							)
						);
		}

		//*****END BASIC DIFFERENTIATION*****//

		//*****MATH FUNCTION DIFFERENTIATION (ALL FOLLOW CHAIN RULE)*****//

		/*
		dLog()
		Natural log rule: ln(x)' = 1/x
		*/
		private static Expression dLog (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Divide(
								Expression.Constant(1.0, typeof(double)), 
								methodBody.Arguments[0]
							), 
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dExp()
		e^x rule: (e^x)' = e^x
		*/
		private static Expression dExp (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Call(typeof(Math).GetMethod("Exp"), methodBody.Arguments[0]),
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dAbs()
		Absolute value rule: |x|' = x/|x|
		*/
		private static Expression dAbs (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Divide(
								methodBody.Arguments[0],
								Expression.Call(
									typeof(Math).GetMethod("Abs", new Type[] {typeof(double)}), methodBody.Arguments[0]
									)
								),
								DerivativeBody(methodBody.Arguments[0], withRespectTo)
							);
		}

		/*
		dSqrt()
		square root rule: sqrt(x)' = 1/(2*sqrt(x))
		*/
		private static Expression dSqrt (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Divide(
								Expression.Constant(1.0, typeof(double)),
								Expression.Multiply(
									Expression.Constant(2.0, typeof(double)),
									Expression.Call(typeof(Math).GetMethod("Sqrt"), methodBody.Arguments[0])
								)
							),
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dSin()
		Sine rule: sin(x)' = cos(x)
		*/
		private static Expression dSin (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Call(
								typeof(Math).GetMethod("Cos"), 
								methodBody.Arguments[0]
							), 
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dCos()
		Cosine rule: cos(x)' = -sin(x)
		*/
		private static Expression dCos (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Negate(
								Expression.Call(
									typeof(Math).GetMethod("Sin"), 
									methodBody.Arguments[0])
							), 
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dTan()
		Tangent rule: tan(x)' = 1/cos(x)^2
		*/
		private static Expression dTan (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Divide(
								Expression.Constant(1.0, typeof(double)), 
								Expression.Power(
									Expression.Call(typeof(Math).GetMethod("Cos"), methodBody.Arguments[0]),
									Expression.Constant(2.0, typeof(double))
								)
							),
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dAsin()
		Inverse sine rule: asin(x)' = 1/sqrt(1 - x^2)
		*/
		private static Expression dAsin (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Divide(
								Expression.Constant(1.0, typeof(double)),
								Expression.Call(
									typeof(Math).GetMethod("Sqrt"), 
									Expression.Subtract(
										Expression.Constant(1.0, typeof(double)), 
										Expression.Power(
											methodBody.Arguments[0], 
											Expression.Constant(2.0, typeof(double))
										)
									)
								)
							),
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dAcos()
		Inverse cosine rule: acos(x)' = -1/sqrt(1 - x^2)
		*/
		private static Expression dAcos (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Negate(
								Expression.Divide(
									Expression.Constant(1.0, typeof(double)),
									Expression.Call(
										typeof(Math).GetMethod("Sqrt"), 
										Expression.Subtract(
											Expression.Constant(1.0, typeof(double)), 
											Expression.Power(
												methodBody.Arguments[0], 
												Expression.Constant(2.0, typeof(double))
											)
										)
									)
								)
							),
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		/*
		dAtan()
		Inverse tangent rule: atan(x)' = 1/(1 + x^2)
		*/
		private static Expression dAtan (MethodCallExpression methodBody, String withRespectTo) {
			return Expression.Multiply(
							Expression.Divide(
								Expression.Constant(1.0, typeof(double)),
								Expression.Add(
									Expression.Constant(1.0, typeof(double)),
									Expression.Power(
										methodBody.Arguments[0],
										Expression.Constant(2.0, typeof(double))
									)
								)
							),
							DerivativeBody(methodBody.Arguments[0], withRespectTo)
						);
		}

		//******END MATH FUNCTION DIFFERENTIATION******//

		//******END DIFFERENTIATION HELPERS******//
	}

}