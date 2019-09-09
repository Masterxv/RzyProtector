namespace Confuser.Renamer
{
    using Confuser.Core;
    using Confuser.Core.Services;
    using dnlib.DotNet;
    using System;

    public interface INameService
    {
        void AddReference<T>(T obj, INameReference<T> reference);
        void Analyze(IDnlibDef def);
        bool CanRename(object obj);
        T FindRenamer<T>();
        RenameMode GetRenameMode(object obj);
        VTableStorage GetVTables();
        void MarkHelper(IDnlibDef def, IMarkerService marker, ConfuserComponent parentComp);
        string ObfuscateName(string name, RenameMode mode);
        string RandomName();
        string RandomName(RenameMode mode);
        void ReduceRenameMode(object obj, RenameMode val);
        void RegisterRenamer(IRenamer renamer);
        void SetCanRename(object obj, bool val);
        void SetOriginalName(object obj, string name);
        void SetOriginalNamespace(object obj, string ns);
        void SetRenameMode(object obj, RenameMode val);
    }
}

