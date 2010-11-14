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
	/// Indicates the category to associate the associated property or event with, 
	/// when listing properties or events in a PropertyGrid control set to Categorized mode.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class ResourcesCategoryAttribute : CategoryAttribute
	{
		#region Constructors
		/// <summary>
		/// Explicit constructor.
		/// </summary>
		/// <param name="categoryName">
		/// Specifies the name of the category in which to group the property 
		/// or event when displayed in a PropertyGrid control set to Categorized mode.
		/// </param>
		public ResourcesCategoryAttribute(string categoryName)
			: base(categoryName)
		{
		}
		#endregion

		#region Overriden Implementation
		/// <summary>
		/// Looks up the localized name of the specified category.
		/// </summary>
		/// <returns>The localized name of the category, or a null reference
		/// if a localized name does not exist.</returns>
		protected override string GetLocalizedString(string categoryName)
		{
			return Resources.GetString(categoryName);
		}
		#endregion
	}
}