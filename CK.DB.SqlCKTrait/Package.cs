using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.SqlCKTrait
{
    /// <summary>
    /// Package that supports <see cref="CKTraitContext"/> and <see cref="CKTrait"/> sql mapping. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions("1.0.0")]
    public abstract class Package : SqlPackage
    {
        [InjectContract]
        public CKTraitContextTable CKTraitContextTable { get; private set; }

        [InjectContract]
        public CKTraitTable CKTraitTable { get; private set; }
    }
}
