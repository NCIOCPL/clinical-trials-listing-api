using System.Linq;
using System.Collections.Generic;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// An IEqualityComparer for LabelInformation objects.
    /// </summary>
    public class LabelInformationComparer : IEqualityComparer<LabelInformation>
    {
        public bool Equals(LabelInformation x, LabelInformation y)
        {
            // If the items are both null, or if one or the other is null, return
            // the correct response right away.
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null || y == null)
            {
                return false;
            }

            bool isEqual =
                x.PrettyUrlName.Equals(y.PrettyUrlName)
                && x.IdString.Equals(y.IdString)
                && x.Label.Equals(y.Label);

            return isEqual;
        }

        public int GetHashCode(LabelInformation obj)
        {
            int hash = 0;

            hash ^=
                obj.PrettyUrlName.GetHashCode()
                ^ obj.IdString.GetHashCode()
                ^ obj.Label.GetHashCode();

            return hash;
        }
    }
}