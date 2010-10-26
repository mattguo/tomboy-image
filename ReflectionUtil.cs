using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Tomboy.InsertImage
{
	class ReflectionUtil
	{
		public static T GetFieldValue<T> (object obj, string fieldName, BindingFlags bindingFlags)
		{
			try {
				var type = obj.GetType ();
				FieldInfo fieldInfo = null;
				foreach (var field in type.GetFields (bindingFlags)) {
					if (field.Name == fieldName) {
						fieldInfo = field;
						break;
					}
				}
				
				return (T) fieldInfo.GetValue (obj);
			}
			catch {
				return default (T);
			}
		}
	}
}
