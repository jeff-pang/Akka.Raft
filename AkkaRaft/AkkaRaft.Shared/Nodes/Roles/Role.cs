using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes.Roles
{
    public abstract class Role
    {
        protected RoleContext Context { get; private set; }
        public abstract string GetName();

        public Role(RoleContext ctx)
        {
            Context = ctx;
            Context.OnRoleChanged?.Invoke(GetName());
            Log.Information("{0}", $"Node is now {GetName()}");
        }
    }
}