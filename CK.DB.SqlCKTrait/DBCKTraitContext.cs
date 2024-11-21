using CK.Core;
using CK.SqlServer;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CK.DB.SqlCKTrait;

/// <summary>
/// Captures a <see cref="CKTraitContext"/> registered in a database.
/// This API has been designed to support <see cref="DBCKTrait"/> caching
/// if needed but this is curently not implemented.
/// </summary>
public class DBCKTraitContext
{
    internal DBCKTraitContext( Package p, CKTraitContext context, int contextId )
    {
        Package = p;
        Context = context;
        CKTraitContextId = contextId;
        EmptyTrait = new DBCKTrait( 0, context.EmptyTrait );
    }

    /// <summary>
    /// Gets the empty trait singleton for this <see cref="DBCKTraitContext"/>.
    /// </summary>
    public DBCKTrait EmptyTrait { get; }

    /// <summary>
    /// Gets the <see cref="CK.DB.SqlCKTrait.Package"/>.
    /// </summary>
    public Package Package { get; }

    /// <summary>
    /// Gets the <see cref="CKTraitContext"/>.
    /// </summary>
    public CKTraitContext Context { get; }

    /// <summary>
    /// Gets the database <see cref="Context"/> identifier.
    /// </summary>
    public int CKTraitContextId { get; }

    /// <summary>
    /// Gets or sets whether <see cref="DBCKTrait"/> must be cached.
    /// This is not yet implemented.
    /// Setting this to true initailizes an internal <see cref="ConcurrentDictionary{TKey, TValue}"/>
    /// of <see cref="CKTrait"/> to <see cref="DBCKTrait"/> and uses it to avoid database calls
    /// as much as possible.
    /// </summary>
    public bool EnableTraitCache
    {
        get => false;
        set => throw new NotImplementedException( "This has to be done." );
    }

    /// <summary>
    /// Maps the <paramref name="trait"/> (that must be in this <see cref="Context"/>).
    /// Its <see cref="CKTrait.AtomicTraits"/> are automatically registered.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to register.</param>
    /// <returns>The DBCKTrait.</returns>
    public DBCKTrait FindOrCreate( ISqlCallContext c, int actorId, CKTrait trait )
    {
        if( trait == null ) throw new ArgumentNullException( nameof( trait ) );
        if( trait.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        if( trait.IsEmpty ) return EmptyTrait;
        return new DBCKTrait( Package.CKTraitTable.FindOrCreate( c, actorId, CKTraitContextId, false, trait.ToString() ), trait );
    }

    /// <summary>
    /// Maps the <paramref name="trait"/> (that must be in this <see cref="Context"/>).
    /// Its <see cref="CKTrait.AtomicTraits"/> are automatically registered.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to register.</param>
    /// <returns>The DBCKTrait.</returns>
    public async Task<DBCKTrait> FindOrCreateAsync( ISqlCallContext c, int actorId, CKTrait trait )
    {
        if( trait == null ) throw new ArgumentNullException( nameof( trait ) );
        if( trait.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        return trait.IsEmpty
                ? EmptyTrait
                : new DBCKTrait( await Package.CKTraitTable.FindOrCreateAsync( c, actorId, CKTraitContextId, false, trait.ToString() ), trait );
    }

    /// <summary>
    /// Finds the mapping for <paramref name="trait"/> (that must be in this <see cref="Context"/>)
    /// or returns the <see cref="EmptyTrait"/> if the trait has not been mapped to the database.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to find.</param>
    /// <returns>The DBCKTrait or the EmptyTrait.</returns>
    public DBCKTrait FindOnly( ISqlCallContext c, int actorId, CKTrait trait )
    {
        if( trait == null ) throw new ArgumentNullException( nameof( trait ) );
        if( trait.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        if( trait.IsEmpty ) return EmptyTrait;
        int id = Package.CKTraitTable.FindOrCreate( c, actorId, CKTraitContextId, true, trait.ToString() );
        return id == 0
                ? EmptyTrait
                : new DBCKTrait( id, trait );
    }

    /// <summary>
    /// Finds the mapping for <paramref name="trait"/> (that must be in this <see cref="Context"/>)
    /// or returns the <see cref="EmptyTrait"/>.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to register.</param>
    /// <returns>The CKTrait identifier.</returns>
    public async Task<DBCKTrait> FindOnlyAsync( ISqlCallContext c, int actorId, CKTrait trait )
    {
        if( trait == null ) throw new ArgumentNullException( nameof( trait ) );
        if( trait.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        if( trait.IsEmpty ) return EmptyTrait;
        int id = await Package.CKTraitTable.FindOrCreateAsync( c, actorId, CKTraitContextId, true, trait.ToString() );
        return id == 0
                ? EmptyTrait
                : new DBCKTrait( id, trait );
    }

    /// <summary>
    /// Removes the <paramref name="trait"/> (that must be in this <see cref="Context"/>).
    /// from the database.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to remove.</param>
    public void RemoveByTraitName( ISqlCallContext c, int actorId, CKTrait trait )
    {
        if( trait == null ) throw new ArgumentNullException( nameof( trait ) );
        if( trait.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        if( !trait.IsEmpty ) Package.CKTraitTable.DestroyByTraitName( c, actorId, CKTraitContextId, trait.ToString() );
    }

    /// <summary>
    /// Removes the <paramref name="trait"/> (that must be in this <see cref="Context"/>).
    /// from the database.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to remove.</param>
    public Task RemoveByTraitNameAsync( ISqlCallContext c, int actorId, CKTrait trait )
    {
        if( trait == null ) throw new ArgumentNullException( nameof( trait ) );
        if( trait.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        return trait.IsEmpty
                ? Task.CompletedTask
                : Package.CKTraitTable.DestroyByTraitNameAsync( c, actorId, CKTraitContextId, trait.ToString() );
    }

    /// <summary>
    /// Removes the <paramref name="trait"/> mapping (that must be in this <see cref="Context"/>).
    /// from the database.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to remove.</param>
    public void Remove( ISqlCallContext c, int actorId, DBCKTrait trait )
    {
        if( trait.Value.Context != Context ) throw new ArgumentException( "Must be a non null ", nameof( trait ) );
        if( !trait.IsEmpty ) Package.CKTraitTable.Destroy( c, actorId, trait.CKTraitId );
    }

    /// <summary>
    /// Removes the <paramref name="trait"/> (that must be in this <see cref="Context"/>).
    /// from the database.
    /// </summary>
    /// <param name="c">The call context to use.</param>
    /// <param name="actorId">The current actor identifier.</param>
    /// <param name="trait">The trait to remove.</param>
    public Task RemoveAsync( ISqlCallContext c, int actorId, DBCKTrait trait )
    {
        if( trait.Value.Context != Context ) throw new ArgumentException( "Trait context mismatch.", nameof( trait ) );
        return trait.IsEmpty
                ? Task.CompletedTask
                : Package.CKTraitTable.DestroyAsync( c, actorId, trait.CKTraitId );
    }

}
