/*
Katherine Kjeer
CS 251 Final Project
May 19, 2015
VectorField.cs
Class to implement an n-dimensional vector field,
	including div, grad and curl operations
*/

using DerivativeExtension;
using SimplifyExtension;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

public class VectorField {
	//functions: array of LambdaExpressions that take an n-dimensional vector and output a real number
	private LambdaExpression [] functions;
	public LambdaExpression [] Functions {
		get {return functions;}
	}

	/*
	VectorField()
	Constructor
	Populates the functions array with functions constructed from the single given expression
	For example, if the given expression is (x, y) => {x, y}, then the functions array will contain:
		(x, y) => x
		(x, y) => y
	Aborts construction if the given expression's parameters and return array are of different lengths
	For example, the following expressions will abort:
		(x, y) => {x, y, z}
		(x, y) => {x}
	*/
	public VectorField (Expression func) {
		//cast the given function as a lambda expression
		LambdaExpression lambda = (LambdaExpression)func;

		//get the parameters of the given function (will be the same as the parameters for each function in the functions array)
		List<ParameterExpression> paramsList = new List<ParameterExpression>(lambda.Parameters);

		//initialize the functions array with 0 length
		//so nothing happens in ToString()
		functions = new LambdaExpression [0];

		//get the body of the given function
		NewArrayExpression funcBody = (NewArrayExpression)lambda.Body;

		//get all the elements in the array that comprises the given function body
		List<Expression> bodyList = new List<Expression>(funcBody.Expressions);

		//check for equal parameters/return elements
		if (paramsList.Count != bodyList.Count) {	
			Console.WriteLine("Unequal number of parameters (" + paramsList.Count + ") and return values (" + bodyList.Count + "). " + 
												"Aborting VectorField construction.");
			return;
		}

		//re-initialize the functions array now that it will be populated
		functions = new LambdaExpression [paramsList.Count];

		//populate the functions array
		for (int i = 0; i < functions.Length; i++) {
			functions[i] = Expression.Lambda(bodyList[i], lambda.Parameters);
		}
	}

	/*
	ToString()
	Overrides the default Object.ToString()
	Returns a string representation of a VectorField by listing all of the constituent functions
	*/
	public override string ToString () {
		string result = "";
		for (int i = 0; i < functions.Length; i++) {
			result += "\n" + "F_" + i + functions[i].Simplify();
		}
		return result;
	}

	/*
	Gradient()
	Static method that returns a new VectorField constructed from the gradient of the given 
		scalar field (a lambda expression)
	Example: if the given lambda expression is (x, y) => x*y, the VectorField functions will be
		(x, y) => y (partial of lambda w.r.t x)
		(x, y) => x (partial of lambda w.r.t y)
	*/
	public static VectorField Gradient (LambdaExpression scalarField) {
		//get the list of parameters to the scalar field (need to differentiate the body w.r.t. each parameter)
		List<ParameterExpression> paramsList = new List<ParameterExpression>(scalarField.Parameters);

		//array to hold each differentiated expression
		Expression [] resultArray = new Expression [paramsList.Count];

		//populate the resultArray by differentiating the scalarField body w.r.t each of its parameters
		for (int i = 0; i < paramsList.Count; i++) {
			resultArray[i] = scalarField.Body.DerivativeBody(paramsList[i].Name);
		}

		//create the new function body out of the resultArray
		NewArrayExpression funcBody = Expression.NewArrayInit(typeof(double), resultArray);

		//create and return the vector field
		Expression func = Expression.Lambda(funcBody, scalarField.Parameters);
		return new VectorField(func);
	}

	/*
	Divergence()
	Instance method to return the divergence (a lambda expression) of a VectorField
	Divergence = d(functions[0])/dparam0 + d(functions[1])/dparam1 + ... + d(functions[n])dparamN
		where param0, ..., paramN are the parameters (x, y, z, ...) to the function
	*/
	public LambdaExpression Divergence () {
		//check that there is at least one function in the vector field - if not, return a default LambdaExpression
		if (functions.Length < 1) {
			Console.WriteLine("No functions in VectorField. Aborting Divergence().");
			return default(LambdaExpression);
		}

		//get the parameters of the 0th expression (same as the parameters for all the functions)
		//these are the same parameters as the resulting divergence expression
		List<ParameterExpression> paramsList = new List<ParameterExpression>(functions[0].Parameters);

		Expression divBody = functions[0].Body.DerivativeBody(paramsList[0].Name);

		for (int i = 1; i < functions.Length; i++) {
			//differentiate the body of the ith function
			Expression divBodyToAdd = functions[i].Body.DerivativeBody(paramsList[i].Name);

			//add the differentiated body to the current divBody
			divBody = Expression.Add(divBody, divBodyToAdd);
		}

		return Expression.Lambda(divBody, functions[0].Parameters);
	}

	/*
	Curl()
	Instance method that returns a new VectorField that is the curl of this
	Only defined for three-dimensional VectorFields, returns a default VectorField if this is not three-dimensional
	*/
	public VectorField Curl () {
		//check for three dimensions
		if (functions.Length != 3) {
			Console.WriteLine("Curl is only defined for three-dimensional VectorFields. " + 
													"Not defined for " + functions.Length + "-dimensional VectorFields. Aborting Curl().");
			return default(VectorField);
		}

		//get the parameters of the 0th function, will be the parameters to the new VectorField
		List<ParameterExpression> paramsList = new List<ParameterExpression>(functions[0].Parameters);

		//the array to hold the results of the new VectorField
		Expression [] resultArray = new Expression [paramsList.Count];

		//df2/dy - df1/dz
		resultArray[0] = Expression.Subtract(functions[2].Body.DerivativeBody(paramsList[1].Name), 
																					functions[1].Body.DerivativeBody(paramsList[2].Name));

		//df2/x - df0/dz
		resultArray[1] = Expression.Subtract(functions[2].Body.DerivativeBody(paramsList[0].Name), 
																					functions[0].Body.DerivativeBody(paramsList[2].Name));

		//df1/dx - df0/dy
		resultArray[2] = Expression.Subtract(functions[1].Body.DerivativeBody(paramsList[0].Name), 
																					functions[0].Body.DerivativeBody(paramsList[1].Name));

		//create the function used to create the new VectorField
		NewArrayExpression funcBody = Expression.NewArrayInit(typeof(double), resultArray);
		Expression func = Expression.Lambda(funcBody, functions[0].Parameters);

		return new VectorField(func);
	}

}