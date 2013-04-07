namespace PureToneAudiometer.ViewModels.Start
{
    using System;

    public class NavigationItem
	{
		public string Glyph { get; set; }
		public string NavigationName { get; set; }
		public Action NavigationAction { get; set; }
        public string Description { get; set; }
	}
}
