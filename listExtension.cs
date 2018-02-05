/*
Katherine Kjeer
2015
listExtension.cs
Extension methods for string manipulation and equality/containment checking
	of Lists and other enumerable data structures
*/

namespace ListExtension {

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	public static class ListExtension {
		//******STRING REPRESENTATIONS******//

		/*
		StringRep()
		Returns a string representation of the list
		*/
		public static string StringRep<T> (this List<T> list) {
			return "{" + String.Join(", ", list.Select(item => item.ElementString()).ToArray().ToList()) + "}";
		}

		/*
		Returns an alternative string representation of a list,
			where each list element is on a separate line
		*/
		public static string LineString<T> (this List<T> list) {
			return "{\n\t" + String.Join(",\n\t", list.Select(item => item.ElementString()).ToArray().ToList()) + "\n}";
		}

		/*
		ElementString()
		Returns a string representation of the given element
		Calls StringRep() or element.ToString()
			based on whether the element is a list or not
		*/
		public static string ElementString<T> (this T element) {
			//get the type and list type (to determine whether element is a list)
			Type eltType = element.GetType();
      Type listType = eltType.GetListType();

      //element is a list => use StringRep()
      if (listType != null) {
      	return (string)typeof(ListExtension).GetMethod("StringRep").MakeGenericMethod(listType).Invoke(null,
      		new object[] {element}); 	
      }

      bool isDict = eltType.IsGenericType && eltType.GetGenericTypeDefinition() == typeof(Dictionary<,>);

			//elt is a dictionary => use DictString()
			if (isDict) {
				Type keyType = eltType.GetGenericArguments()[0];
				Type valueType = eltType.GetGenericArguments()[1];
				return (string)typeof(ListExtension).GetMethod("DictString").MakeGenericMethod(keyType, valueType).Invoke(null, new object[] {element});
			}

      //element is not a list => use element's built-in ToString()
      return element.ToString();
		}

		/*
		DictString()
		Returns a string representation of the given dictionary,
			where each pairing item => dict[item] is on a separate line
		*/
		public static string DictString<A, B> (this Dictionary<A, B> dict) {
			List<string> pairings = dict.Keys.Select(
																item => item.ElementString() + " => " + dict[item].ElementString()
															).ToList();
			return pairings.LineString();
		}

		//******END STRING REPRESENTATIONS******//

    //******LIST COMPARISONS******//

		/*
		EqualLists()
		Returns true iff the two lists contain the same elements, in any order
		Useful for list comparision in Group
		*/
		public static bool EqualLists<T> (this List<T> list1, List<T> list2) {
			//list1 or list2 is null
			if (list1 == null || list2 == null) {
				return list1 == list2;
			}

			//get the lists containing the ids of all the elements in each list and sort them
			List<string> idList1 = list1.Select(item => item.GetId()).OrderBy(s => s).ToList();
			List<string> idList2 = list2.Select(item => item.GetId()).OrderBy(s => s).ToList();

			//compare the sorted lists of ids
			return Enumerable.SequenceEqual(idList1, idList2);
		}

		/*
		EqualDicts()
		Returns true iff the two dictionaries map the same keys to the same values
		Helper for ContainsSequence()
		*/
		public static bool EqualDicts<A, B> (this Dictionary<A, B> dict1, Dictionary<A, B> dict2) {
			//dict1 or dict2 is null
			if (dict1 == null || dict2 == null) {
				return dict1 == dict2;
			}

			//get the lists containing the ids of all the elements in each list and sort them
			List<string> idList1 = dict1.Select(item => item.GetId()).OrderBy(s => s).ToList();
			List<string> idList2 = dict2.Select(item => item.GetId()).OrderBy(s => s).ToList();

			//compare the sorted lists of ids
			return Enumerable.SequenceEqual(idList1, idList2);
		}

		/*
		ContainsSequence()
		Returns true iff the given list contains the given elt
		Intended as a "replacement" for List.Contains() or Enumerable.SequenceEqual(), 
			since {{1, 2, 3}, {4, 5, 6}}.Contains({1, 2, 3}) returns false, i.e
			List.Contains() does not work well for nested lists
		*/
		public static bool ContainsSequence<T> (this List<T> list, T elt) {
			Type eltType = elt.GetType();
			Type eltListType = eltType.GetListType();

			//elt is a list => use EqualLists() (EqualLists() handles lists that are nested arbitrarily deeply)
			if (eltListType != null) {
				return list.Where(item => (bool)typeof(ListExtension).GetMethod("EqualLists").MakeGenericMethod(eltListType).Invoke(null, new object[] {item, elt})).ToList().Count > 0;
			}

			bool isDict = eltType.IsGenericType && eltType.GetGenericTypeDefinition() == typeof(Dictionary<,>);

			//elt is a dictionary => use EqualDicts()
			if (isDict) {
				Type keyType = eltType.GetGenericArguments()[0];
				Type valueType = eltType.GetGenericArguments()[1];
				return list.Where(item => (bool)typeof(ListExtension).GetMethod("EqualDicts").MakeGenericMethod(keyType, valueType).Invoke(null, new object[] {item, elt})).ToList().Count > 0;
			}

			//elt is not a list => use the built-in List.Contains()
			return list.Contains(elt);
		}

		/*
		FilterSequence()
		Removes all lists from a list of lists where another list in the list contains the same elements,
			but in a different order
		Used to remove duplicate lists in Group.GetSubgroups()
		Essentially treats two lists that contain the same elements in any order as duplicates
		*/
		public static List<T> FilterSequence<T> (this List<T> list) {
			List<Tuple<string, T>> tuples = list.Select(item => new Tuple<string, T>(item.GetId(), item)).ToList();
			List<T> distinctValues =
		    (from tuple in tuples
		    group tuple by tuple.Item1
		    into idTuples
		    select idTuples.First().Item2).ToList();
		  return distinctValues;
		}

		/*
		GetId()
		Returns an alphabetized string representation of the given element
		Works for non-list types and arbitarily nested lists (lists, lists of lists, lists of lists of lists, ...)
			e.g. if the elt is {"b", "a", "c"}, the id will be "abc"
		Helper for EqualLists() and FilterSequence()
		*/
		public static string GetId<T> (this T elt) {
			if (elt == null) {
				return "";
			}

			//get types
			Type type = elt.GetType();
			Type listType = type.GetListType();

			//elt is a list => use ListId()
			if (listType != null) { 
				string id = (string)typeof(ListExtension).GetMethod("ListId").MakeGenericMethod(listType).Invoke(null, new object [] {elt});
				return id.SortedString();
			}

			//elt is a dictionary => use DictId() (do not sort the resulting id since the key-value pairs must be kept distinct)
			bool isDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
			if (isDict) {
				Type keyType = type.GetGenericArguments()[0];
				Type valueType = type.GetGenericArguments()[1];
				string id = (string)typeof(ListExtension).GetMethod("DictId").MakeGenericMethod(keyType, valueType).Invoke(null, new object [] {elt});
				return id;
			}

			//elt is not a list => use elt's built-in ToString (sorted)
			return elt.ToString().SortedString();
		}

		/*
		ListId()
		Returns the id for a list of any kind of element (a non-list, another list, a lists of lists, etc.)
		Helper for GetId()
		*/
		public static string ListId<T> (this List<T> list) {
			if (list == null) {
				return "";
			}

			string result = "";
			for (int i = 0; i < list.Count; i++) {
        result += list[i].GetId();
			}
			result = result.SortedString();
			return result;
		}

		/*
		DictId()
		Returns the id for a dictionary of any kind of elements
		Helper for GetId()
		*/
		public static string DictId<A, B> (this Dictionary<A, B> dict) {
			if (dict == null) {
				return "";
			}

			string result = "";
			foreach (A key in dict.Keys) {
        result += key.GetId() + " => " + dict[key].GetId();
			}
			return result;
		}

		/*
		SortedString()
		Returns the alphabetized version of the input string
		Helper for GetId() and ListId()
		*/
		public static string SortedString (this string str) {
			char [] ch = str.ToCharArray();
			Array.Sort(ch);
			return new string(ch);
		}

		//******END LIST COMPARISONS******//

		//******GENERAL HELPER FUNCTIONS******//

		/*
		GetListType()
		Returns the list type of the given type
			e.g. if the given type is List<int>, then int will be returned
			returns null if the given type is not a list
		Helper for pretty much every other listExtension method
		Courtesy of StackOverflow
		*/
		public static Type GetListType (this Type type) {
      foreach (Type intType in type.GetInterfaces()) {
          if (intType.IsGenericType
              && intType.GetGenericTypeDefinition() == typeof(IList<>)) {
              return intType.GetGenericArguments()[0];
          }
      }
      return null;
    }

    //******END GENERAL HELPER FUNCTIONS******//

	} //end ListExtension class
} //end ListExtension namespace
