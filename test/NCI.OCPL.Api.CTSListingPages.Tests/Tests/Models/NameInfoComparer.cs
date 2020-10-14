using System.Collections.Generic;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public class NameInfoComparer : IEqualityComparer<NameInfo>
    {
        public bool Equals(NameInfo x, NameInfo y)
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
                x.Label.Equals(y.Label)
                && x.Normalized.Equals(y.Normalized);

            return isEqual;
        }

        public int GetHashCode(NameInfo obj)
        {
            int hash = 0;

            hash ^=
                obj.Label.GetHashCode()
                ^obj.Normalized.GetHashCode();

            return hash;
        }
    }
}