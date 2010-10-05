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