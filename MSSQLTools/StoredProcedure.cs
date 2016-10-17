using System;
using System.Collections.Specialized;

// this is part of <Microsoft SQL Server Tools>.
// for full licensing details, please go to https://github.com/ch3plusStudio/MSSQLTools/blob/master/LICENSE.md

namespace MSSQLTools
{
    class StoredProcedure : DBProgram
    {
        private Microsoft.SqlServer.Management.Smo.StoredProcedure sp = null;

        private StoredProcedure() { }

        public StoredProcedure(Microsoft.SqlServer.Management.Smo.StoredProcedure sp)
        {
            this.sp = sp;
        }

        public override bool IsEncrypted
        {
            get { return sp.IsEncrypted; }
            set { sp.IsEncrypted = value; }
        }

        public override string Name
        {
            get { return sp.Name; }
        }
        public override Type ObjectType
        {
            get { return Type.StoredProcedure; }
        }

        public override StringCollection Script()
        {
            return sp.Script();
        }

        public override String ScriptHeader(bool forAlter)
        {
            return sp.ScriptHeader(forAlter);
        }

        public override bool IsSystemObject
        {
            get { return sp.IsSystemObject; }
        }

        public override bool TextMode
        {
            get { return sp.TextMode; }
            set { sp.TextMode = value; }
        }

        public override void Alter(){ sp.Alter(); }
    }
}
