using CK.Core;

namespace CK.DB.SqlCKTrait
{
    /// <summary>
    /// Holds the atomic traits (actually words from the <see cref="CKTraitWordTable"/>)
    /// of a <see cref="CKTrait"/>.
    /// </summary>
    [SqlTable( "tCKTraitSet", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract partial class CKTraitSetTable : SqlTable
    {
        void StObjConstruct( CKTraitTable t, CKTraitWordTable words )
        {
        }

    }
}
