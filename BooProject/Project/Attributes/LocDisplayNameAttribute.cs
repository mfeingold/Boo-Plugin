//
//   Copyright © 2010 Michael Feingold
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.ComponentModel;

namespace Hill30.BooProject.Project.Attributes
{
	/// <summary>
	/// Specifies the display name for a property, event, 
	/// or public void method which takes no arguments.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class LocDisplayNameAttribute : DisplayNameAttribute
	{
		#region Fields
		private readonly string name;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="name">Attribute display name.</param>
		public LocDisplayNameAttribute(string name)
		{
			this.name = name;
		}
		#endregion

		#region Overriden Implementation
		/// <summary>
		/// Gets attribute display name.
		/// </summary>
		public override string DisplayName
		{
			get
			{
				return Resources.GetString(name) ?? name;
			}
		}
		#endregion
	}
}