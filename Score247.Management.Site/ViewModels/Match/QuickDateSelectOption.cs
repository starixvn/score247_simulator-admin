using System;

namespace Score247.Management.Site.ViewModels.Match
{
    public class QuickDateSelectOption
    {
        public string DisplayName { get; set; }
        public string Value { get; set; }

        public QuickDateSelectOption()
        {
        }

        public QuickDateSelectOption(DateTime time)
        {
            Value = time.ToString("MM/dd/yyyy");
            DisplayName = time.ToString("ddd, MMMM dd").ToUpper();
        }
    }
}