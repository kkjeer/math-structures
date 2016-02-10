/*
Katherine Kjeer
CS 251 Final Project
May 19, 2015
MathDriver.cs
Contains the testing code for the remaining project files
This project requires installing mono (http://www.mono-project.com/download/) in order to run on a Mac.
To run, if all code files are stored in DIRECTORY:
> cd PATH/TO/DIRECTORY
> mcs /t:exe /out:MathDriver.exe MathDriver.cs VectorField.cs Group.cs derivativeExtension.cs simplifyExtension.cs listExtension.cs setExtension.cs
> mono MathDriver.exe
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ListExtension;
using SetExtension;
using DerivativeExtension;
using SimplifyExtension;

public class MathDriver {
	public static void Main (string [] args) {
		TestVectorField();
		TestGroup();
		TestSet();
		TestList();
	}

	/*
	TestVectorField()
	Tests various methods of the VectorField class
	*/
	public static void TestVectorField () {
		Console.WriteLine("****** Testing VectorField ******");

		//testing constructor
		Expression<Func<double, double, double, double []>> func = (x, y, z) => new double [] {x*Math.Cos(y), x*y, 2/z};
		VectorField vectorField = new VectorField(func);
		Console.WriteLine("vectorField: " + vectorField);

		//testing Divergence
		LambdaExpression divergence = vectorField.Divergence();
    Console.WriteLine("divergence: " + divergence);
    Console.WriteLine("simplified divergence: " + divergence.Simplify());
    Func<double, double, double, double> divFunc = (Func<double, double, double, double>)divergence.Compile();
    Console.WriteLine("divFunc(1, 1, 1): " + divFunc(1, 1, 1));

    //testing Curl
    VectorField curl = vectorField.Curl();
    Console.WriteLine("curl: " + curl);

    //testing Gradient
    Expression<Func<double, double, double>> gradFunc = (x, y) => Math.Cos(x)*Math.Sin(y);
    VectorField gradField = VectorField.Gradient(gradFunc);
    Console.WriteLine("gradField: " + gradField);

    //testing Divergence (using the gradient field calculated above)
    LambdaExpression divOfGrad = gradField.Divergence();
    Console.WriteLine("simplified divOfGrad: " + divOfGrad.Simplify());
    Func<double, double, double> divOfGradFunc = (Func<double, double, double>)divOfGrad.Compile();
    Console.WriteLine("divOfGradFunc(1, 1): " + divOfGradFunc(1, 1));

    Console.WriteLine();
	}

	/*
	TestGroup()
	Tests various methods of the Group class
	*/
	public static void TestGroup () {
		Console.WriteLine("****** Testing Group ******");

		//Z/8Z
		List<int> intList = new List<int>() {0, 1, 2, 3, 4, 5, 6, 7};
		Expression<Func<int, int, int>> intExpr = (x, y) => (x + y) % 8;
		Group<int> z8z = new Group<int>(intList, intExpr, "Z/8Z");
		TestOneGroup(z8z);

		//perm({1, 2, 3})
		List<Dictionary<int, int>> permList = new List<Dictionary<int, int>> () {
																																							new Dictionary<int, int>(){{1, 1}, {2, 2}, {3, 3}},
																																							new Dictionary<int, int>(){{1, 1}, {2, 3}, {3, 2}},
																																							new Dictionary<int, int>(){{1, 2}, {2, 1}, {3, 3}},
																																							new Dictionary<int, int>(){{1, 2}, {2, 3}, {3, 1}},
																																							new Dictionary<int, int>(){{1, 3}, {2, 1}, {3, 2}},
																																							new Dictionary<int, int>(){{1, 3}, {2, 2}, {3, 1}},
																																						};
		Expression<Func<Dictionary<int, int>, Dictionary<int, int>, Dictionary<int, int>>> permExpr = (dict1, dict2) => 
																																																		new Dictionary<int, int> () {
																																																			{1, dict1[dict2[1]]},
																																																			{2, dict1[dict2[2]]},
																																																			{3, dict1[dict2[3]]}
																																																		};																																														
		Group<Dictionary<int, int>> permGroup = new Group<Dictionary<int, int>>(permList, permExpr, "Permutations on {1, 2, 3}");
		TestOneGroup(permGroup);
	}

	/*
	TestOneGroup()
	Tests the methods of the Group class on the given group
	*/
	public static void TestOneGroup<T> (Group<T> group) {
		Console.WriteLine("Testing " + group);

		//identity and inverse
		Console.WriteLine("Identity of " + group.Name + ": " + group.GetIdentity());
		Console.WriteLine("Inverses of all elements in " + group.Name + ": " + group.GetAllInverses().DictString());

		//generator set
		Console.WriteLine("Generator set of " + group.Name + ": " + group.GetGeneratorSet().StringRep());

		//abelian and center
		Console.WriteLine("Is " + group.Name + " abelian? " + group.IsAbelian());
		Console.WriteLine(group.GetCenter());

		//subgroups, cyclic subgroups and normal subgroups
		Console.WriteLine("Subgroups of " + group.Name + ": " + group.GetSubgroups().LineString());
		Console.WriteLine("Cyclic subgroups of " + group.Name + ": " + group.GetCyclicSubgroups().LineString());
		Console.WriteLine("Normal subgroups of " + group.Name + ": " + group.GetNormalSubgroups().LineString());

		//left and right cosets and quotient groups
		Console.WriteLine("All left cosets of " + group.Name + ": " + group.GetAllLeftCosets().LineString());
		Console.WriteLine("All right cosets of " + group.Name + ": " + group.GetAllRightCosets().LineString());
		Console.WriteLine("All quotient groups of " + group.Name + ": " + group.QuotientString());

		Console.WriteLine();
	}

	/*
	TestList()
	Tests various extension methods of the ListExtension class
	*/
	public static void TestList () {
		Console.WriteLine("****** Testing List ******");

		List<List<List<int>>> nested1 = new List<List<List<int>>>(){
																		new List<List<int>>(){
																			new List<int>(){1, 2, 3}, 
																			new List<int>(){4, 5, 6}
																		}, 
																		new List<List<int>>(){
																			new List<int>(){7, 8, 9}, 
																			new List<int>(){10, 11, 12}
																		}
																	};
		List<List<List<int>>> nested2 = new List<List<List<int>>>(){
																		new List<List<int>>(){
																			new List<int>(){7, 8, 9}, 
																			new List<int>(){10, 11, 12}
																		}, 
																		new List<List<int>>(){
																			new List<int>(){4, 5, 6}, 
																			new List<int>(){1, 2, 3}
																		}
																	};

		List<List<int>> sequence1 = new List<List<int>>(){
																	new List<int>(){1, 2, 3}, 
																	new List<int>(){4, 5, 6}
																};
		List<List<int>> sequence2 = new List<List<int>>(){
																	new List<int>(){34, 70, -1}, 
																	new List<int>(){15, 0, -4}
																};

		List<List<int>> filter = new List<List<int>>(){
																new List<int>(){1, 2, 3}, 
																new List<int>(){4, 5, 6},
																new List<int>(){3, 2, 1},
																new List<int>(){7, 8, 9}
															};

		//StringRep() and LineString()
		Console.WriteLine("Testing string representations");														
		Console.WriteLine("nested1.StringRep(): " + nested1.StringRep());
		Console.WriteLine("nested2.LineString(): " + nested2.LineString());
		Console.WriteLine("sequence1: " + sequence1.StringRep());
		Console.WriteLine("sequence2: " + sequence2.StringRep());
		Console.WriteLine("filter: " + sequence2.StringRep());
		Console.WriteLine();

		//EqualLists()
		Console.WriteLine("Testing EqualLists()");
		Console.WriteLine("nested1.EqualLists(nested2)? (expected: True) " + nested1.EqualLists(nested2));
		Console.WriteLine("sequence1.EqualLists(sequence2)? (expected: False) " + sequence1.EqualLists(sequence2));
		Console.WriteLine();

		//ContainsSequence()
		Console.WriteLine("Testing ContainsSequence()");
		Console.WriteLine("nested1.ContainsSequence(sequence1)? (expected: True) " + nested1.ContainsSequence(sequence1));
		Console.WriteLine("nested1.ContainsSequence(sequence2)? (expected: False) " + nested1.ContainsSequence(sequence2));
		Console.WriteLine();

		//FilterSequence()
		Console.WriteLine("Testing FilterSequence()");
		Console.WriteLine("filter.FilterSequence():\n\texpected: {{1, 2, 3}, {4, 5, 6}, {7, 8, 9}\n\t" + filter.FilterSequence().StringRep());
		Console.WriteLine();
	}

	public static void TestSet () {
		Console.WriteLine("****** Testing Set ******");

		List<int> intlist1 = new List<int>() {1, 2, 3, 4, 5, 6, 7, 8};
		List<int> intlist2 = new List<int>() {3, 4, 5, 6, 7};
		TestTwoSets(intlist1, intlist2, false);

		List<string> stringlist1 = new List<string>() {"hello", "world", "cs", "251"};
		List<string> stringlist2 = new List<string>() {"hello", "world", "programming", "languages"};
		TestTwoSets(stringlist1, stringlist2, true);
	}

	/*
	TestTwoSets()
	Tests the methods of the SetExtension class on the two given lists
	*/
	public static void TestTwoSets<T> (List<T> list1, List<T> list2, bool testPowerSet) {
		string listsString = list1.StringRep() + " and " + list2.StringRep();
		Console.WriteLine("Testing " + listsString);

		//testing set operations
		printSetOp(list1.Intersection(list2).StringRep(), "intersection of " + listsString);
		printSetOp(list1.Union(list2).StringRep(), "union of " + listsString);
		printSetOp(list1.Complement(list2).StringRep(), "relative complement of " + listsString);
		printSetOp(list2.Complement(list1).StringRep(), "relative complement of " + list2.StringRep() + " and " + list1.StringRep());
		printSetOp(list1.Difference(list2).StringRep(), "symmetric difference of " + listsString);
		printSetOp(list1.CartesianProduct(list2).StringRep(), "cartesian product of " + listsString);
		if (testPowerSet) {
			printSetOp(list1.PowerSet().LineString(), "power set of " + list1.StringRep());
		}

		//testing set functions
		string funcString = " from " + list1.StringRep() + " to " + list2.StringRep();
		printSetFunc(list1, list1.Bijection(list2), "bijection" + funcString);
		printSetFunc(list1, list1.Injection(list2), "injection" + funcString);
		printSetFunc(list1, list1.Surjection(list2), "surjection" + funcString);
	}

	/*
	printSetOp()
	Prints the result of the given set operation (intersection, union, etc)
	Helper for TestTwoSets()
	*/
	public static void printSetOp (string setOp, string message) {
		Console.WriteLine("Testing " + message);
		Console.WriteLine(setOp);
		Console.WriteLine();
	}

	/*
	printSetFunc()
	Prints the result of the given set function (bijection, injection, surjection)
	Helper for TestTwoSets()
	*/
	public static void printSetFunc<A, B> (List<A> domain, Func<A, B> func, string message) {
		Console.WriteLine("Testing " + message);
		foreach (A input in domain) {
			Console.WriteLine(input + " => " + func(input));
		}
		Console.WriteLine();
	}
}