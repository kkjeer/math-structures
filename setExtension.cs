/*
Katherine Kjeer
2015
setExtension.cs
Extension methods to support set operations (intersection, union, etc.) on Lists
*/

namespace SetExtension {

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using ListExtension;

	public static class SetExtension {
		//******SET OPERATIONS******//

		/*
		Intersection()
		Returns the intersection of the two lists
			{x | x in first && x in second}
		*/
		public static List<T> Intersection<T> (this List<T> first, List<T> second) {
			return first.Where(elt => second.ContainsSequence(elt)).Distinct().ToList();
		}

		/*
		Union()
		Returns the union of the two lists
			{x | x in first || x in second}
		*/
		public static List<T> Union<T> (this List<T> first, List<T> second) {
			return first.Where(elt => true).Union(second.Where(elt => true)).Distinct().ToList();
		}

		/*
		Complement()
		Returns the relative complement of the second list in the first: elements that are in first but not in second
			{x in first | x not in second}
		*/
		public static List<T> Complement<T> (this List<T> first, List<T> second) {
			return first.Where(elt => !second.ContainsSequence(elt)).Distinct().ToList();
		}

		/*
		Difference()
		Returns the symmetric difference of the two lists
			(first union second) - (first intersect second)
		*/
		public static List<T> Difference<T> (this List<T> first, List<T> second) {
			return first.Union(second).Complement(first.Intersection(second));
		}

		/*
		CartesianProduct()
		Returns the cartesian product of the two lists
		 {(x, y) | x in first && y in second}
		*/
		//cartesian product: first X second
		public static List<KeyValuePair<A, B>> CartesianProduct<A, B> (this List<A> first, List<B> second) {
			return (from elt1 in first from elt2 in second select new KeyValuePair<A, B>(elt1, elt2)).Distinct().ToList();
		}

		/*
		PowerSet()
		Returns the power set (set of all subsets) of the list
		*/
		public static List<List<T>> PowerSet<T> (this List<T> list) {
  		return (from m in Enumerable.Range(0, 1 << list.Count)
         				select
           				(from i in Enumerable.Range(0, list.Count)
           				where (m & (1 << i)) != 0
           				select list[i]).ToList()).OrderBy(item => item.Count).ToList();
		}

		//******END SET OPERATIONS******//

		//******FUNCTIONS ON SETS*******//

		/*
		Bijection()
		Returns a randomly generated one-to-one and onto function from the first list -> the second list
		If the two lists are of different sizes, no bijection exists, so a default function is returned
		*/
		public static Func<A, B> Bijection<A, B> (this List<A> first, List<B> second) {
			if (first.Count != second.Count) {
				Console.WriteLine("Bijection impossible. Unequal number of elements in first set (" + 
													first.Count + ") and second set (" + second.Count + "). Returning default function.");
				return x => default(B);
			}

			return first.RandomMap(second);
		}

		/*
		Injection()
		Returns a randomly generated one-to-one function from first -> second
		If the first list is larger than the second, no injection exists
		*/
		public static Func<A, B> Injection<A, B> (this List<A> first, List<B> second) {
			if (first.Count > second.Count) {
				Console.WriteLine("Injection impossible. More elements in first set (" + 
													first.Count + ") than in second set (" + second.Count + "). Returning default function.");
				return x => default(B);
			}

			return first.RandomMap(second);
		}

		/*
		Surjection()
		Returns a randomly generated onto function from first -> second
		If the second list is larger than the first, no surjection exists
		*/
		public static Func<A, B> Surjection<A, B> (this List<A> first, List<B> second) {
			if (second.Count > first.Count) {
				Console.WriteLine("Surjection impossible. Fewer elements in first set (" 
													+ first.Count + ") than in second set (" + second.Count + "). Returning default function");
				return x => default(B);
			}

			Dictionary<A, B> map = new Dictionary<A, B>();

			//make sure each value in second has some key in first that maps to it
			foreach (B value in second) {
				A key = first[new Random().Next(first.Count)];
				while (map.ContainsKey(key)) {
					key = first[new Random().Next(first.Count)];
				}
				map.Add(key, value);
			}

			//pick random values from second to map any unused keys in first to
			List<A> unusedKeys = first.Complement(map.Keys.ToList());
			foreach(A key in unusedKeys) {
				B value = second[new Random().Next(second.Count)];
				map.Add(key, value);
			}

			return x => map[x];
		}

		/*
		RandomMap()
		Returns a random function from first -> second
		Helper for Bijection() and Injection()
		Guaranteed to be an injection if the first list is not larger than the second
		Will be a bijection if the two lists have the same size
		*/
		public static Func<A, B> RandomMap<A, B> (this List<A> first, List<B> second) {
			Dictionary<A, B> map = new Dictionary<A, B>();
			foreach (A key in first) {
				B value = second[new Random().Next(second.Count)];
				while (map.ContainsValue(value)) {
					value = second[new Random().Next(second.Count)];
				}
				map.Add(key, value);
			}
			return x => map[x];
		}

		//******END FUNCTIONS ON SETS*******//

	} //end setExtension class
} //end setExtension namespace
