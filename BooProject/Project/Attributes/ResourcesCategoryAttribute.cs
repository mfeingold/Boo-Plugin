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