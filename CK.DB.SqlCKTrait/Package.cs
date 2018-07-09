using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.SqlCKTrait
{
    /// <summary>
    /// Package that supports <see cref="CKTraitContextTable"/> and <see cref="CKTraitTable"/> sql mapping. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions("1.0.0")]
    public abstract class Package : SqlPackage
    {
        /// <summary>
        /// Gets the <see cref="CKTraitContextTable"/>.
        /// </summary>
        [InjectContract]
        public CKTraitContextTable CKTraitContextTable { get; private set; }

        /// <summary>
        /// Gets the <see cref="CKTraitTable"/>.
        /// </summary>
        [InjectContract]
        public CKTraitTable CKTraitTable { get; private set; }
    }
}
