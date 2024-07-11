using CK.Core;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.SqlCKTrait.Tests
{
    [TestFixture]
    public class DBCKTraitTests
    {
        static readonly CKTraitContext Context1 = CKTraitContext.Create( "Context1", ',' );
        static readonly CKTraitContext Context2 = CKTraitContext.Create( "Context2", ',' );

        static readonly CKTrait t1MongoDbSqlServerNetCoreApp20 = Context1.FindOrCreate( "MongoDb, SqlServer, NetCoreApp20" );
        static readonly CKTrait t1MongoDbNetCoreApp20 = Context1.FindOrCreate( "MongoDb, NetCoreApp20" );
        static readonly CKTrait t1MongoDb = Context1.FindOrCreate( "MongoDb" );
        static readonly CKTrait t1SqlServer = Context1.FindOrCreate( "SqlServer" );
        static readonly CKTrait t1NetCoreApp20 = Context1.FindOrCreate( "NetCoreApp20" );
        static readonly CKTrait t1NetFramework = Context1.FindOrCreate( "NetFramework" );

        static readonly CKTrait t2MongoDbSqlServerNetCoreApp20 = Context1.FindOrCreate( "MongoDb, SqlServer, NetCoreApp20" );
        static readonly CKTrait t2MongoDbNetCoreApp20 = Context1.FindOrCreate( "MongoDb, NetCoreApp20" );
        static readonly CKTrait t2MongoDb = Context2.FindOrCreate( "MongoDb" );
        static readonly CKTrait t2SqlServer = Context2.FindOrCreate( "SqlServer" );
        static readonly CKTrait t2NetCoreApp20 = Context2.FindOrCreate( "NetCoreApp20" );
        static readonly CKTrait t2NetFramework = Context2.FindOrCreate( "NetFramework" );

        [Test]
        public async Task creating_and_removing_traits_mapping_Async()
        {
            var p = SharedEngine.Map.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var dbC = await p.CKTraitContextTable.RegisterContextAsync( ctx, 1, Context1 );

                var empty = await dbC.FindOrCreateAsync( ctx, 1, Context1.EmptyTrait );
                empty.CKTraitId.Should().Be( 0 );
                empty.Value.Should().Be( Context1.EmptyTrait );

                DBCKTrait tDBMongoDbSqlServerNetCoreApp20 = await dbC.FindOrCreateAsync( ctx, 1, t1MongoDbSqlServerNetCoreApp20 );
                tDBMongoDbSqlServerNetCoreApp20.CKTraitId.Should().BeGreaterThan( 0 );

                DBCKTrait tDBMongoDb = await dbC.FindOnlyAsync( ctx, 1, t1MongoDb );
                tDBMongoDb.Value.Should().BeSameAs( t1MongoDb );

                DBCKTrait tDBSqlServer = await dbC.FindOnlyAsync( ctx, 1, t1SqlServer );
                tDBSqlServer.Value.Should().BeSameAs( t1SqlServer );

                DBCKTrait tDBNetCoreApp20 = await dbC.FindOnlyAsync( ctx, 1, t1NetCoreApp20 );
                tDBNetCoreApp20.Value.Should().BeSameAs( t1NetCoreApp20 );

                DBCKTrait tDBNotFound = await dbC.FindOnlyAsync( ctx, 1, t1MongoDbNetCoreApp20 );
                tDBNotFound.IsEmpty.Should().BeTrue();
                tDBNotFound.Value.Should().BeSameAs( Context1.EmptyTrait );

                DBCKTrait tDBMongoDbNetCoreApp20 = await dbC.FindOrCreateAsync( ctx, 1, t1MongoDbNetCoreApp20 );
                tDBMongoDbNetCoreApp20.Value.Should().BeSameAs( t1MongoDbNetCoreApp20 );

                // Removes the "MongoDb, SqlServer, NetCoreApp20": "SqlServer" atomic trait is no more used.
                await dbC.RemoveAsync( ctx, 1, tDBMongoDbSqlServerNetCoreApp20 );
                await dbC.RemoveAsync( ctx, 1, tDBSqlServer );

                await dbC.Awaiting( c => c.RemoveAsync( ctx, 1, tDBMongoDb ) ).Should().ThrowAsync<SqlDetailedException>();
                await dbC.Awaiting( c => c.RemoveAsync( ctx, 1, tDBNetCoreApp20 ) ).Should().ThrowAsync<SqlDetailedException>();

                await dbC.RemoveAsync( ctx, 1, tDBMongoDbNetCoreApp20 );

                await dbC.RemoveAsync( ctx, 1, tDBMongoDb );
                await dbC.RemoveAsync( ctx, 1, tDBNetCoreApp20 );

                // No error if they do not exist.
                await dbC.RemoveAsync( ctx, 1, tDBSqlServer );
                await dbC.RemoveAsync( ctx, 1, tDBMongoDb );
                await dbC.RemoveAsync( ctx, 1, tDBNetCoreApp20 );
                await dbC.RemoveAsync( ctx, 1, tDBMongoDbNetCoreApp20 );
            }
        }

        [Test]
        public async Task trait_context_is_checked_to_avoid_mismatch_Async()
        {
            var p = SharedEngine.Map.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var dbC1 = await p.CKTraitContextTable.RegisterContextAsync( ctx, 1, Context1 );
                var dbC2 = await p.CKTraitContextTable.RegisterContextAsync( ctx, 1, Context2 );
                dbC1.CKTraitContextId.Should().NotBe( dbC2.CKTraitContextId );

                var tIn1 = await dbC1.FindOrCreateAsync( ctx, 1, t1MongoDb );
                var tIn2 = await dbC2.FindOrCreateAsync( ctx, 1, t2MongoDb );

                tIn1.Value.Should().Be( t1MongoDb );
                tIn2.Value.Should().Be( t2MongoDb );
                tIn1.CKTraitId.Should().NotBe( 0 );
                tIn2.CKTraitId.Should().NotBe( 0 );
                tIn1.CKTraitId.Should().NotBe( tIn2.CKTraitId );

                await dbC1.Awaiting( c => c.FindOrCreateAsync( ctx, 1, t2MongoDb ) )
                    .Should().ThrowAsync<ArgumentException>()
                    .Where( e => e.Message.StartsWith( "Trait context mismatch." ) );

                await dbC2.Awaiting( c => c.FindOrCreateAsync( ctx, 1, t1MongoDb ) )
                    .Should().ThrowAsync<ArgumentException>()
                    .Where( e => e.Message.StartsWith( "Trait context mismatch." ) );

            }
        }

    }
}
