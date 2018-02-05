/*
Katherine Kjeer
2015
Group.cs
Class to implement groups, consisting of a list and a binary operation
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ListExtension;
using SetExtension;

public class Group<T> {
	//expr: Expression that contains the binary operation of the group
	//useful in ToString(), since Expressions print more nicely than Funcs
	//no getter: never needs to be accessed outside the Group class
	private Expression<Func<T, T, T>> expr;

	//list: list that contains the elements of the group
	private List<T> list;
	public List<T> List {
		get {return list;}
	}

	//function: the binary operation of the group
	//the compiled version of expr
	private Func<T, T, T> function;
	public Func<T, T, T> Function {
		get {return function;}
	}

	//name: the optional string name of the group
	//useful for distinguishing groups in testing
	private string name;
	public string Name {
		get {return name;}
		set {name = value;}
	}

	/*
	Group()
	Constructor
	Creates a Group from a List and and an Expression (and an optional name)
	Checks whether the given list and expression form a group (fulfill all four group axioms)
		and if not, sets the properties to their default values
	*/
	public Group (List<T> listParam, Expression<Func<T, T, T>> exprParam, string optionalName = "Nameless") {
		//remove duplicates on the list
		list = listParam.Where(elt => true).Distinct().ToList();

		//get the expr and the function (compiled expr)
		expr = exprParam;
		function = expr.Compile();

		name = optionalName;

		//check for group axioms
		if (!IsGroup()) {
			list = default(List<T>);
			function = default(Func<T, T, T>);
			expr = default(Expression<Func<T, T, T>>);
			name = "";
		}
		
	}

	/*
	ToString()
	Overrides the default Object.ToString()
	Returns a string representation of a Group by printing a string representation of its list and its expression
		(not its function - expressions have a much nicer ToString() method)
	*/
	public override string ToString () {
		return name + " Group -> Set: " + list.StringRep() + " Operation: " + expr;
	}

	public string QuotientString () {
		return GetAllQuotients().Select(quotient => quotient.Name + " Group -> Set: " + quotient.List.StringRep()).ToList().LineString();
	}

	/*
	Equals()
	Returns true iff this and other have the same lists and the same expression
	*/
	public bool Equals (Group<T> other) {
		return list.EqualLists(other.List) && expr.Equals(other.expr);
	}

	/*
	IsGroup()
	Returns true iff this fulfills the four group axioms:
		closure: for all (x, y) in list, function(x, y) is in list
		associativity: for all (x, y, z) in list, function(x, function(y, z)) == function(function(x, y), z)
		identity: there is some e in list such that for all x in list, function(e, x) == function(x, e) == x
		inverse: for each x in list, there is some y in list such that function(x, y) == function(y, x) == e
	*/
	public bool IsGroup () {
		//check closure
		List<Tuple<T, T>> pairs = (from item in list from item2 in list select new Tuple<T, T>(item, item2)).Distinct().ToList();
		List<Tuple<T, T>> closedSet = pairs.Where(tuple => list.ContainsSequence(function(tuple.Item1, tuple.Item2))).Distinct().ToList();
		if (closedSet.Count != pairs.Count) {
			return false;
		}

		//check associativity
		List<Tuple<T, T, T>> triples = (from item in list
																		from item2 in list 
																		from item3 in list 
																		select new Tuple<T, T, T>(item, item2, item3)).ToList();
		List<Tuple<T, T, T>> assocSet = triples.Where(
																			tuple => 
																				new List<T>(){function(tuple.Item1, function(tuple.Item2, tuple.Item3))}.EqualLists(
																					new List<T>(){function(function(tuple.Item1, tuple.Item2), tuple.Item3)})).Distinct().ToList();
		if (assocSet.Count != triples.Count) {
			return false;
		}

		//check identity
		var identity = GetIdentity();
		if (identity == null) {
			return false;
		}

		//check inverses
		bool allInverses = list.All(elt => GetInverse(elt) != null);
		if (!allInverses) {
			return false;
		}

		return true;
	}

	/*
	GetGeneratorSet()
	Returns a list of all elements in list that act as generators for the group, 
		i.e all elements x such that list == <x>
	*/
	public List<T> GetGeneratorSet () {
		return list.Where(elt => list.EqualLists(FromGenerator(elt))).Distinct().ToList();
	}

	/*
	FromGenerator()
	Returns a list of elements generated by the given element
	Helper for GetGeneratorSet(), but can also be a useful method on its own
	*/
	public List<T> FromGenerator (T generator) {
		T originalGenerator = generator;
		List<T> genList = new List<T>() {};

		//repeatedly apply function to the generator, until the generated list contains the original generator
		do {
			generator = function(generator, originalGenerator);
			genList.Add(generator);
		} while (!new List<T>(){generator}.EqualLists(new List<T>(){originalGenerator}));

		return genList;
	}

	/*
	FromGenerator()
	Returns a list of elements generated by the given element
	The list has a length of the int parameter order
	*/
	public List<T> FromGenerator (T generator, int order) {
		T originalGenerator = generator;
		List<T> groupList = new List<T>() {originalGenerator};

		//repeatedly apply funtion to the generator, up to the given order
		for (int i = 1; i < order; i++) {
			generator = function(generator, originalGenerator);
			groupList.Add(generator);
		}

		return groupList;
	}

	/*
	GetSubgroups()
	Returns a list of all the subgroups of this
	*/
	public List<Group<T>> GetSubgroups () {
		//List<List<T>> listChoices = (from elt in list select FromGenerator(elt)).Distinct().ToList().FilterSequence();
		List<List<T>> listChoices = list.PowerSet();
		List<Group<T>> groupChoices = (from lChoice in listChoices select new Group<T>(lChoice, expr)).Distinct().ToList();
		List<Group<T>> subgroups = groupChoices.Where(gChoice => gChoice.list != null).Distinct().ToList();
		ChangeNames(subgroups, "Subgroup");
		return subgroups;
	}

	/*
	GetCyclicSubgroups()
	Returns a list of all the cyclic subgroups of this
	*/
	public List<Group<T>> GetCyclicSubgroups () {
		List<List<T>> listChoices = (from elt in list select FromGenerator(elt)).Distinct().ToList().FilterSequence();
		List<Group<T>> groupChoices = (from lChoice in listChoices select new Group<T>(lChoice, expr)).Distinct().ToList();
		List<Group<T>> subgroups = groupChoices.Where(gChoice => gChoice.list != null).Distinct().ToList();
		ChangeNames(subgroups, "Cyclic subgroup");
		return subgroups;
	}

	/*
	GetNormalSubgroups()
	Returns a list of all the normal subgroups of this
	*/
	public List<Group<T>> GetNormalSubgroups () {
		List<Group<T>> normal = GetSubgroups().Where(subgroup => IsNormalSubgroup(subgroup)).Distinct().ToList();
		ChangeNames(normal, "Normal subgroup");
		return normal;
	}

	/*
	IsSubgroup()
	Returns true iff other is a subgroup of this
	Helper for IsNormalSubgroup()
	*/
	public bool IsSubgroup (Group<T> other) {
		HashSet<T> thisHash = new HashSet<T>(list);
		HashSet<T> otherHash = new HashSet<T>(other.List);

		return otherHash.IsProperSubsetOf(thisHash) && other.IsGroup();
	}

	/*
	IsNormalSubgroup()
	Returns true iff subgroup is a normal subgroup of this, i.e. the left and right cosets of subgroup are the same
	*/
	public bool IsNormalSubgroup (Group<T> subgroup) {
		//a normal subgroup has to be a subgroup
		if (!IsSubgroup(subgroup)) {
			return false;
		}

		//all subgroups of an abelian group are normal
		if (IsAbelian()) {
			return true;
		}

		//check for equality of left and right cosets
		return GetLeftCosets(subgroup).SequenceEqual(GetRightCosets(subgroup));
	}

	/*
	GetAllQuotients()
	Returns a list of the quotient groups with respect to each of the normal subgroups
	*/
	public List<Group<List<T>>> GetAllQuotients () {
		List<Group<List<T>>> quotients = GetNormalSubgroups().Select(normal => GetQuotientGroup(normal)).ToList();
		ChangeNames(quotients, "Quotient group");
		return quotients;
	}

	/*
	GetAllLeftCosets()
	Returns a list of the left cosets with respect to each of the normal subgroups
	*/
	public List<List<List<T>>> GetAllLeftCosets () {
		return GetNormalSubgroups().Select(normal => GetLeftCosets(normal)).ToList();
	}

	/*
	GetAllRightCosets()
	Returns a list of the right cosets with respect to each of the normal subgroups
	*/
	public List<List<List<T>>> GetAllRightCosets () {
		return GetNormalSubgroups().Select(normal => GetRightCosets(normal)).ToList();
	}

	/*
	GetQuotientGroup()
	Returns the quotient group of subgroup in this, where:
		the list of the quotient group is the set of left cosets of subgroup in this
		the operation of the quotient group works as follows:
			for two lists {elt0, elt1, ..., eltn} and {item0, item1, ..., itemn} in the set of left cosets,
			return the left coset {c0, c1, ..., cn} where c0 == function(elt0, item0)
	*/
	public Group<List<T>> GetQuotientGroup (Group<T> subgroup) {
		//make sure subgroup is a normal subgroup
		if (!IsNormalSubgroup(subgroup)) {
			return default(Group<List<T>>);
		}

		//get the left cosets of subgroup in this
		List<List<T>> leftCosets = GetLeftCosets(subgroup);

		//construct the expression (operation) for the quotient group
		Expression<Func<List<T>, List<T>, List<T>>> expression = (list1, list2) => 
																						leftCosets.Where(
																							coset => new List<T>(){coset[0]}.EqualLists(new List<T>(){function(list1[0], list2[0])})
																						).Distinct().ToList()[0];
		
		//return the quotient group constructed from the above
		return new Group<List<T>>(leftCosets, expression, "Quotient group for " + name + " in normal subgroup " + subgroup.Name);
	}

	/*
	GetLeftCosets()
	Returns a list of all the left cosets of subgroup in this, where
		each left coset is a list consisting of {gs | s in subgroup} for some element g in list
	*/
	public List<List<T>> GetLeftCosets (Group<T> subgroup) {
		//make sure subgroup is actually a subgroup
		if (!IsSubgroup(subgroup)) {
			return default(List<List<T>>);
		}

		return (from gItem in list select (from subItem in subgroup.List select function(gItem, subItem)).Distinct().ToList()).Distinct().ToList();
	}

	/*
	GetRightCosets()
	Returns a list of all the right cosets of subgroup in this, where
		each right coset is a list consisting of {sg | s in subgroup} for some element g in list
	*/
	public List<List<T>> GetRightCosets (Group<T> subgroup) {
		//make sure subgroup is actually a subgroup
		if (!IsSubgroup(subgroup)) {
			return default(List<List<T>>);
		}

		return (from gItem in list select (from subItem in subgroup.List select function(subItem, gItem)).Distinct().ToList()).Distinct().ToList();
	}

	/*
	IsAbelian()
	Returns true iff the group is abelian, i.e 
		for all (x, y) in list, function(x, y) == function(y, x)
	*/
	public bool IsAbelian () {
		List<Tuple<T, T>> pairs = (from item in list from item2 in list select new Tuple<T, T>(item, item2)).ToList();
		List<Tuple<T, T>> abelian = pairs.Where(
																	tuple => new List<T>(){function(tuple.Item1, tuple.Item2)}.EqualLists(
																													new List<T>(){function(tuple.Item2, tuple.Item1)})
																	).Distinct().ToList();
		return pairs.EqualLists(abelian);
	}

	/*
	GetCenter()
	Returns the center of the group, a new Group where the list is
		{x | for all g in list, function (x, g) == function (y, g)}
		and the expression is the same as expr
	*/
	public Group<T> GetCenter () {
		List<T> centerList = list.Where(elt => list.All(x => new List<T>(){function(elt, x)}.EqualLists(new List<T>(){function(x, elt)}))).Distinct().ToList();
		return new Group<T>(centerList, expr, "Center for " + name);
	}

	/*
	GetIdentity()
	Returns the identity element for the group, or null if none exists
	Different return types possible => return type is dynamic
	*/
	public dynamic GetIdentity () {
		//list of all possible identities
		List<T> identityList = list.Where(elt => list.All(x => new List<T>(){function(elt, x)}.EqualLists(new List<T>(){x}) && 
																											new List<T>(){function(x, elt)}.EqualLists(new List<T>(){x}))).Distinct().ToList();

		if (identityList.Count > 0) {
			return identityList[0];
		}

		//no identity
		return null;
	}

	/*
	GetAllInverses
	*/
	public Dictionary<T, T> GetAllInverses () {
		return list.Select(item => new Tuple<T, T>(item, (T)GetInverse(item))).ToDictionary(item => item.Item1, item => item.Item2);
	}

	/*
	GetInverse()
	Returns the inverse of the given element in the group, or null if either there is no identity or the given element has no inverse
	Different return types possible => return type is dynamic
	*/
	public dynamic GetInverse (T elt) {
		var identity = GetIdentity();

		//no identity => no inverse
		if (identity == null) {
			return null;
		}

		//list of possible inverse elements
		List<T> inverseList = list.Where(x => new List<T>(){function(elt, x)}.EqualLists(new List<T>(){identity}) && 
																			new List<T>(){function(x, elt)}.EqualLists(new List<T>(){identity})).ToList();

		//check for an inverse
		if (inverseList.Count > 0) {
			return inverseList[0];
		}

		//no inverse
		return null;
	}

	/*
	ChangeNames()
	Sets the name of every group in the given list to incorporate the given name
	Useful for changing the names of all subgroups, normal subgroups, and quotient groups
	The type parameter is A, not T since quotient groups have the type List<T>
	*/
	public void ChangeNames<A> (List<Group<A>> groupList, string nameAddition) {
		for (int i = 0; i < groupList.Count; i++) {
			groupList[i].Name = nameAddition + " " + i + " for " + name + ":";
		}
	}
}
