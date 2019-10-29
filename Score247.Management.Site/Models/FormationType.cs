using Score247Management.Core.Enumerations;

namespace Score247.Management.Site.Models
{
    public class FormationType : Enumeration
    {
        public static readonly FormationType F343 = new FormationType(1, "3-4-3");
        public static readonly FormationType F352 = new FormationType(2, "3-5-2");
        public static readonly FormationType F361 = new FormationType(3, "3-6-1");
        public static readonly FormationType F4141 = new FormationType(4, "4-1-4-1");
        public static readonly FormationType F4222 = new FormationType(5, "4-2-2-2");
        public static readonly FormationType F4312 = new FormationType(6, "4-3-1-2");
        public static readonly FormationType F433 = new FormationType(7, "4-3-3");
        public static readonly FormationType F451 = new FormationType(8, "4-5-1");
        public static readonly FormationType F460 = new FormationType(9, "4-6-0");

        public FormationType()
        {
        }

        public FormationType(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public FormationType(byte value)
            : base(value, value.ToString())
        {
        }
    }
}