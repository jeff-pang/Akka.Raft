using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaRaft.Shared.Nodes.Roles
{
    public class RoleContext
    {
        public Action<string> OnRoleChanged { get; set; }
        private Role _role;
        // Gets or sets the state

        public Role Role
        {

            get { return _role; }
        }
        public string RoleName { get { return _role.GetName(); } }

        public static RoleContext _ctx;
        public static RoleContext Start(StateEvents nodeEvents)
        {
            if (_ctx == null)
            {
                _ctx = new RoleContext()
                {
                    NodeEvents = nodeEvents                   
                };
                _ctx._role = new FollowerRole(_ctx);
            }
            return _ctx;
        }
        public StateEvents NodeEvents { get; private set; }
        private RoleContext()
        {
        }

        public void Transition<T>() where T : Role
        {
            NodeEvents.Clear();
            _role = (Role)Activator.CreateInstance(typeof(T), this);
        }
    }
}
