using CK.Core;

namespace CK.DB.SqlCKTrait;

/// <summary>
/// Holds all the words of all atomic created <see cref="CKTraitTable"/> items.
/// </summary>
[SqlTable( "tCKTraitWord", Package = typeof( Package ) )]
[Versions( "1.0.0" )]
public abstract partial class CKTraitWordTable : SqlTable
{
    void StObjConstruct()
    {
    }

}
