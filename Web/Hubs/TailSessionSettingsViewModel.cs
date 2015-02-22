namespace LogtailR.Web.Hubs
{
    public class TailSessionSettingsViewModel
    {
        public string IncludeRx { get; set; }
        public string ExcludeRx { get; set; }

        public string RedColorRx { get; set; }
        public string WhiteColorRx { get; set; }
        public string YellowColorRx { get; set; }
    }
}