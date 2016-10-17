using System;
using System.Collections.Specialized;

// this is part of <Microsoft SQL Server Tools>.
// for full licensing details, please go to https://github.com/ch3plusStudio/MSSQLTools/blob/master/LICENSE.md

namespace MSSQLTools
{
    class UserDefinedFunction : DBProgram
    {
        private Microsoft.SqlServer.Management.Smo.UserDefinedFunction udf = null;

        private UserDefinedFunction() { }

        public UserDefinedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction udf)
        {
            this.udf = udf;
        }

        public override bool IsEncrypted
        {
            get { return udf.IsEncrypted; }
            set { udf.IsEncrypted = value; }
        }

        public override string Name
        {
            get { return udf.Name; }
        }

        public override Type ObjectType
        {
            get { return Type.UserDefinedFunction; }
        }

        public override StringCollection Script()
        {
            return udf.Script();
        }

        public override String ScriptHeader(bool forAlter)
        {
            return udf.ScriptHeader(forAlter);
        }

        public override bool IsSystemObject
        {
            get { return udf.IsSystemObject; }
        }

        public override bool TextMode
        {
            get { return udf.TextMode; }
            set { udf.TextMode = value; }
        }

        public override void Alter() { udf.Alter(); }
    }
}
