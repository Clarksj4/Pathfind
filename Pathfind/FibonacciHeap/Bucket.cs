using System.Collections.Generic;

namespace FibonacciHeap
{
    /// <summary>
    /// Dynamically sized bucket
    /// </summary>
    public class Bucket<TItem>
    {
        /// <summary>
        /// Gets or sets the item at the given index
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <returns>The item at index</returns>
        public TItem this[int index]
        {
            get
            {
                // If no item at given index, create and fill with default value
                if (!bucket.ContainsKey(index))
                    bucket.Add(index, default(TItem));

                // Return the item at index
                return bucket[index];
            }

            set
            {
                // If no item at index, create entry at index storing given value
                if (!bucket.ContainsKey(index))
                    bucket.Add(index, value);

                // Overwrite item at index
                else
                    bucket[index] = value;
            }
        }

        private Dictionary<int, TItem> bucket;

        /// <summary>
        /// Empty bucket
        /// </summary>
        public Bucket()
        {
            bucket = new Dictionary<int, TItem>();
        }
    }
}
