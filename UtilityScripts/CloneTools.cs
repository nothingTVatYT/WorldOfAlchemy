using System;
using System.Reflection;
using System.Linq;

public class CloneTools
{

	public static void CopyProperties<T1, T2> (T1 sourceObject, T2 targetObject)
			where T1: class
			where T2: class
	{
		PropertyInfo[] srcFields = sourceObject.GetType ().GetProperties (
			                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

		PropertyInfo[] destFields = targetObject.GetType ().GetProperties (
			                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

		foreach (var property in srcFields) {
			var dest = destFields.FirstOrDefault (x => x.Name == property.Name);
			if (dest != null && dest.CanWrite)
				dest.SetValue (targetObject, property.GetValue (sourceObject, null), null);
		}
	}
}

