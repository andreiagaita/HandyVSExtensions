using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoiledCat.Utils.Collections
{
	/// <summary>
	/// An object that wants to expose a property as its key,
	/// for use with the MutantHashSet
	/// </summary>
	/// <typeparam name="KeyType">The type of the key</typeparam>
	public interface IKeyedObject<KeyType>
	{
		KeyType Key { get; }
	}
}
