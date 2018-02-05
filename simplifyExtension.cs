/*
Katherine Kjeer
2015
simplifyExtension.cs
Extension methods to (partially) simplify a real-valued mathematical function
	e.g. replaces 1 * x with x, x + 0 with x, etc.
Treats mathematical functions as Expression trees
*/

namespace SimplifyExtension {

	using System; 
	using System.Linq.Expressions;

	public static class SimplifyExtension {

		/*
		Simplify()
		returns a new (lambda) expression with the same parameters and a simplified body
		*/
		public static Expression Simplify (this Expression expr) {
			LambdaExpression lambda = (LambdaExpression)expr;
			return Expression.Lambda(lambda.Body.SimplifyBody(), lambda.Parameters);
		}

		/*
		SimplifyBody()
		returns a new simplified expression
		*/
		public static Expression SimplifyBody (this Expression expr) {
			if (expr.NodeType == ExpressionType.Negate) {
				return SimplifyNegate(expr);
			}

			if (expr.NodeType == ExpressionType.Add) {
				return SimplifyAdd(expr);
			}

			if (expr.NodeType == ExpressionType.Subtract) {
				return SimplifySubtract(expr);
			}

			if (expr.NodeType == ExpressionType.Multiply) {
				return SimplifyMultiply(expr);
			}

			if (expr.NodeType == ExpressionType.Divide) {
				return SimplifyDivide(expr);
			}

			if (expr.NodeType == ExpressionType.Power) {
				return SimplifyPower(expr);
			}

			if (expr.NodeType == ExpressionType.Call) {
				return SimplifyMath(expr);
			}

			return expr;
		}

		/*
		SimplifyMath()
		Private helper to simplify math functions (log, trig, etc.)
		*/
		private static Expression SimplifyMath (Expression expr) {
			MethodCallExpression method = (MethodCallExpression)expr;

			if (method.Method.Name == "Log") {
				return SimplifyLog(method);
			}

			if (method.Method.Name == "Exp") {
				return SimplifyExp(method);
			}

			if (method.Method.Name == "Pow") {
				return SimplifyPower(method);
			}

			if (method.Method.Name == "Abs") {
				return SimplifyAbs(method);
			}

			if (method.Method.Name == "Sqrt") {
				return SimplifySqrt(method);
			}

			if (method.Method.Name == "Sin") {
				return SimplifySin(method);
			}

			if (method.Method.Name == "Cos") {
				return SimplifyCos(method);
			}

			if (method.Method.Name == "Tan") {
				return SimplifyTan(method);
			}

			if (method.Method.Name == "Asin") {
				return SimplifyAsin(method);
			}

			if (method.Method.Name == "Acos") {
				return SimplifyAcos(method);
			}

			if (method.Method.Name == "Atan") {
				return SimplifyAtan(method);
			}

			return expr;
		}

		//******VALUE FUNCTIONS******//

		/*
		getValue()
		Private helper to get the constant value of an expression
		*/
		private static double getValue (Expression expr) {
			if (expr.NodeType == ExpressionType.Constant) {
				ConstantExpression constant = (ConstantExpression)expr;
				double value = (double)constant.Value;
				return value;
			}
			throw new Exception("Expression " + expr + " is not a constant. Cannot get value.");
		}

		/*
		isZero()
		Private helper to check whether an expression is 0
		*/
		private static bool isZero (Expression expr) {
			if (expr.NodeType == ExpressionType.Constant) {
				ConstantExpression constant = (ConstantExpression)expr;
				double value = (double)constant.Value;
				return value == 0.0;
			}
			return false;
		}

		/*
		isOne()
		Private helper to check whether an expression is 1
		*/
		private static bool isOne (Expression expr) {
			if (expr.NodeType == ExpressionType.Constant) {
				ConstantExpression constant = (ConstantExpression)expr;
				double value = (double)constant.Value;
				return value == 1.0;
			}
			return false;
		}

		//******END VALUE FUNCTIONS******//

		//******BASIC SIMPLIFICATION******//

		/*
		SimplifyNegate()
		Replaces constant(-1) * constant(x) with constant(-1 * x)
		*/
		private static Expression SimplifyNegate (Expression expr) {
			UnaryExpression unary = (UnaryExpression)expr;
			Expression operand = unary.Operand.SimplifyBody();

			//the operand is a constant => return the constant -1 * operand
			if (operand.NodeType == ExpressionType.Constant) {
				return Expression.Constant(-1 * getValue(operand), typeof(double));
			}

			//nothing more can be done => return the negation of the operand
			return Expression.Negate(operand);
		}

		/*
		SimplifyAdd()
		Replaces constant(x) + constant(y) with constant(x + y)
		Replaces constant(0) + variable(y) with variable(y)
		Replaces variable(x) + constant(0) with variable(x)
		*/
		private static Expression SimplifyAdd (Expression expr) {
			BinaryExpression binary = (BinaryExpression)expr;
			Expression simplifiedLeft = binary.Left.SimplifyBody();
			Expression simplifiedRight = binary.Right.SimplifyBody();

			//the left is a constant
			if (simplifiedLeft.NodeType == ExpressionType.Constant) {
				double left = getValue(simplifiedLeft);

				//both the left and right are constants => return their constant sum
				if (simplifiedRight.NodeType == ExpressionType.Constant) {
					double right = getValue(simplifiedRight);
					return Expression.Constant(left + right, typeof(double));
				}

				//the left is 0 and the right isn't a constant => return the right
				if (left == 0) {
					return simplifiedRight;
				}
			} 

			//the left isn't a constant but the right is
			else if (simplifiedRight.NodeType == ExpressionType.Constant) {
				double right = getValue(simplifiedRight);

				//the right is 0 and the left isn't a constant => return the left
				if (right == 0) {
					return simplifiedLeft;
				}
			}

			//neither the left nor the right are constants => return their (non-constant) sum
			return Expression.Add(simplifiedLeft, simplifiedRight);
		}

		/*
		SimplifySubtract()
		Replaces constant(x) - constant(y) with constant(x - y)
		Replaces constant(0) - variable(y) with variable(-y)
		Replaces variable(x) - constant(y) with variable(x)
		*/
		private static Expression SimplifySubtract (Expression expr) {
			BinaryExpression binary = (BinaryExpression)expr;
			Expression simplifiedLeft = binary.Left.SimplifyBody();
			Expression simplifiedRight = binary.Right.SimplifyBody();

			//the left is a constant
			if (simplifiedLeft.NodeType == ExpressionType.Constant) {
				double left = getValue(simplifiedLeft);

				//both the left and right are constants => return their constant difference
				if (simplifiedRight.NodeType == ExpressionType.Constant) {
					double right = getValue(simplifiedRight);
					return Expression.Constant(left - right, typeof(double));
				}

				//the left is 0 and the right isn't a constant => return the negated right
				if (left == 0) {
					return Expression.Negate(simplifiedRight);
				}
			} 

			//the left isn't a constant but the right is
			else if (simplifiedRight.NodeType == ExpressionType.Constant) {
				double right = getValue(simplifiedRight);

				//the right is 0 and the left isn't a constant => return the left
				if (right == 0) {
					return simplifiedLeft;
				}
			}

			//neither the left nor the right are constants => return their (non-constant) difference
			return Expression.Subtract(simplifiedLeft, simplifiedRight);
		}

		/*
		SimplifyMultiply()
		Replaces constant(x) * constant(y) with constant(x * y)
		Replaces constant(0) * variable(y) with constant(0)
		Replaces constant(1) * variable(y) with variable(y)
		Replaces variable(x) * constant(0) with constant(0)
		Replaces variable(x) * constant(1) with variable(x)
		*/
		private static Expression SimplifyMultiply (Expression expr) {
			BinaryExpression binary = (BinaryExpression)expr;
			Expression simplifiedLeft = binary.Left.SimplifyBody();
			Expression simplifiedRight = binary.Right.SimplifyBody();

			//the left is a constant
			if (simplifiedLeft.NodeType == ExpressionType.Constant) {
				double left = getValue(simplifiedLeft);

				//the left and right are both constants => return their constant product
				if (simplifiedRight.NodeType == ExpressionType.Constant) {
					double right = getValue(simplifiedRight);
					return Expression.Constant(left*right, typeof(double));
				}

				//the left is 0 => return 0
				if (left == 0.0) {
					return Expression.Constant(0.0, typeof(double));
				} 

				//the left is 1 => return the right
				else if (left == 1.0) {
					return simplifiedRight;
				}
			} 

			//the left isn't a constant but the right is
			else if (simplifiedRight.NodeType == ExpressionType.Constant) {
				double right = getValue(simplifiedRight);

				//the right is 0 => return 0
				if (right == 0.0) {
					return Expression.Constant(0.0, typeof(double));
				} 

				//the right is 1 => return the left
				else if (right == 1.0) {
					return simplifiedLeft;
				}
			}

			//neither the left nor the right are constants => return their (non-constant) product
			return Expression.Multiply(simplifiedLeft, simplifiedRight);
		}

		/*
		SimplifyDivide()
		Replaces variable(x) / constant(0) with variable(x) / constant(0) (will be NaN when evaluated)
		Replaces variable(x) / variable(1) with constant(1)
		Replaces constant(0) / variable(y) with constant(0)
		Replaces variable(x) / constant(1) with variable(x)
		Replaces constant(x) / constant(y) with constant(x / y)
		*/
		private static Expression SimplifyDivide (Expression expr) {
			BinaryExpression binary = (BinaryExpression)expr;
			Expression simplifiedLeft = binary.Left.SimplifyBody();
			Expression simplifiedRight = binary.Right.SimplifyBody();

			//bottom is zero => return top/bottom
			if (isZero(simplifiedRight)) {
				return Expression.Divide(simplifiedLeft, simplifiedRight);
			}

			//top and bottom are equal => return 1
			if (simplifiedLeft == simplifiedRight) {
				return Expression.Constant(1.0, typeof(double));
			}

			//top is 0 => return 0 (don't care what the bottom is - the bottom could be a variable expression)
			if (isZero(simplifiedLeft)) {
				return Expression.Constant(0.0, typeof(double));
			}

			//bottom is 1 => return the top
			if (isOne(simplifiedRight)) {
				return simplifiedLeft;
			}

			//both top and bottom are some other constants => return their constant quotient
			if (simplifiedLeft.NodeType == ExpressionType.Constant && simplifiedRight.NodeType == ExpressionType.Constant) {
				return Expression.Constant(getValue(simplifiedLeft)/getValue(simplifiedRight), typeof(double));
			}

			//nothing more can be done => return the quotient of the top and the bottom
			return Expression.Divide(simplifiedLeft, simplifiedRight);
		}

		/*
		SimplifyPower()
		Replaces constant(0) ^ constant(0) with constant(0) ^ constant(0) (will be Nan when called)
		Replaces constant(0) ^ variable(y) with constant(0)
		Replaces constant(1) ^ variable(y) with constant(1)
		Replaces variable(x) ^ constant(0) with constant(1)
		Replaces constant(x) ^ constant(y) with constant(x ^ y)
		*/
		private static Expression SimplifyPower (Expression expr) {
			Expression simplifiedLeft;
			Expression simplifiedRight;

			//a power expression could either come from Expression.Power or Expression.Call(Math.Pow)
			if (expr.NodeType == ExpressionType.Power) {
				BinaryExpression binary = (BinaryExpression)expr;
				simplifiedLeft = binary.Left.SimplifyBody();
				simplifiedRight = binary.Right.SimplifyBody();
			} else {
				MethodCallExpression method = (MethodCallExpression)expr;
				simplifiedLeft = method.Arguments[0].SimplifyBody();
				simplifiedRight = method.Arguments[1].SimplifyBody();
			}
			
			//the base is 0
			if (isZero(simplifiedLeft)) {

				//the exponent is also 0 => return 0^0 (will be NaN when called)
				if (isZero(simplifiedRight)) {
					return Expression.Power(simplifiedLeft, simplifiedRight);
				}

				//the exponent is nonzero => return 0
				return Expression.Constant(0.0, typeof(double));
			}

			//the base is 1 => return 1 (don't care what the exponent is)
			//or the exponent is zero => return 1 (don't care what the base is)
			if (isOne(simplifiedLeft) || isZero(simplifiedRight)) {
				return Expression.Constant(1.0, typeof(double));
			}

			//the exponent is 1 => return the base
			if (isOne(simplifiedRight)) {
				return simplifiedLeft;
			}

			//both the base and the exponent are some other constants => return their constant exponentiation
			if (simplifiedLeft.NodeType == ExpressionType.Constant && simplifiedRight.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Pow(getValue(simplifiedLeft), getValue(simplifiedRight)), typeof(double));
			}

			//nothing more can be done
			return Expression.Power(simplifiedLeft, simplifiedRight);
		}

		//******END BASIC SIMPLIFICATION******//

		//******MATH SIMPLIFICATION******//

		/*
		SimplifyLog()
		Replaces ln(constant(x)) with constant(ln(x))
		Replaces ln(e^x) with x
		*/
		private static Expression SimplifyLog (MethodCallExpression method) {
			Expression logArg = method.Arguments[0].SimplifyBody();

			//the arg is a constant => return the constant ln(arg)
			if (logArg.NodeType == ExpressionType.Constant) {
				double logValue = getValue(logArg);
				return Expression.Constant(Math.Log(logValue), typeof(double));
			}

			//the arg is a method call
			if (logArg.NodeType == ExpressionType.Call) {
				MethodCallExpression logMethod = (MethodCallExpression)logArg;

				//calling ln(e^something) => return the simplified something
				if (logMethod.Method.Name == "Exp") {
					Expression expArg = logMethod.Arguments[0];
					return expArg.SimplifyBody();
				}
			}

			//nothing more can be done => return ln(arg)
			return Expression.Call(typeof(Math).GetMethod("Log", new Type [] {typeof(double)}), logArg);
		}

		/*
		SimplifyExp()
		Replaces e ^ constant(x) with constant(e ^ x)
		Replaces e ^ ln(x) with x
		*/
		private static Expression SimplifyExp (MethodCallExpression method) {
			Expression expArg = method.Arguments[0].SimplifyBody();

			//the arg is a constant => return the constant e^arg
			if (expArg.NodeType == ExpressionType.Constant) {
				double expValue = getValue(expArg);
				return Expression.Constant(Math.Exp(expValue), typeof(double));
			}

			//calling e^(some math function)
			if (expArg.NodeType == ExpressionType.Call) {
				//get the some math function that e is being raised to and cast it
				MethodCallExpression expMethod = (MethodCallExpression)expArg;

				//calling e^(ln(something)) => return the simplified something
				if (expMethod.Method.Name == "Log") {
					//get the argument to the log method
					Expression logArg = expMethod.Arguments[0];
					return logArg.SimplifyBody();
				}
			}

			//nothing more can be done => return e^arg
			return Expression.Call(typeof(Math).GetMethod("Exp"), expArg);
		}

		/*
		SimplifyAbs()
		Replaces |constant(x)| with constant(|x|)
		*/
		private static Expression SimplifyAbs (MethodCallExpression method) {
			Expression absArg = method.Arguments[0].SimplifyBody();

			//the arg is a constant => return the constant absolute value of the arg
			if (absArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Abs(getValue(absArg)), typeof(double));
			}

			//nothing more can be done => return |arg|
			return Expression.Call(typeof(Math).GetMethod("Abs", new Type[] {typeof(double)}), absArg);
		}

		/*
		SimplifySqrt()
		Replaces sqrt(constant(x)) with constant(sqrt(x))
		*/
		private static Expression SimplifySqrt (MethodCallExpression method) {
			Expression sqrtArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant square root of the arg
			if (sqrtArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Sqrt(getValue(sqrtArg)), typeof(double));
			}

			//nothing more can be done => return sqrt(arg)
			return Expression.Call(typeof(Math).GetMethod("Sqrt"), sqrtArg);
		}

		/*
		SimplifySin()
		Replaces sin(constant(x)) with constant(sin(x))
		*/
		private static Expression SimplifySin (MethodCallExpression method) {
			Expression sinArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant sin(arg)
			if (sinArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Sin(getValue(sinArg)), typeof(double));
			}

			//nothing more can be done => return sin(arg)
			return Expression.Call(typeof(Math).GetMethod("Sin"), sinArg);
		}

		/*
		SimplifyCos()
		Replaces cos(constant(x)) with constant(cos(x))
		*/
		private static Expression SimplifyCos (MethodCallExpression method) {
			Expression cosArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant cos(arg)
			if (cosArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Cos(getValue(cosArg)), typeof(double));
			}

			//nothing more can be done => return cos(arg)
			return Expression.Call(typeof(Math).GetMethod("Cos"), cosArg);
		}

		/*
		SimplifyTan()
		Replaces tan(constant(x)) with constant(tan(x))
		*/
		private static Expression SimplifyTan (MethodCallExpression method) {
			Expression tanArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant tan(arg)
			if (tanArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Tan(getValue(tanArg)), typeof(double));
			}

			//nothing more can be done => return tan(arg)
			return Expression.Call(typeof(Math).GetMethod("Tan"), tanArg);
		}

		/*
		SimplifyAsin()
		Replaces asin(constant(x)) with constant(asin(x))
		*/
		private static Expression SimplifyAsin (MethodCallExpression method) {
			Expression asinArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant asin(arg)
			if (asinArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Asin(getValue(asinArg)), typeof(double));
			}

			//nothing more can be done => return asin(arg)
			return Expression.Call(typeof(Math).GetMethod("Asin"), asinArg);
		}

		/*
		SimplifyAcos()
		Replaces acos(constant(x)) with constant(acos(x))
		*/
		private static Expression SimplifyAcos (MethodCallExpression method) {
			Expression acosArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant acos(arg)
			if (acosArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Acos(getValue(acosArg)), typeof(double));
			}

			//nothing more can be done => return cos(arg)
			return Expression.Call(typeof(Math).GetMethod("Acos"), acosArg);
		}

		/*
		SimplifyAtan()
		Replaces atan(constant(x)) with constant(atan(x))
		*/
		private static Expression SimplifyAtan (MethodCallExpression method) {
			Expression atanArg = method.Arguments[0].SimplifyBody();

			//arg is a constant => return the constant atan(arg)
			if (atanArg.NodeType == ExpressionType.Constant) {
				return Expression.Constant(Math.Atan(getValue(atanArg)), typeof(double));
			}

			//nothing more can be done => return atan(arg)
			return Expression.Call(typeof(Math).GetMethod("Atan"), atanArg);
		}

		//******END MATH SIMPLIFICATION******//

	} //end SimplifyExtension class
} //end SimplifyExtension namespace
