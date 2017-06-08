using System;
using System.Collections;

namespace AoLib
{
	/// <summary>
	/// Summary description for StringCollection.
	/// </summary>
	public class StringArray : CollectionBase
	{
		public String this[int index]
		{
			get { return (String)this.List[index];	}
			set { this.List[index] = (String)value; }
		}

		internal StringArray() {}

		public Int32 Add(String value)
		{
			return List.Add(value);
		}
		public Int32 IndexOf(String value)
		{
			return List.IndexOf(value);
		}
		public void Insert(Int32 index, String value)
		{
			List.Insert(index, value);
		}
		public void Remove(String value)
		{
			List.Remove(value);
		}

		public bool Contains(String value)
		{
			return List.Contains(value);
		}
		protected override void OnValidate(object value)
		{
			if (value.GetType() != Type.GetType("System.String"))
			{
				throw new ArgumentException("value must be of type String, or must derive from String.", "value");
			}
		}
	}
}
