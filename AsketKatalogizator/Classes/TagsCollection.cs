using System.Collections.Generic;
using System.Linq;

namespace AsketKatalogizator {
    class TagsCollection : List<string> {

        public override string ToString() {
            return this.Aggregate(string.Empty, (seed, current) => seed += $"[{current}] ");
        }
    }
}