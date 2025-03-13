using CK.SqlServer;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using CK.Core;
using Shouldly;
using static CK.Testing.MonitorTestHelper;
using Microsoft.Data.SqlClient;
using System.Data;
using CK.Testing;

namespace CK.DB.SqlCKTrait.Tests;

[TestFixture]
public class RawTraitTests
{
    [Test]
    public void proper_subsets_and_supersets()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int contextId = p.CKTraitContextTable.RegisterContext( ctx, 1, "RawTest", ',' );
            int letterId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "A,B,C,D,E,F" );
            int digitId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "0,1,2,3,4,5,6,7,8,9" );
            int evenDigitId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "0,2,4,6,8" );
            int oddDigitId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "1,3,5,7,9" );
            int[] digitsId = Enumerable.Range( 0, 10 )
                                .Select( i => p.CKTraitTable.FindOrCreate( ctx, 1, contextId, findOnly: true, traitName: i.ToString() ) )
                                .ToArray();
            digitsId.ShouldContain( id => id > 0 );

            using( var cmdSubSet = new SqlCommand( "select CKTraitId from CK.fCKTraitProperSubSet( @Id )" ) )
            using( var cmdSuperSet = new SqlCommand( "select CKTraitId from CK.fCKTraitProperSuperSet( @Id )" ) )
            {
                var pSubSet = cmdSubSet.Parameters.Add( "@Id", SqlDbType.Int );
                var pSuperSet = cmdSuperSet.Parameters.Add( "@Id", SqlDbType.Int );

                pSuperSet.Value = evenDigitId;
                ctx[p].ExecuteReader( cmdSuperSet, row => row.GetInt32( 0 ) )
                      .ShouldBe( new[] { digitId } );

                pSuperSet.Value = oddDigitId;
                ctx[p].ExecuteReader( cmdSuperSet, row => row.GetInt32( 0 ) )
                      .ShouldBe( new[] { digitId } );

                pSubSet.Value = digitId;
                ctx[p].ExecuteReader( cmdSubSet, row => row.GetInt32( 0 ) )
                      .ShouldBe( [evenDigitId, oddDigitId, .. digitsId], ignoreOrder: true );

                for( int idx = 0; idx < 10; ++idx )
                {
                    var atomId = digitsId[idx];
                    pSuperSet.Value = atomId;
                    List<int> superSets = ctx[p].ExecuteReader( cmdSuperSet, row => row.GetInt32( 0 ) );
                    if( idx % 2 == 0 )
                    {
                        superSets.ShouldBe( [evenDigitId, digitId], ignoreOrder: true );
                    }
                    else
                    {
                        superSets.ShouldBe( [oddDigitId, digitId], ignoreOrder: true );
                    }
                    // Since these are atomic trait, there is no SubSet.
                    pSubSet.Value = atomId;
                    ctx[p].ExecuteReader( cmdSubSet, row => row.GetInt32( 0 ) ).ShouldBeEmpty();
                }

            }
        }
    }

    [Test]
    public void subsets_and_supersets()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int contextId = p.CKTraitContextTable.RegisterContext( ctx, 1, "RawTest", ',' );
            int letterId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "A,B,C,D,E,F" );
            int digitId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "0,1,2,3,4,5,6,7,8,9" );
            int evenDigitId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "0,2,4,6,8" );
            int oddDigitId = p.CKTraitTable.FindOrCreate( ctx, 1, contextId, false, "1,3,5,7,9" );
            int[] digitsId = Enumerable.Range( 0, 10 )
                                .Select( i => p.CKTraitTable.FindOrCreate( ctx, 1, contextId, findOnly: true, traitName: i.ToString() ) )
                                .ToArray();
            digitsId.ShouldContain( id => id > 0 );

            using( var cmdSubSet = new SqlCommand( "select CKTraitId from CK.fCKTraitSubSet( @Id )" ) )
            using( var cmdSuperSet = new SqlCommand( "select CKTraitId from CK.fCKTraitSuperSet( @Id )" ) )
            {
                var pSubSet = cmdSubSet.Parameters.Add( "@Id", SqlDbType.Int );
                var pSuperSet = cmdSuperSet.Parameters.Add( "@Id", SqlDbType.Int );

                pSuperSet.Value = evenDigitId;
                ctx[p].ExecuteReader( cmdSuperSet, row => row.GetInt32( 0 ) )
                      .ShouldBe( new[] { digitId, evenDigitId } );

                pSuperSet.Value = oddDigitId;
                ctx[p].ExecuteReader( cmdSuperSet, row => row.GetInt32( 0 ) )
                      .ShouldBe( new[] { digitId, oddDigitId } );

                pSubSet.Value = digitId;
                ctx[p].ExecuteReader( cmdSubSet, row => row.GetInt32( 0 ) )
                      .ShouldBe( [evenDigitId, oddDigitId, digitId, ..digitsId], ignoreOrder: true );

                for( int idx = 0; idx < 10; ++idx )
                {
                    var atomId = digitsId[idx];
                    pSuperSet.Value = atomId;
                    List<int> superSets = ctx[p].ExecuteReader( cmdSuperSet, row => row.GetInt32( 0 ) );
                    if( idx % 2 == 0 )
                    {
                        superSets.ShouldBe( [atomId, evenDigitId, digitId], ignoreOrder: true );
                    }
                    else
                    {
                        superSets.ShouldBe( [atomId, oddDigitId, digitId], ignoreOrder: true );
                    }
                    // Since these are atomic trait, they are necessaily alone.
                    pSubSet.Value = atomId;
                    ctx[p].ExecuteReader( cmdSubSet, row => row.GetInt32( 0 ) ).ShouldBe( [atomId] );
                }

            }


        }
    }
}
