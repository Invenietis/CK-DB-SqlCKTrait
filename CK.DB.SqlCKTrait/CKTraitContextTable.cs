using CK.Core;
using CK.Setup;
using CK.SqlServer;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CK.DB.SqlCKTrait;

/// <summary>
/// CKTraitContext registration table.
/// </summary>
[SqlTable( "tCKTraitContextTable", Package = typeof( Package ) )]
[Versions( "1.0.0" )]
public abstract partial class CKTraitContextTable : SqlTable
{
    readonly ConcurrentDictionary<string, DBCKTraitContext> _cache;
    Package _package;

    void StObjConstruct( Package p )
    {
        _package = p;
    }

    /// <summary>
    /// Initializes a new CKTraitContextTable.
    /// Internal cache of <see cref="DBCKTraitContext"/> is created.
    /// </summary>
    public CKTraitContextTable()
    {
        _cache = new ConcurrentDictionary<string, DBCKTraitContext>();
    }

    /// <summary>
    /// Registers a <see cref="CKTraitContext"/>. Finds or it creates it and handle separator
    /// change as needed.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="traitContext">The context to register.</param>
    /// <returns>The mapped context.</returns>
    public DBCKTraitContext RegisterContext( ISqlCallContext c, int actorId, CKTraitContext traitContext )
    {
        if( traitContext == null ) throw new ArgumentNullException( nameof( traitContext ) );
        var db = _cache.GetOrAdd( traitContext.Name, name =>
        {
            return new DBCKTraitContext( _package, traitContext, RegisterContext( c, actorId, name, traitContext.Separator ) );
        } );
        return db;
    }

    /// <summary>
    /// Registers a <see cref="CKTraitContext"/>. Finds or it creates it and handle separator
    /// change as needed.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="traitContext">The context to register.</param>
    /// <returns>The mapped context.</returns>
    public async Task<DBCKTraitContext> RegisterContextAsync( ISqlCallContext c, int actorId, CKTraitContext traitContext )
    {
        if( !_cache.TryGetValue( traitContext.Name, out var db ) )
        {
            int id = await RegisterContextAsync( c, actorId, traitContext.Name, traitContext.Separator );
            db = new DBCKTraitContext( _package, traitContext, id );
            _cache.TryAdd( traitContext.Name, db );
        }
        return db;
    }

    /// <summary>
    /// Actual call to sCKTraitContextRegister procedure.
    /// This should rarely be used directly: <see cref="RegisterContext(ISqlCallContext, int, CKTraitContext)"/>
    /// with an actual context is the preferred way.
    /// </summary>
    /// <param name="c">The call context.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="contextName">The context name. Can not be null or empty.</param>
    /// <param name="separator">The context's traits separator.</param>
    /// <returns>The context identifier.</returns>
    [SqlProcedure( "sCKTraitContextRegister" )]
    public abstract int RegisterContext( ISqlCallContext c, int actorId, string contextName, char separator );

    /// <summary>
    /// Actual call to sCKTraitContextRegister procedure.
    /// This should rarely be used directly: <see cref="RegisterContext(ISqlCallContext, int, CKTraitContext)"/>
    /// with an actual context is the preferred way.
    /// </summary>
    /// <param name="c">The call context.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="contextName">The context name. Can not be null or empty.</param>
    /// <param name="separator">The context's traits separator.</param>
    /// <returns>The context identifier.</returns>
    [SqlProcedure( "sCKTraitContextRegister" )]
    public abstract Task<int> RegisterContextAsync( ISqlCallContext c, int actorId, string contextName, char separator );

    /// <summary>
    /// Calls CK.sCKTraitContextSeparatorSet.
    /// This updates the context separator.
    /// An error is raised if any atomic trait used by the context contains the new seprator.
    /// On success, all composite TraitNames for this <paramref name="ckTraitContextId"/> are updated.
    /// This should rarely be used directly: <see cref="RegisterContext(ISqlCallContext, int, CKTraitContext)"/>
    /// with an actual context is the preferred way.
    /// </summary>
    /// <param name="c">The sql call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="ckTraitContextId">The context identifier.</param>
    /// <param name="newSeparator">The new separator to set.</param>
    [SqlProcedure( "sCKTraitContextSeparatorSet" )]
    public abstract void SetSeparator( ISqlCallContext c, int actorId, int ckTraitContextId, char newSeparator );

    /// <summary>
    /// Calls CK.sCKTraitContextSeparatorSet.
    /// This updates the context separator.
    /// An error is raised if any atomic trait used by the context contains the new seprator.
    /// On success, all composite TraitNames for this <paramref name="ckTraitContextId"/> are updated.
    /// This should rarely be used directly: <see cref="RegisterContext(ISqlCallContext, int, CKTraitContext)"/>
    /// with an actual context is the preferred way.
    /// </summary>
    /// <param name="c">The sql call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="ckTraitContextId">The context identifier.</param>
    /// <param name="newSeparator">The new separator to set.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sCKTraitContextSeparatorSet" )]
    public abstract Task SetSeparatorAsync( ISqlCallContext c, int actorId, int ckTraitContextId, char newSeparator );

}
