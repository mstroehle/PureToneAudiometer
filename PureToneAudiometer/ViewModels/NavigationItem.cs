namespace PureToneAudiometer.ViewModels
{
	using System;

	public class NavigationItem
	{
		public string Glyph { get; set; }
		public string NavigationName { get; set; }
		public Action NavigationAction { get; set; }
	}
}
