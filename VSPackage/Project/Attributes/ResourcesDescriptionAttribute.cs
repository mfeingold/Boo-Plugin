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
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class ResourcesDescriptionAttribute : DescriptionAttribute
	{
		#region Fields
		private bool replaced;
		#endregion

		#region Constructors
		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="description">Attribute description.</param>
		public ResourcesDescriptionAttribute(string description)
			: base(description)
		{
		}
		#endregion

		#region Overriden Implementation
		/// <summary>
		/// Gets attribute description.
		/// </summary>
		public override string Description
		{
			get
			{
				if(!replaced)
				{
					replaced = true;
					DescriptionValue = Resources.GetString(base.Description);
				}

				return base.Description;
			}
		}
		#endregion
	}
}