using CK.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.DB.SqlCKTrait
{

    /// <summary>
    /// Immutable encapsulation of a <see cref="CKTrait"/> and its database identifier.
    /// Multiple databases are not handled: this must be use in the context of
    /// the same database.
    /// Implements value semantic.
    /// </summary>
    public struct DBCKTrait
    {
        /// <summary>
        /// Gets the database mapped trait identifier.
        /// </summary>
        public int CKTraitId { get; }

        /// <summary>
        /// Gets the trait itself.
        /// </summary>
        public CKTrait Value { get; }

        /// <summary>
        /// Gets whether this is the mepty trait.
        /// </summary>
        public bool IsEmpty => CKTraitId == 0;

        /// <summary>
        /// Initializes a new <see cref="DBCKTrait"/>.
        /// </summary>
        /// <param name="id">The database mapped identifier.</param>
        /// <param name="t">The trait.</param>
        public DBCKTrait( int id, CKTrait t )
        {
            CKTraitId = id;
            Value = t;
        }

        /// <summary>
        /// Overridden to rely on <see cref="CKTrait"/> reference equality.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if obj is the same trait as this one.</returns>
        public override bool Equals( object obj ) => obj is CKTrait t ? ReferenceEquals( t, Value ) : false;

        /// <summary>
        /// Overridden to return the <see cref="CKTrait.ToString"/> full trait name.
        /// </summary>
        /// <returns>The full trait name.</returns>
        public override string ToString() => Value?.ToString() ?? String.Empty;

        /// <summary>
        /// Overridden to rely on <see cref="CKTrait"/> reference equality.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }
}
