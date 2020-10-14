using System.Linq;
using System.Collections.Generic;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// An IEqualityComparer for ListingInfo objects.
    /// </summary>
    public class ListingInformationComparer : IEqualityComparer<ListingInfo>
    {
        public bool Equals(ListingInfo x, ListingInfo y)
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
                x.ConceptId.SequenceEqual(y.ConceptId)
                && x.PrettyUrlName.Equals(y.PrettyUrlName)
                && x.UniqueId.Equals(y.UniqueId)
                && new NameInfoComparer().Equals(x.Name, y.Name)
                && x.ExactMatch.Equals(y.ExactMatch);

            return isEqual;
        }

        public int GetHashCode(ListingInfo obj)
        {
            int hash = 0;

            hash ^=
                obj.ConceptId.GetHashCode()
                ^obj.PrettyUrlName.GetHashCode()
                ^obj.UniqueId.GetHashCode()
                ^(new NameInfoComparer()).GetHashCode(obj.Name)
                ^obj.ExactMatch.GetHashCode();

            return hash;
        }
    }
}