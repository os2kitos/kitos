using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Configuration;

namespace Infrastructure.DataAccess
{
    /// <summary>
    /// Stolen from http://stackoverflow.com/questions/18889218/unique-key-constraints-for-multiple-columns-in-entity-framework
    /// </summary>
    internal static class TypeConfigurationExtensions
    {
        /// <summary>
        /// Creates a non-clustered unique index
        /// </summary>
        /// <param name="property"></param>
        /// <param name="indexName">The index name.</param>
        /// <param name="columnOrder">A zero-based number which will be used to determine column ordering for multi-column indexes.</param>
        /// <returns></returns>
        public static PrimitivePropertyConfiguration HasUniqueIndexAnnotation(
            this PrimitivePropertyConfiguration property,
            string indexName,
            int columnOrder)
        {
            return property.HasIndexAnnotation(indexName, columnOrder, true);
        }

        /// <summary>
        /// Creates a non-clustered
        /// </summary>
        /// <param name="property"></param>
        /// <param name="indexName">The index name.</param>
        /// <param name="columnOrder">A zero-based number which will be used to determine column ordering for multi-column indexes.</param>
        /// <param name="unique">Determines if unique constraint should be applied</param>
        /// <returns></returns>
        public static PrimitivePropertyConfiguration HasIndexAnnotation(
            this PrimitivePropertyConfiguration property,
            string indexName,
            int columnOrder = 0,
            bool unique = false)
        {
            var indexAttribute = new IndexAttribute(indexName, columnOrder) { IsUnique = unique };
            var indexAnnotation = new IndexAnnotation(indexAttribute);

            return property.HasColumnAnnotation(IndexAnnotation.AnnotationName, indexAnnotation);
        }
    }
}
