using CK.Monitoring;
using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace CK.DB.SqlCKTrait.Tests
{
    public class Program
    {
        public static int Main( string[] args )
        {
            return new AutoRun( Assembly.GetEntryAssembly() )
                    .Execute( args, new ExtendedTextWrapper( Console.Out ), Console.In );
        }
    }
}
