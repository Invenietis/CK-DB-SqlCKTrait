using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System.Threading.Tasks;

namespace CK.DB.SqlCKTrait
{
    /// <summary>
    /// Holds the persisted <see cref="CKTrait"/>.
    /// </summary>
    [SqlTable( "tCKTrait", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "fCKTraitProperSubSet" )]
    [SqlObjectItem( "fCKTraitProperSuperSet" )]
    public abstract partial class CKTraitTable : SqlTable
    {
        void StObjConstruct( CKTraitContextTable c, CKTraitWordTable words )
        {
        }

        /// <summary>
        /// Finds or creates (optionaly only finds) a trait by it full name.
        /// Traits are separated by the configured separator of the provided <paramref name="cKTraitContextId"/>.
        /// The traits are normalized (trimmed, sorted, deduplicated): this reproduces 
        /// the C# implementation of the CKTrait.
        /// </summary>
        /// <param name="c">The sql call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="cKTraitContextId">The context idetifier previously registered.</param>
        /// <param name="findOnly">True to only find the trait.</param>
        /// <param name="traitName">The full name of the trait.</param>
        /// <returns>
        /// The trait identifier or 0 if not found (<paramref name="findOnly"/> was true)
        /// or if the trait was empty.
        /// </returns>
        [SqlProcedure( "sCKTraitFindOrCreate" )]
        public abstract int FindOrCreate( ISqlCallContext c, int actorId, int cKTraitContextId, bool findOnly, string traitName );

        /// <summary>
        /// Finds or creates (optionaly only finds) a trait by it full name.
        /// Traits are separated by the configured separator of the provided <paramref name="cKTraitContextId"/>.
        /// The traits are normalized (trimmed, sorted, deduplicated): this reproduces 
        /// the C# implementation of the CKTrait.
        /// </summary>
        /// <param name="c">The sql call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="cKTraitContextId">The context idetifier previously registered.</param>
        /// <param name="findOnly">True to only find the trait.</param>
        /// <param name="traitName">The full name of the trait.</param>
        /// <returns>
        /// The trait identifier or 0 if not found (<paramref name="findOnly"/> was true)
        /// or if the trait was empty.
        /// </returns>
        [SqlProcedure( "sCKTraitFindOrCreate" )]
        public abstract Task<int> FindOrCreateAsync( ISqlCallContext c, int actorId, int cKTraitContextId, bool findOnly, string traitName );

        /// <summary>
        /// Destroys a trait by its identifier (does nothing if the trait does not exist). 
        /// If the trait is atomic and its value is currently used in a composite 
        /// trait of the same context an error is raised.
        /// To remove an atomic trait, all composite traits that use it must 
        /// already be destroyed.This is because traits are "values". 
        /// Removing the atomic value would create different traits that 
        /// share the same value.
        /// </summary>
        /// <param name="c">The sql call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="ckTraitId">The trait identifier to destroy.</param>
        [SqlProcedure( "sCKTraitDestroy" )]
        public abstract void Destroy( ISqlCallContext c, int actorId, int ckTraitId );

        /// <summary>
        /// Destroys a trait by its identifier (does nothing if the trait does not exist). 
        /// If the trait is atomic and its value is currently used in a composite 
        /// trait of the same context an error is raised.
        /// To remove an atomic trait, all composite traits that use it must 
        /// already be destroyed.This is because traits are "values". 
        /// Removing the atomic value would create different traits that 
        /// share the same value.
        /// </summary>
        /// <param name="c">The sql call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="ckTraitId">The trait identifier to destroy.</param>
        [SqlProcedure( "sCKTraitDestroy" )]
        public abstract Task DestroyAsync( ISqlCallContext c, int actorId, int ckTraitId );

        /// <summary>
        /// Removes a trait by its full name from a context.
        /// The same normalization as in <see cref="FindOrCreate"/> applies and the same
        /// limitation regarding destroying atomic traits as in <see cref="Destroy"/> also applies.
        /// </summary>
        /// <param name="c">The sql call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="ckTraitContextId">The context identifier.</param>
        /// <param name="traitName">The full trait name.</param>
        [SqlProcedure( "sCKTraitDestroyByTraitName" )]
        public abstract void DestroyByTraitName( ISqlCallContext c, int actorId, int ckTraitContextId, string traitName );

        /// <summary>
        /// Removes a trait by its full name from a context.
        /// The same normalization as in <see cref="FindOrCreate"/> applies and the same
        /// limitation regarding destroying atomic traits as in <see cref="Destroy"/> also applies.
        /// </summary>
        /// <param name="c">The sql call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="ckTraitContextId">The context identifier.</param>
        /// <param name="traitName">The full trait name.</param>
        [SqlProcedure( "sCKTraitDestroyByTraitName" )]
        public abstract Task DestroyByTraitNameAsync( ISqlCallContext c, int actorId, int ckTraitContextId, string traitName );

    }
}
