using System.Linq;
using System.Collections.Generic;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// An IEqualityComparer for OverrideRecord objects.
    /// </summary>
    public class OverrideRecordComparer : IEqualityComparer<OverrideRecord>
    {
        public bool Equals(OverrideRecord x, OverrideRecord y)
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
                x.Type.Equals(y.Type)
                && x.ConceptId.SequenceEqual(y.ConceptId)
                && x.PrettyUrlName.Equals(y.PrettyUrlName)
                && x.UniqueId.Equals(y.UniqueId)
                && new NameInfoComparer().Equals(x.Name, y.Name);

            return isEqual;
        }

        public int GetHashCode(OverrideRecord obj)
        {
            int hash = 0;

            hash ^=
                obj.Type.GetHashCode()
                ^obj.ConceptId.GetHashCode()
                ^obj.PrettyUrlName.GetHashCode()
                ^obj.UniqueId.GetHashCode()
                ^(new NameInfoComparer()).GetHashCode(obj.Name);

            return hash;
        }
    }
}