using System;
using System.Collections.Specialized;

// this is part of <Microsoft SQL Server Tools>.
// for full licensing details, please go to https://github.com/ch3plusStudio/MSSQLTools/blob/master/LICENSE.md

namespace MSSQLTools
{
    abstract class DBProgram
    {
        public abstract String Name { get; }

        public enum Type : int { StoredProcedure, UserDefinedFunction}

        public abstract Type ObjectType { get; }

        public abstract String ScriptHeader(bool forAlter);

        public abstract StringCollection Script();

        public abstract bool IsEncrypted { get; set; }

        public abstract bool IsSystemObject { get; }

        public abstract bool TextMode { get; set; }

        public abstract void Alter();
    }
}
